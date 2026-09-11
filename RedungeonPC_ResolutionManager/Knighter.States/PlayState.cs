using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class PlayState : State
{
	protected override bool ShowContextPromptLegend => false;

	public readonly TileMap TileMap;

	public readonly EntityManager EntityManager;

	public readonly LightManager LightManager;

	public readonly SessionData Session;

	public PlayerEntity Player;

	public bool ContinuePending;

	private float playTicks;

	public float WorldSpeed;

	public bool SloMo;

	public bool SloMoAffectsPlayer;

	public float SloMoFactor = 0.3f;

	public bool PreventDeathScreenshots;

	public PlayerControlScheme PlayerControl;

	public GameHud Hud;

	public Camera Camera;

	public LevelGenerator LevelGenerator;

	private ParticleEmitter grueEmitter;

	private Vector2 smoothPlayerPos;

	private Vector2 slowPlayerPos;

	private float TerminatorSpeed;

	private const float initialTerminatorSpeed = 0.15f;

	public float TerminatorTarget;

	public float TerminatorKeepUpDistance = 170f;

	public bool TerminatorDontKeepUp;

	private bool terminatorPaused = true;

	private Animation grue;

	private Vector2 lastPlayerPos;

	private Light terminatorLight;

	public float TerminatorLightModifier = 1f;

	private BagOf<SoundName> monsterSounds;

	private int tillNextMonsterSound = 60;

	private bool terminatorRollback;

	private int lastCountdown;

	private int stutterTimer;

	private const int UnpauseTimerStep = 20;

	private bool fromShop;

	private int shopAnim;

	private const int shopAnimDuration = 70;

	private float playerSpeedTarget;

	private float playerSpeed;

	public float CurrentDistance;

	private string lastModuleId;

	public Dictionary<SpellType, SpellEffect> SpellEffects;

	public bool Started { get; private set; }

	public bool PlayerMoved { get; private set; }

	public Color BackgroundColor => default(Color).FromRgb(0);

	public int WorldTicks => (int)playTicks;

	public bool NewBest { get; private set; }

	public float Terminator { get; private set; }

	public bool Paused { get; private set; }

	public int UnpauseTimer { get; set; }

	private bool shopAnimation
	{
		get
		{
			if (fromShop)
			{
				return shopAnim < 70;
			}
			return false;
		}
	}

	public PlayState(bool fromShop = false)
	{
		base.TransDuration = 20;
		this.fromShop = fromShop;
		Session = new SessionData();
		TileMap = new TileMap();
		EntityManager = new EntityManager();
		ShowCoins = false;
		base.core.Renderer.PostEffectManager.Reset();
		LightManager = new LightManager();
		LevelGenerator = new LevelGenerator(this);
		Camera = new Camera();
		base.core.Renderer.SetCamera(Camera);
		playerSpeed = 0f;
		grue = new Animation(0.15f);
		grue.Add("live", "grue_", "11123332");
		grue.Play("live");
		Terminator = 1000f;
		TerminatorTarget = Terminator;
		TerminatorSpeed = 0.15f;
		monsterSounds = new BagOf<SoundName>().Put(SoundName.critters_roar_1, 2).Put(SoundName.critters_roar_2, 2).Put(SoundName.critters_roar_3, 2)
			.Put(SoundName.critters_roar_4, 2)
			.Put(SoundName.critters_scratch_1)
			.Put(SoundName.critters_scratch_2)
			.Put(SoundName.critters_scratch_3)
			.Put(SoundName.critters_scratch_4);
		SpellEffects = new Dictionary<SpellType, SpellEffect>();
		SpellEffects[SpellType.Poison] = new PoisonEffect(this);
		SpellEffects[SpellType.Ice] = new IceEffect(this);
		SpellEffects[SpellType.Darkness] = new DarknessEffect(this);
		SpellEffects[SpellType.Confusion] = new ConfusionEffect(this);
	}

	public void RefreshControls() => InitControls();

	private void InitControls()
	{
		PlayerControl?.Reset();
		if (base.core.OptionsData.SwipeControl)
		{
			PlayerControl = new PlayerControlSchemeSwipe(this);
		}
		else
		{
			PlayerControl = new PlayerControlSchemeDPad(this, base.core.OptionsData.CompactDPad);
		}
		PlayerControl.Load();
		PlayerControl.UpdateTransition();
	}

	public override void OnReturn()
	{
		InitControls();
		base.OnReturn();
	}

	public override void Load()
	{
		Screen("game");
		Subscribe(MessageType.GameEvent);
		base.core.ParticleManager.KillEmittersInWorld();
		playTicks = 0f;
		WorldSpeed = 0.7f;
		Hud = new GameHud(this);
		Started = false;
		NewBest = false;
		PlayerMoved = false;
		LevelGenerator.Begin();
		Hud.Load();
		InitControls();
		smoothPlayerPos = Player.WorldCenter;
		slowPlayerPos = Player.WorldCenter;
		Terminator = Player.WorldPosition.Y + 150f;
		TerminatorTarget = Terminator;
		lastPlayerPos = Player.WorldPosition;
		terminatorLight = LightManager.AddLight(Color.Red, 20f, 0f);
		terminatorLight.Position = new Vector2(lastPlayerPos.X, Terminator);
		TileMap.Load();
		EntityManager.Load();
		LightManager.Load();
		if (!fromShop)
		{
			Camera.Follow(Player);
		}
		Camera.JumpTo(Player.WorldCenter.Shift(0f, fromShop ? (-500) : 40));
		CreateGrueEmitter();
		base.Load();
	}

	public override void Unload()
	{
		Rumble.Stop();
		LightManager.Unload();
		EntityManager.Unload();
		TileMap.Unload();
		grueEmitter.Kill();
		base.Unload();
	}

	public void StartPlaying()
	{
		if (base.core.ProfileData.ControlsSelectorPending)
		{
			SendMessage(new PushStateMessage(new ControlsSelectorState(quickMode: true)));
			Paused = true;
		}
		Started = true;
		_inc(Stat.Attempts);
		string name = "main";
		Character character = base.core.ProfileData.Character;
		if (character == Character.Bragg)
		{
			name = "pirate";
		}
		base.core.AudioManager.PlayMusic(name);
		MakeGameplayScreenshot(15);
	}

	public void Stutter(int delay)
	{
		stutterTimer = delay;
	}

	public void Pause(bool enteringBackground)
	{
		Rumble.Stop();
		if (!ContinuePending)
		{
			if (!enteringBackground)
			{
				MakeGameplayScreenshot(0, evenIfDead: true);
			}
			EntityManager.Pause();
			Paused = true;
			PlayerControl.Reset();
		}
	}

	public void Unpause(bool useTimer = true)
	{
		Paused = false;
		if (useTimer)
		{
			UnpauseTimer = 60;
		}
	}

	public void ShowRespawnPoint()
	{
		Camera.SetTarget(LevelGenerator.GetRespawnPointCoordinates().Shift(0.5f, 0.5f) * 16f);
	}

	public void Continue(bool countAsRevive = true)
	{
		ContinuePending = false;
		base.core.Renderer.PostEffectManager.Reset();
		if (countAsRevive)
		{
			Session.Revives++;
		}
		else
		{
			Hud.ShowAlert("try-again", "Try again!", Color.DodgerBlue);
			LevelGenerator.ResetFirstModule();
			InitControls();
		}
		int count = 0;
		if (Player is BraggChar)
		{
			count = ((BraggChar)Player).Keys;
		}
		LevelGenerator.RespawnPlayer();
		if (Player is BraggChar)
		{
			((BraggChar)Player).CollectKey(count);
		}
		Camera.Follow(Player);
		terminatorPaused = true;
		TerminatorTarget = Player.WorldPosition.Y + 170f;
		terminatorRollback = true;
	}

	public override void OnLeaveBehind()
	{
		UnpauseTimer = 0;
		PlayerControl.Reset();
	}

	public override void UpdateTransition()
	{
		PlayerControl.UpdateTransition();
		base.UpdateTransition();
	}

	public override void Update()
	{
		if (base.core.CurrentPlayState == null)
		{
			return;
		}
		playerSpeedTarget *= 0.92f;
		playerSpeed += (((playerSpeedTarget > 1f) ? playerSpeedTarget : 0f) - playerSpeed) * 0.03f;
		float value = 1f - Component._m(playerSpeed * 0.15f, 0.15f);
		Camera.ZoomBox.Set("speed", value, inWorld: true);
		PlayerControl.Update();
		if (fromShop && shopAnim < 70)
		{
			shopAnim++;
			if (shopAnim == 10)
			{
				Camera.Follow(Player);
				Player.ShopDrop();
			}
		}
		LightManager.Update();
		if (UnpauseTimer > 0)
		{
			UnpauseTimer--;
			int num = (int)(((float)UnpauseTimer - 0.1f) / 20f) + 1;
			if (num != lastCountdown)
			{
				SendMessage(new PlaySoundMessage(SoundName.countdown_ding, 1f, (num == 1) ? 1f : 0f));
			}
			lastCountdown = num;
			if (UnpauseTimer == 0)
			{
				EntityManager.Resume();
			}
		}
		if (Paused || UnpauseTimer > 0)
		{
			return;
		}
		if (stutterTimer > 0)
		{
			stutterTimer--;
			if (stutterTimer != 0)
			{
				return;
			}
		}
		int num2 = (int)playTicks;
		playTicks += (SloMo ? SloMoFactor : WorldSpeed);
		if ((int)playTicks == num2 && SloMo)
		{
			if (!SloMoAffectsPlayer)
			{
				Player.Update();
				if (Player.HoldingWeb != null)
				{
					Player.HoldingWeb.Update();
				}
			}
			else
			{
				Player.UpdateInSloMo();
			}
			return;
		}
		if (base.core.CurrentPlayState == this)
		{
			LevelGenerator.Update();
			TileMap.Update();
			EntityManager.Update();
		}
		UpdateSpellEffects();
		if (Started)
		{
			Hud.Update();
			grue.Update();
			if (!Player.Dead)
			{
				Session.Ticks++;
				Session.CoinChain.Update();
				_inc(Stat.TicksInGame);
				lastPlayerPos += new Vector2((Player.WorldPosition.X - lastPlayerPos.X) * 0.05f, (Player.WorldPosition.Y - lastPlayerPos.Y) * 0.2f);
			}
			if (!shopAnimation)
			{
				if (!Player.Dead && Terminator > 0f - base.core.Renderer.World.Y - 30f)
				{
					TerminatorTarget -= TerminatorSpeed;
				}
				if (Player.Dead && Session.CauseOfDeath == InjuryType.Timeout)
				{
					TerminatorTarget -= 0.8f;
				}
				if (Player.Dead && Session.CauseOfDeath != InjuryType.Timeout && Terminator < 0f - base.core.Renderer.World.Y + (float)base.core.Renderer.ScreenHeight / Camera.Zoom + 100f)
				{
					TerminatorTarget += TerminatorSpeed * 3f;
				}
				if (!TerminatorDontKeepUp)
				{
					TerminatorTarget = Component._m(TerminatorTarget, Player.WorldPosition.Y + TerminatorKeepUpDistance);
				}
				else if (TerminatorTarget <= Player.WorldPosition.Y + TerminatorKeepUpDistance)
				{
					TerminatorDontKeepUp = false;
				}
			}
			Terminator += (TerminatorTarget - Terminator) * 0.08f;
			if (base.core.ProfileData.Character == Character.PanicBot)
			{
				Terminator = 300f;
			}
			smoothPlayerPos += (Player.WorldCenter - smoothPlayerPos) * 0.1f;
			slowPlayerPos += (Player.WorldCenter - slowPlayerPos) * 0.02f;
			grueEmitter.Position.Y = Terminator;
			grueEmitter.Position.X = slowPlayerPos.X;
			if (Terminator <= Player.WorldPosition.Y + 10f && !terminatorRollback)
			{
				Player.Hurt(InjuryType.Timeout);
			}
			if (Terminator >= Player.WorldPosition.Y + 50f)
			{
				terminatorRollback = false;
			}
			terminatorLight.Position = new Vector2(smoothPlayerPos.X, Terminator);
			float num3 = Terminator - smoothPlayerPos.Y;
			if (num3 < 100f)
			{
				tillNextMonsterSound--;
				if (tillNextMonsterSound <= 0)
				{
					SendMessage(new PlaySoundMessage(monsterSounds.DrawDifferent(), Component._rnd(0.5f, 1f) * (100f - num3) / 100f, 0f, Component._rnd(-0.6f, 0.6f)));
					tillNextMonsterSound = Component._rnd(40, 80);
				}
			}
			if (num3 <= 100f && num3 >= -100f)
			{
				Camera.Shake("terminator", 2f * (100f - Math.Abs(num3)) / 200f);
				Rumble.Pulse("terminator", 0.3f * (100f - Math.Abs(num3)) / 200f, 0f, 6);
				base.core.AudioManager.MusicVolumeBox.Set("darkness", 0.1f + 0.9f * (num3 * num3) / 10000f, inWorld: true, 1f, 1f);
			}
			terminatorLight.TargetIntencity = TerminatorLightModifier * 0.8f * (140f - num3) / 140f;
			if (base.IsTopState && base.TicksInState % 60 == 0)
			{
				MakeGameplayScreenshot();
			}
		}
		base.Update();
	}

	private void UpdateSpellEffects()
	{
		foreach (KeyValuePair<SpellType, SpellEffect> spellEffect in SpellEffects)
		{
			spellEffect.Value?.Update();
		}
	}

	public void MakeGameplayScreenshot(int delay = 0, bool evenIfDead = false)
	{
		if (!PreventDeathScreenshots && base.IsTopState && ((!Player.Dead && !Player.Falling) || evenIfDead))
		{
			SendMessage(new ScreenshotMessage(WhenToTakeScreenshot.WhileDrawing, base.core.GameplayScreenshot), delay);
		}
	}

	public override void HandleInput()
	{
		if (shopAnimation || Player.Dead)
		{
			return;
		}
		bool flag = Hud.HandleInput();
		if (!Paused && UnpauseTimer == 0 && !flag)
		{
			PlayerControl.HandleInput();
		}
		else if (base.core.TouchState.Count > 0 && base.core.TouchState[0].State == TouchLocationState.Pressed)
		{
			UnpauseTimer -= 10;
			if (UnpauseTimer < 0)
			{
				UnpauseTimer = 0;
			}
		}
		base.HandleInput();
	}

	public void Jump(Vector2 direction)
	{
		if (!Paused && UnpauseTimer <= 0 && !Player.Dead && !Player.Falling)
		{
			Player.Jump(direction);
			PlayerMoved = true;
			OnPlayerAction();
		}
	}

	public void OnPlayerAction()
	{
		PlayerMoved = true;
		if (terminatorPaused)
		{
			terminatorPaused = false;
		}
	}

	public void TrackSpeed()
	{
		playerSpeedTarget += 1f;
	}

	public void TrackDistance(float newY)
	{
		int num = -((int)newY - LevelGenerator.SpawnY);
		CurrentDistance = num;
		if (num <= Session.Distance)
		{
			return;
		}
		_inc(Stat.CoveredDistance, num - Session.Distance);
		Session.Distance = num;
		if (num % 50 == 0)
		{
			Hud.ShowAlert("milestone", num + __(SId.MISC_meters), Color.White, 90, null, GameHud.AlertKind.Text);
		}
		if (Session.Distance > base.core.ProfileData.BestDistance && base.core.ProfileData.BestDistance != 0 && !NewBest)
		{
			Hud.ShowAlert("new-best", __(SId.HUD_new_best), TextProfile.OrangeLight, 90, null, GameHud.AlertKind.Text);
			base.core.ParticleManager.AddEmitter(inWorld: true, new Vector2(Player.WorldPosition.X + 8f - (float)(base.core.Renderer.ScreenWidth / 2) - 20f, (LevelGenerator.SpawnY - base.core.ProfileData.BestDistance) * 16), base.core.Renderer.ScreenWidth + 40, 2f).OnSpawn(delegate
			{
			}).OnUpdate(delegate(Particle p)
			{
				p.Position += new Vector2(p.Offset.X / 10f, (0f - Math.Abs(p.Offset.X * p.Offset.X)) / 1000f);
				p.Dead = (float)p.Age > Math.Abs(p.Offset.X * 1.3f);
			})
				.OnDraw(delegate(Particle p)
				{
					float num3 = Math.Abs(p.Offset.X * 1.3f);
					float num4 = (num3 - (float)p.Age) / num3;
					base.core.Renderer.DrawDotW(p.Position.X, p.Position.Y, Color.Orange * 0.3f * num4, 1.7f * num4);
				})
				.Burst(100);
			NewBest = true;
		}
		string moduleIdWithPlayer = LevelGenerator.GetModuleIdWithPlayer();
		if (moduleIdWithPlayer != string.Empty && lastModuleId != moduleIdWithPlayer)
		{
			lastModuleId = moduleIdWithPlayer;
		}
		float num2 = Component._m(1.4f, 1f + 0.3f * (float)num / 200f);
		TerminatorSpeed = 0.15f * num2;
	}

	private void DrawShopTransition()
	{
		if (fromShop && shopAnimation)
		{
			float num = Component._M(shopAnim - 30, 0f) / 40f;
			float num2 = Component._M(shopAnim - 30, 0f) / 40f;
			float num3 = 1f - 0.7f * Component._m(shopAnim, 15f) / 15f;
			base.core.Renderer["fg", -1, false].FillScreen(Color.Black * (1f - num));
			bool flag = num3 > 0.3f;
			Sprite sprite = (flag ? _(CharDescription.Get[base.core.ProfileData.Character].Portrait) : _(CharDescription.Get[base.core.ProfileData.Character].Icon));
			base.core.Renderer["fg"].DrawSpriteS(sprite, base.core.Renderer.ScreenCenter.Shift(0f, (float)base.topSafeArea + (float)base.core.Renderer.ScreenHeight * 0.05f - 2f + num2 * (float)base.core.Renderer.ScreenHeight * 2f) - (flag ? (sprite.Link * num3) : Vector2.Zero), Color.White * (1f - num2), new Vector2(flag ? num3 : (1f / Settings.GuiScale)), 0f, SpriteFlip.None, flag ? SpriteOrigin.TopLeft : SpriteOrigin.BottomCenter);
		}
	}

	public override void Draw()
	{
		base.core.GraphicsDevice.Clear(BackgroundColor);
		if (!fromShop && Transition == TransType.In && base.TicksInState < base.TransDuration)
		{
			float num = 1f - (float)base.Trans / (float)base.TransDuration;
			base.core.Renderer["fg", -1000, false].FillScreen(Color.Black * num);
		}
		DrawShopTransition();
		TileMap.Draw();
		EntityManager.Draw();
		Player?.DrawSpellEffects();
		if (base.IsTopState && Started)
		{
			PlayerControl.Draw();
		}
		Hud.Draw();
		if (!base.core.TakingScreenshot)
		{
			DrawDistanceMark(base.core.ProfileData.BestDistance);
		}
		DrawTerminator();
		if (UnpauseTimer > 0 && !base.core.TakingScreenshot)
		{
			base.core.Renderer["fg", 1, false].FillScreen(Color.Black * 0.6f * ((float)UnpauseTimer / 60f));
			base.core.Renderer["fg", 1, false].DrawTextS(((int)(((float)UnpauseTimer - 0.1f) / 20f) + 1).ToString(), base.core.Renderer.ScreenCenter.Shift(0f, -5f), new TextProfile
			{
				Font = Font.Bold,
				Color = Color.White,
				SecondColor = Color.Black,
				Decoration = TextDecoration.Contour,
				BoxAlignment = Alignment2D.Middle,
				TextAlignment = Alignment2D.Middle,
				Scale = 1.5f + 1.5f * (float)(UnpauseTimer - 1).Mod(20) / 20f
			});
		}
		if (base.core.TakingScreenshot && !Settings.HideScreenshotOverlays)
		{
			float num2 = 0.75f;
			Vector2 vector = new Vector2(base.core.Renderer.ScreenWidth / 2, base.core.Renderer.ScreenHeight - 35);
			Sprite sprite = _(SpriteName.glow_huge);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, vector.Shift(0f, 80f * num2), Color.Black, Vector2.One * num2 * 2.5f, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			sprite = _(SpriteName.title_bat_2);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, vector.Shift(50f * num2, -30f * num2), Color.DimGray, Vector2.One * num2 * 0.8f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			sprite = _(SpriteName.title_re);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, vector, null, Vector2.One * num2, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			sprite = _(SpriteName.title_dungeon);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, vector.Shift(0f, (float)(sprite.Height - 5) * num2), null, Vector2.One * num2, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			sprite = _(SpriteName.title_bat_1);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, vector.Shift(-50f * num2, -15f * num2), Color.Black * 0.2f, Vector2.One * num2, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, vector.Shift(-50f * num2, -25f * num2), null, Vector2.One * num2, 0f, SpriteFlip.None, SpriteOrigin.Center);
			float num3 = 0.75f;
			string text = Session.Distance + __(SId.MISC_meters);
			float num4 = 2f * num3;
			string text2 = text;
			foreach (char c in text2)
			{
				Sprite sprite2 = base.core.SpriteManager.GetSprite("score_digit_" + c);
				num4 += (float)(sprite2.Width - 3) * num3;
			}
			vector = new Vector2(((float)base.core.Renderer.ScreenWidth - num4) / 2f, 10f);
			sprite = _(SpriteName.glow_huge);
			base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite, new Vector2(base.core.Renderer.ScreenWidth / 2, 10f), Color.Black, Vector2.One * 0.75f * 2.5f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			int num5 = 0;
			text2 = text;
			foreach (char c2 in text2)
			{
				Sprite sprite3 = base.core.SpriteManager.GetSprite("score_digit_" + c2);
				base.core.Renderer["fg", 9999, false].DrawSpriteS(sprite3, vector, null, new Vector2(num3));
				vector = vector.Shift((float)(sprite3.Width - 3) * num3, 0f);
				num5++;
			}
		}
		base.Draw();
	}

	private void CreateGrueEmitter()
	{
		bool portrait = Settings.IsTouchDevice && base.core.Renderer.ScreenHeight > base.core.Renderer.ScreenWidth;
		grueEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, new Vector2(Player.WorldCenter.X, Terminator), 1f).OnSpawn(delegate(Particle p)
		{
			int num = Component._rnd(0, 35);
			p.Aux.X = ((num >= 15) ? ((num < 15) ? 1 : 2) : 0);
			p.Aux.Y = Component._rnd(120, 240);
			float num2 = (portrait ? 46f : 60f) + p.Aux.X * (portrait ? 10f : 15f);
			p.Offset = new Vector2(Component._rnd(0f - num2, num2), (num2 > 0f) ? (-5f + Component._rnd(p.Aux.X * 10f, 20f + p.Aux.X * 25f)) : (-5f + Component._rnd(-20f + p.Aux.X * 25f, 20f + p.Aux.X * 25f)));
			p.Position = p.Offset;
		}).OnUpdate(delegate(Particle p)
		{
			float num = 0f - (smoothPlayerPos.Y - Terminator);
			p.Position = ((num > 0f) ? grueEmitter.Position : smoothPlayerPos) + p.Offset;
			p.Dead = (float)p.Age > p.Aux.Y;
		})
			.OnDraw(delegate(Particle p)
			{
				float num = ((p.Age < 15) ? ((float)p.Age / 15f) : (((float)p.Age > p.Aux.Y - 15f) ? ((p.Aux.Y - (float)p.Age) / 15f) : 1f));
				Vector2 vector = slowPlayerPos - p.Position;
				float num2 = 0f - (smoothPlayerPos.Y - Terminator);
				float num3 = Component._M(90f + num2, 0f) / 90f - 0.5f;
				float num4 = ((num2 < 0f) ? 1f : (1f / (float)Math.Exp(num2 * num2 / 35f)));
				float grueVisibility = portrait ? 0.62f : 1f;
				float grueScale = portrait ? 0.78f : 1f;
				base.core.Renderer[1000].DrawSpriteW(grue.GetFrame((int)((float)p.Age * 0.2f % 8f)), p.Position - vector * num3 * 0.2f * (2f - p.Aux.X) + vector * num4 * 0.8f + vector * ((70f - vector.LengthSquared()) / 70f) * 0.01f * (2f - p.Aux.X) + vector * 0.01f * Component._sin((float)p.Age * 0.3f) - vector * (1f - num) * 0.2f, Color.White * (num * (1f - p.Aux.X * 0.1f) * grueVisibility), rotation: 0f - (float)Math.Atan2(p.Position.X - smoothPlayerPos.X, p.Position.Y - smoothPlayerPos.Y) + 0.1f * Component._sin((float)p.Age * 0.2f * (1f + p.Aux.X)), scale: new Vector2(1f - p.Aux.X * 0.1f) * num * grueScale, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			});
		grueEmitter.Max(30);
		grueEmitter.Start(5, 10);
	}

	private void DrawTerminator()
	{
		if (Started)
		{
			int num = 23;
			base.core.Renderer[1000].DrawRectangleW(new RectangleF(base.core.Renderer.ToWorld(Vector2.Zero).X - 10f, Terminator + (float)num, (float)base.core.Renderer.ScreenWidth / Camera.Zoom + 20f, (float)base.core.Renderer.ScreenHeight / Camera.Zoom - Terminator - base.core.Renderer.World.Y + 10f), Color.Black);
			base.core.Renderer[1000].DrawDarknessWave(Terminator + num, WorldTicks * 0.35f);
		}
	}

	private void DrawDistanceMark(int distance)
	{
		if (distance != 0 && (base.core.GetCurrentState() == this || Session.Distance < base.core.ProfileData.BestDistance) && distance >= Session.Distance)
		{
			int num = (LevelGenerator.SpawnY - distance) * 16;
			float y = base.core.Renderer.ToScreen(new Vector2(0f, num)).Y;
			base.core.Renderer["bg", 10, false].DrawRectangleS(new Vector2(0f, y), base.core.Renderer.ScreenWidth, 1f, Color.Orange * 0.5f);
			Renderer renderer = base.core.Renderer[1000];
			string text = string.Format(__(SId.HUD_best_m), distance);
			Vector2 position = new Vector2(5f, y + 3f - 16f);
			TextProfile orangeBoldText = TextProfile.OrangeBoldText;
			Font? font = Font.Thin;
			TextDecoration? textDecoration = TextDecoration.None;
			renderer.DrawTextS(text, position, orangeBoldText.Alter(Color.Orange * 0.5f, null, textDecoration, textAlignment: Alignment2D.Left, boxAlignment: Alignment2D.Left, width: 500, height: null, font: font, scale: 0.75f));
		}
	}

	public override void OnBackButtonPressed()
	{
		// B/Escape é cancelar/voltar. Durante gameplay não existe uma tela
		// anterior para voltar e, portanto, o comando não pode abrir a pausa.
		// A pausa é responsabilidade exclusiva de Start e do botão de pausa HUD.
		base.OnBackButtonPressed();
	}

	public override void OnStartButtonPressed()
	{
		Hud.TryToPause();
		base.OnStartButtonPressed();
	}
}
