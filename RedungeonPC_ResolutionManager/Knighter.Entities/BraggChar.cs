using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class BraggChar : PlayerEntity
{
	private BraggsParrotEntity parrot;

	private int shotAnim = -1;

	private int muzzleDuration = 7;

	private int shotAnimDuration = 15;

	private int shotCost = 15;

	private bool shooting => shotAnim >= 0;

	public int Keys { get; private set; }

	[Preserve]
	public BraggChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.095f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "bragg_n_", "1234");
		animation.Add("e", "bragg_e_", "1234");
		animation.Add("w", "bragg_w_", "1234");
		animation.Add("s", "bragg_s_", "1234");
		animation.Add("spin", "bragg_fall_", "1111122222");
		AnimateUTurns = false;
		PosShift = new Vector2(-1.5f, -9f);
		ShadowShift = new Vector2(0f, 3f);
		Keys = 0;
	}

	public override void Load()
	{
		if (Abilities.SkillLevel[Skill.Parrot] > 0 && base.playState.Session.Revives == 0)
		{
			SpawnParrot(this, first: true);
		}
		base.Load();
	}

	public void SpawnParrot(Entity spawner, bool first = false)
	{
		parrot = new BraggsParrotEntity((int)Math.Round(spawner.WorldCoordinates.X), (int)Math.Round(spawner.WorldCoordinates.Y), this, first);
		SendMessage(new SpawnEntityMessage(parrot, null));
		if (!first)
		{
			base.playState.Hud.ShowAlert("parrot", __(SId.SKILL_GEM_alert_gem_the_parrot), default(Color).FromRgb(3248683), 120, SpriteName.parrot_front_1);
		}
	}

	public void ParrotEscaped(bool first)
	{
		parrot = null;
		if (!Dead)
		{
			base.playState.Hud.ShowAlert("parrot-escaped", first ? __(SId.SKILL_GEM_alert_gem_escaped) : __(SId.SKILL_GEM_alert_gem_escaped_again), default(Color).FromRgb(3248683), 120, SpriteName.parrot_back_1);
		}
	}

	protected override int CoinMultiplier()
	{
		if (parrot != null)
		{
			return 2;
		}
		return 1;
	}

	public override bool SpawnFallFragments()
	{
		return true;
	}

	public override void CollideWith(Entity other)
	{
		base.CollideWith(other);
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		return base.TryResist(injuryType, offender);
	}

	public override void TryTriggerAbility()
	{
		if (!base.Falling)
		{
			if (base.core.CurrentPlayState.Session.CollectedCoins < shotCost)
			{
				SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, string.Format(__(SId.SKILL_GUNSHOT_alert_cost), shotCost)), CurrentPlatform));
				return;
			}
			base.core.CurrentPlayState.Session.CollectedCoins -= shotCost;
			base.core.ProfileData.Coins -= shotCost;
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, "-^" + shotCost, Color.White, 1.5f), CurrentPlatform));
			shotAnim = 0;
			Light light = base.core.CurrentPlayState.LightManager.AddLight(Color.Gold, 0.8f, 0.4f, this);
			light.Follow(this);
			light.FollowRate = 1f;
			light.ChangeRate = 0.1f;
			light.Radius = 4f;
			light.Intencity = 0.4f;
			light.Die();
			base.playState.Camera.Shake("shot");
			SendMessage(new PlayWorldSoundMessage(SoundName.bragg_shot, base.WorldCenter));
			int num = Abilities.SkillLevel[Skill.Gunshot];
			SendMessage(new SpawnEntityMessage(new ProjectileEntity(base.WorldCenterCoordinates.X - FacingDirection.Y * 0.3f, base.WorldCenterCoordinates.Y, FacingDirection.Clone(), ProjectileEntity.ProjectileType.Bullet).SetKillReward(num switch
			{
				2 => 15, 
				1 => 0, 
				_ => 20, 
			}, this), null));
			_inc(Stat.BraggTimesFired);
			base.TryTriggerAbility();
		}
	}

	public override bool Paralized()
	{
		if (!shooting)
		{
			return base.Paralized();
		}
		return true;
	}

	public override void Update()
	{
		base.playState.Hud.AbilitiesHud.skillPanels[Skill.TreasureHunt].Text = "× " + Keys;
		if (shooting)
		{
			shotAnim++;
			if (shotAnim >= shotAnimDuration)
			{
				shotAnim = -1;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		DrawCustomHUD();
		if (shooting)
		{
			string text = FacingDirection.DirectionId();
			Sprite sprite = _("bragg_" + ((shotAnim < muzzleDuration) ? "muzzle" : "gun") + "_" + text);
			Vector2 vector = new Vector2(0f);
			switch (text)
			{
			case "s":
				vector = new Vector2(-5f, -1f);
				break;
			case "n":
				vector = new Vector2(0f, -14f);
				break;
			case "w":
				vector = new Vector2(-20f, 0f);
				break;
			}
			Vector2 vector2 = base.WorldPosition + PosShift + base.dAnim + vector;
			base.core.Renderer[base.Z].DrawSpriteW(sprite, vector2);
			base.core.Renderer["fg", -3, false].DrawSpriteW(_("bragg_gun_" + text), vector2.Shift(0f, (text == "s") ? 9 : ((text == "n") ? 28 : 16)), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
		}
		else
		{
			base.Draw();
		}
	}

	private void DrawCustomHUD()
	{
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlayWorldSoundMessage(SoundName.bragg_death, base.WorldPosition));
		Animation animation = new Animation();
		animation.Add("spin", "bragg_hat_", "1234");
		animation.Play("spin");
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(-0.1875f, 0f), SpriteName.bragg_hat_1, -1, new Vector4(0f, 0f, 2.5f, 0f)).SetAnim(animation), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_gun), null));
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 3f).OnSpawn(delegate(Particle p)
		{
			p.Velocity = SciHelper.GetRandomVectorInCircle(0.4f);
		}).OnUpdate(delegate(Particle p)
		{
			if (p.Age < 20)
			{
				p.Position += p.Velocity;
				p.Velocity += new Vector2(0f, 0.05f);
			}
			p.Dead = p.Age > 25;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer.DrawDotW(p.Position.X, p.Position.Y - 8f, default(Color).FromRgb(12194836) * ((float)(25 - p.Age) / 25f), 1f);
			})
			.Burst(20);
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		Animation animation = new Animation();
		animation.Add("spin", "bragg_hat_", "1234");
		animation.Play("spin");
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos.Shift(-0.1875f, 0f), SpriteName.bragg_hat_1, -1, new Vector4(0f, 0f, 2.5f, 0f)).SetAnim(animation), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.bragg_gun), null));
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.bragg_shot;
	}

	protected override void UpdateAbilities()
	{
		float num = Abilities.SkillCharge[Skill.Gunshot];
		Abilities.SkillCharge[Skill.Gunshot] = Component._m((float)base.playState.Session.CollectedCoins / (float)shotCost, 1f);
		if (Abilities.SkillCharge[Skill.Gunshot].IsEqualTo(1f) && num < 1f)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.bragg_gun_cock, base.WorldCenter));
		}
	}

	public void CollectKey(int count = 1)
	{
		Keys += count;
	}

	public bool SpendKeys(int number)
	{
		if (Keys < number)
		{
			return false;
		}
		Keys -= number;
		return true;
	}
}
