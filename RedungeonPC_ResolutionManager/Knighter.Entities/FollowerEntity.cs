using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FollowerEntity : Entity, IPushableEntity
{
	private Animation shadow;

	private Animation bone1;

	private Animation bone2;

	private int riseTimer;

	private int riseDuration = 25;

	private int riseTick = -1;

	private int delay = 40;

	private Light light;

	private Sprite skullSprite;

	private Queue<Vector2> path;

	private Vector2 lastPos;

	private Vector2 nextPos;

	private Vector2 lastPlayerPos;

	private int stepT;

	private int stepD = 25;

	private int ttl = -1;

	private FollowerPadEntity homePad;

	private int untilPeek = 30;

	private int peekD = 60;

	private int peekT = -1;

	private float arrowR;

	private float arrowA;

	public FollowerKind Kind { get; private set; }

	public bool Awake { get; private set; }

	public bool Dead { get; private set; }

	public bool Important { get; private set; }

	public FollowerEntity(int x, int y, TileDesc desc, FollowerKind kind = FollowerKind.Blue, int distance = 0)
		: base((float)x + 0.5f - 0.1f, (float)y + 0.5f - 0.1f, 0.2f, 0.2f)
	{
		Kind = kind;
		Awake = false;
		skullSprite = ((kind == FollowerKind.Blue) ? _(SpriteName.follower_skull_sw) : _(SpriteName.follower_skull_s_y));
		shadow = new Animation(0.15f);
		shadow.Add("wave", "follower_shadow_", "12345678");
		shadow.Play("wave");
		bone1 = new Animation(0.15f);
		bone1.Add("spin", "follower_bone_", "1234");
		bone1.Play("spin");
		bone2 = new Animation(0.15f);
		bone2.Add("spin", "follower_bone_", "4123");
		bone2.Play("spin");
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb((Kind == FollowerKind.Blue) ? 6996223 : 16711680), 1.5f, 0.7f, this);
		light.ChangeRate = 0.1f;
		light.FollowRate = 1f;
		light.TargetIntencity = 0f;
		path = new Queue<Vector2>();
		if (kind == FollowerKind.Red)
		{
			if (distance < 50)
			{
				delay = 40;
				stepD = 30;
				ttl = 80;
			}
			else if (distance < 150)
			{
				delay = 20;
				stepD = 20;
				ttl = 100;
			}
			else if (distance < 200)
			{
				delay = 10;
				stepD = 15;
				ttl = 150;
			}
			else
			{
				delay = 1;
				stepD = 10;
				ttl = 150;
			}
			return;
		}
		if (distance < 50)
		{
			delay = 30;
			stepD = 40;
		}
		else if (distance < 150)
		{
			delay = 20;
			stepD = 30;
		}
		else
		{
			delay = 20;
			stepD = 20;
		}
		int num = desc["delay"];
		if (num >= 0)
		{
			delay = num;
		}
		int num2 = desc["ticks-per-tile"];
		if (num2 >= 0)
		{
			stepD = num2;
		}
		Important = desc["important"] == 1;
	}

	public override void Load()
	{
		homePad = new FollowerPadEntity(Convert.ToInt32(x), Convert.ToInt32(y), Kind == FollowerKind.Red);
		SendMessage(new SpawnEntityMessage(homePad, CurrentPlatform));
		homePad.Active = true;
		homePad.Taken = true;
		base.Load();
	}

	private void SetSpriteFromAngle(double angle)
	{
		angle %= 360.0;
		if (angle > 180.0)
		{
			angle -= 360.0;
		}
		string text = ((Kind == FollowerKind.Blue) ? "" : "_y");
		string text2 = "";
		float num = 22.5f;
		if (angle < (double)(1f * num) && angle >= (double)(-1f * num))
		{
			text2 = "n";
		}
		else if (angle < (double)(3f * num) && angle >= (double)(1f * num))
		{
			text2 = "nw";
		}
		else if (angle < (double)(5f * num) && angle >= (double)(3f * num))
		{
			text2 = "w";
		}
		else if (angle < (double)(7f * num) && angle >= (double)(5f * num))
		{
			text2 = "sw";
		}
		else if (angle < (double)(-7f * num) || angle >= (double)(7f * num))
		{
			text2 = "s";
		}
		else if (angle < (double)(-5f * num) && angle >= (double)(-7f * num))
		{
			text2 = "se";
		}
		else if (angle < (double)(-3f * num) && angle >= (double)(-5f * num))
		{
			text2 = "e";
		}
		else if (angle < (double)(-1f * num) && angle >= (double)(-3f * num))
		{
			text2 = "ne";
		}
		skullSprite = _("follower_skull_" + text2 + text);
	}

	public override void Update()
	{
		PlayerEntity player = base.core.CurrentPlayState.Player;
		if (!Dead && Awake && player.Unloaded && (ttl > 50 || ttl < 0))
		{
			ttl = 50;
			return;
		}
		if (Dead)
		{
			if (base.Flying)
			{
				SetFlying(value: false);
				CurrentPlatform = homePad.CurrentPlatform;
				x -= base.Origin.X;
				y -= base.Origin.Y;
			}
			if (riseTimer > 0)
			{
				Vector2 vector = base.WorldCenterCoordinates - player.WorldCenterCoordinates;
				double num = Math.Atan2(vector.X, vector.Y) * 180.0 / Math.PI;
				int num2 = ((Kind == FollowerKind.Blue) ? 135 : 180);
				num = (double)num2 + (double)((float)riseTimer / (float)riseDuration) * (num - (double)num2);
				SetSpriteFromAngle(num);
				riseTimer--;
				if (riseTimer == 0)
				{
					Awake = false;
					light.TargetIntencity = 0f;
					SendMessage(new PlayWorldSoundMessage(SoundName.follower_chase, base.WorldCenter));
				}
				if (!(homePad.CenterCoordinates - base.CenterCoordinates).IsEqualTo(Vector2.Zero))
				{
					x += (homePad.CenterCoordinates.X - 0.1f - x) * 0.1f;
					y += (homePad.CenterCoordinates.Y - 0.1f - y) * 0.1f;
					UpdateTiles();
				}
			}
		}
		else if (Awake)
		{
			if (!base.Flying)
			{
				SetFlying(value: true);
			}
			if (player.WorldTile != null && !player.WorldTile.WorldCoordinates.IsEqualTo(lastPlayerPos))
			{
				lastPlayerPos = player.WorldTile.WorldCoordinates.Clone();
				path.Enqueue(lastPlayerPos.Clone());
			}
			shadow.Update();
			bone1.Update();
			bone2.Update();
			if (riseTimer < riseDuration)
			{
				riseTimer++;
				if (riseTimer == riseDuration)
				{
					riseTick = base.worldTicks;
				}
			}
			else if (delay > 0)
			{
				delay--;
			}
			else
			{
				if (ttl > 0)
				{
					ttl--;
					if (ttl == 0)
					{
						Break(null);
					}
				}
				bool flag = true;
				if (stepT == stepD)
				{
					stepT = 0;
					lastPos = base.WorldTile.WorldCoordinates.Clone();
					if (path.Count > 0)
					{
						nextPos = path.Dequeue();
					}
					else
					{
						flag = false;
					}
				}
				if (flag && base.levelMap[nextPos] != null && base.levelMap[nextPos].IsPassableFor(this))
				{
					float num3 = (float)stepT / (float)stepD;
					x = lastPos.X + (nextPos.X - lastPos.X) * num3 + 0.4f;
					y = lastPos.Y + (nextPos.Y - lastPos.Y) * num3 + 0.4f;
					UpdateTiles();
					stepT++;
				}
			}
			Vector2 vector2 = base.WorldCenterCoordinates - player.WorldCenterCoordinates;
			double num4 = Math.Atan2(vector2.X, vector2.Y) * 180.0 / Math.PI;
			if (riseTimer < riseDuration)
			{
				int num5 = ((Kind == FollowerKind.Blue) ? 135 : 180);
				num4 = (double)num5 + (double)((float)riseTimer / (float)riseDuration) * (num4 - (double)num5);
			}
			SetSpriteFromAngle(num4);
		}
		else if (Kind == FollowerKind.Blue)
		{
			float num6 = (base.WorldCenterCoordinates - player.WorldCenterCoordinates).LengthSquared();
			light.TargetIntencity = ((num6 < 8f && !player.Dead) ? 0.7f : 0f);
			if (num6 <= 1f && !player.Paralized())
			{
				Awake = true;
				SetFlying(value: true);
				homePad.Taken = false;
				homePad.Active = false;
				SendMessage(new PlayWorldSoundMessage(SoundName.follower_rise, base.WorldCenter));
				lastPos = base.WorldTile.WorldCoordinates.Clone();
				if (player.WorldTile != null)
				{
					lastPlayerPos = player.WorldTile.WorldCoordinates.Clone();
					nextPos = lastPlayerPos.Clone();
					path.Enqueue(nextPos);
					stepT = stepD;
				}
				light.TargetIntencity = 0.8f;
				light.Intencity = 2f;
				light.TargetRadius = 2f;
				light.Radius = 3f;
			}
		}
		else if (peekT < 0)
		{
			untilPeek--;
			if (untilPeek <= 0)
			{
				untilPeek = 60;
				peekT = 0;
				SendMessage(new PlayWorldSoundMessage(SoundName.follower_peek, base.WorldCenter));
				light.TargetIntencity = 1f;
			}
		}
		else
		{
			peekT++;
			if (peekT == peekD - 5)
			{
				light.TargetIntencity = 0f;
			}
			if (peekT >= peekD)
			{
				peekT = -1;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (!Awake)
		{
			switch (Kind)
			{
			case FollowerKind.Blue:
				base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.follower_sleeping), base.WorldCenter.Shift(-1f, -3f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.follower_sleeping_eyes), base.WorldCenter.Shift(-1f, -3f), Color.White * (light.Intencity * 2f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(_(SpriteName.follower_sleeping), base.WorldPosition.Shift(-1f, 0f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical, SpriteOrigin.Center);
				break;
			case FollowerKind.Red:
			{
				Sprite sprite = _(SpriteName.chest_1);
				if (peekT >= 0)
				{
					sprite = _((peekT < 5 || peekT > peekD - 5) ? SpriteName.chest_follower_1 : SpriteName.chest_follower_2);
				}
				base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldPosition.Shift(0.5f, 6.5f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
				base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldPosition.Shift(-11.5f, -4.5f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
				break;
			}
			}
		}
		else
		{
			PlayerEntity player = base.core.CurrentPlayState.Player;
			if (!player.Dead && !base.core.TakingScreenshot)
			{
				Vector2 vector = player.WorldCenter - base.WorldCenter;
				float num = (float)Math.Atan2(vector.X, 0f - vector.Y);
				arrowR += (num - arrowR) * 0.1f;
				float num2 = ((0f - vector.Y >= 48f) ? 1f : 0f);
				arrowA += (num2 - arrowA) * 0.1f;
				base.core.Renderer["fg", -1, false].DrawSpriteW(_((Kind == FollowerKind.Red) ? SpriteName.follower_alert_red : SpriteName.follower_alert_blue), player.WorldCenter + player.dAnim, Color.White * arrowA, Vector2.One * arrowA, arrowR, SpriteFlip.None, SpriteOrigin.Center);
			}
			float num3 = (float)riseTimer / (float)riseDuration;
			float num4 = 4f + num3 * (8f + ((riseTick < 0) ? 0f : (Component._sin((float)(base.worldTicks - riseTick) * 0.17f) * num3 * 0.5f)));
			float num5 = ((riseTick > 0) ? 1f : (0.9f + 0.1f * Component._m((float)riseTimer / 20f, 1f)));
			Vector2 vector2 = ((ttl > 120) ? Vector2.Zero : SciHelper.GetRandomVectorInCircle(3f * (float)(120 - ttl) / 120f));
			base.core.Renderer[base.Z + 16].DrawSpriteW(shadow.GetCurrentFrame(), base.WorldCenter.Shift(-0.5f, 0f - num4 + 5.5f), default(Color).FromRgb((Kind == FollowerKind.Blue) ? 7445979 : 16711680) * num3, Vector2.One * num5, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer[base.Z + 16].DrawSpriteW(bone1.GetCurrentFrame(), base.WorldCenter.Shift(-3f, (0f - num4) * (0.2f + 0.15f * Component._sin((float)base.worldTicks * 0.07f))), null, Vector2.One * num5, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer[base.Z + 16].DrawSpriteW(bone2.GetCurrentFrame(), base.WorldCenter.Shift(3f, (0f - num4) * (0.2f - 0.15f * Component._sin((float)base.worldTicks * 0.07f))), null, Vector2.One * num5, 0f, SpriteFlip.Horizontal, SpriteOrigin.Center);
			base.core.Renderer[base.Z + 16].DrawSpriteW(skullSprite, base.WorldCenter.Shift(0f, 0f - num4), null, Vector2.One * num5, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (ttl >= 0 && !base.core.TakingScreenshot && delay == 0)
			{
				base.core.Renderer[base.Z + 16].DrawSpriteW(skullSprite, base.WorldCenter.Shift(0f, 0f - num4) + vector2, Color.White * 0.5f, Vector2.One * num5, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		base.Draw();
	}

	public override void InteractWith(Entity other)
	{
		if (Kind != FollowerKind.Red || Awake || Dead)
		{
			return;
		}
		if (other is PlayerEntity playerEntity)
		{
			Awake = true;
			SendMessage(new PlayWorldSoundMessage(SoundName.follower_rise, base.WorldCenter));
			lastPos = base.WorldTile.WorldCoordinates.Clone();
			if (playerEntity.WorldTile != null)
			{
				lastPlayerPos = playerEntity.WorldTile.WorldCoordinates.Clone();
				nextPos = lastPlayerPos.Clone();
				path.Enqueue(nextPos);
				stepT = stepD;
			}
			light.TargetIntencity = 0.8f;
			light.Intencity = 2f;
			light.TargetRadius = 2f;
			light.Radius = 3f;
			SpawnBoards();
		}
		base.InteractWith(other);
	}

	public void SpawnBoards()
	{
		homePad.Taken = false;
		SendMessage(new PlayWorldSoundMessage(SoundName.chest_break, base.WorldCenter));
		for (int i = 1; i <= 4; i++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_board_1), null));
		}
		for (int j = 1; j <= 3; j++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_board_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_board_2), null));
		}
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_lid, 60, new Vector4(Component._rnd(-0.1f, 0.1f), Component._rnd(-0.1f, 0.1f), 2.5f, 0.02f), "", 19f), null));
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		SendMessage(new RemoveEntityMessage(this));
		SendMessage(new PlayWorldSoundMessage(SoundName.follower_death, base.WorldCenter));
		FragmentEntity fragmentEntity = new FragmentEntity(base.WorldCenterCoordinates, SpriteName.follower_skull_dead, 60, new Vector4(Component._rnd(-0.15f, 0.15f), Component._rnd(-0.15f, 0.15f), 0.2f, 0.02f), "", 19f);
		SendMessage(new SpawnEntityMessage(fragmentEntity, null));
		light.Follow(fragmentEntity);
		light.TargetIntencity = 0f;
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.follower_bone_1), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.follower_bone_1), null));
		if (Kind == FollowerKind.Red && !Awake)
		{
			SpawnBoards();
		}
		if (Kind == FollowerKind.Red)
		{
			SpawnCoins(offender as IchitakaChar);
		}
		if (offender is PlayerEntity)
		{
			_inc(Stat.FollowersKilled);
		}
		base.Break(offender);
	}

	public void SpawnCoins(IchitakaChar ichitaka)
	{
		if (!base.core.CurrentPlayState.Player.Dead)
		{
			int num = (int)Component._m(30f, base.core.CurrentPlayState.LevelGenerator.AvgCoinValue() * 15);
			int num2 = num;
			int num3 = 0;
			num3 += num / 4;
			num %= 4;
			num3 += num / 3;
			num %= 3;
			num3 += num / 2;
			num %= 2;
			num3 += num;
			for (int i = 0; i < num3; i++)
			{
				int num4 = ((num2 >= 4) ? 4 : ((num2 >= 3) ? 3 : ((num2 < 2) ? 1 : 2)));
				ItemEntity itemEntity = new ItemEntity(base.WorldCenterCoordinates.X + 0.5f * Component._cos((float)i * ((float)Math.PI * 2f / (float)num3)) - 0.5f, base.WorldCenterCoordinates.Y + 0.5f * Component._sin((float)i * ((float)Math.PI * 2f / (float)num3)) - 0.5f, ItemEntity.ValueToType(num4));
				num2 -= num4;
				SendMessage(new SpawnEntityMessage(itemEntity, null), (ichitaka == null) ? (i * 5) : 0);
				itemEntity.SetTarget(base.core.CurrentPlayState.Player, 40);
				ichitaka?.AddDancingCoin(itemEntity);
			}
		}
	}

	public override bool IsPassableFor(Entity other)
	{
		if (IsBroken) return true;
		FollowerEntity followerEntity = other as FollowerEntity;
		if (!(other is FragmentEntity) && (!Awake || followerEntity != null || delay != 0 || Dead) && (Awake || followerEntity == null))
		{
			if (followerEntity != null)
			{
				if (!followerEntity.Dead)
				{
					return !followerEntity.Awake;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override void CollideWith(Entity other)
	{
		if (!Awake || Dead)
		{
			return;
		}
		if (other is PlayerEntity playerEntity)
		{
			playerEntity.Hurt(InjuryType.Follower, this);
		}
		if (other is FollowerPadEntity followerPadEntity && Kind == FollowerKind.Blue && followerPadEntity.Active && !followerPadEntity.Taken)
		{
			followerPadEntity.Taken = true;
			path.Clear();
			Dead = true;
			homePad = followerPadEntity;
			followerPadEntity.Active = false;
			if (followerPadEntity.WorldTile == base.core.CurrentPlayState?.Player?.WorldTile)
			{
				base.core.CurrentPlayState.Player.Hurt(InjuryType.Follower, this);
			}
		}
		if ((other is BoltEntity || other is RotobladeEntity) && !other.IsBroken)
		{
			Break(other);
		}
		if (other is BatEntity || other is SlimeEntity)
		{
			other.Break(this);
		}
		base.CollideWith(other);
	}

	public void Crash(Entity offender)
	{
		if (!IsBroken)
		{
			Break(offender);
		}
	}
}
