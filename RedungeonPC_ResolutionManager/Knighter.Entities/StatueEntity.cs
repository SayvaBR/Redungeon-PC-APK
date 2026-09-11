using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class StatueEntity : Entity
{
	public class StatueHitbox : Entity
	{
		private StatueEntity parent;

		public StatueHitbox(float x, float y, StatueEntity parent)
			: base(x, y, 1f, 1f)
		{
			this.parent = parent;
		}

		public override bool IsPassableFor(Entity other)
		{
			if (!(other is FragmentEntity))
			{
				return other.WorldCoordinates.Y.IsEqualTo(base.WorldCoordinates.Y);
			}
			return true;
		}

		public override void CollideWith(Entity other)
		{
			if (other is PlayerEntity playerEntity)
			{
				playerEntity.Hurt(InjuryType.Axe, parent);
			}
			base.CollideWith(other);
		}

		public override void InteractWith(Entity other)
		{
			base.InteractWith(other);
		}

		public override void Break(Entity offender)
		{
			parent.Break(offender);
			base.Break(offender);
		}
	}

	private enum SS
	{
		Sleeping,
		Awake,
		AwakeGreen,
		Aiming,
		Hitting,
		Backing,
		Broken
	}

	private enum SE
	{
		WakeUp,
		WakeUpGreen,
		Hit,
		BackOff,
		FallAsleep,
		Break
	}

	private StateMachine<SS, SE> sm;

	private Animation anim;

	private Vector2 posShift;

	private int dir;

	private Light light;

	private Light axeLight;

	private StatueHitbox box1;

	private StatueHitbox box2;

	private bool living;

	private bool awake
	{
		get
		{
			if (sm.CurrentState == SS.Sleeping || sm.CurrentState == SS.Broken)
			{
				if (sm.CurrentState == SS.Broken)
				{
					if (anim != null)
					{
						return anim.GetCurrentFrameNumber() < 25;
					}
					return false;
				}
				return false;
			}
			return true;
		}
	}

	public bool Unbreakable { get; private set; }

	public StatueEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		anim = new Animation(0.2f, loop: false);
		anim.Add("wake", "statue_", "01234");
		anim.Add("wake-green", "statue_green_", "123");
		anim.Add("aim", "statue_", "5678");
		anim.Add("hit", "statue_", "9ab");
		anim.Add("back", "statue_", "cd4");
		anim.Add("sleep", "statue_", "43210");
		anim.Add("break", "statue_break_", "1111111112223333333333333456");
		anim.Play("wake");
		anim.Pause();
		Unbreakable = desc["unbreakable"] == 1;
		dir = ((desc["dir"] != 0) ? 1 : (-1));
		if (desc.Flipped)
		{
			dir *= -1;
		}
		float num = desc["chance"];
		living = SciHelper.ChanceRoll(num / 10f);
		posShift = new Vector2((dir > 0) ? (-13.5f) : (-32.5f), -18f);
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(6996223), 1.5f, 0.7f, this);
		light.ChangeRate = 0.1f;
		light.FollowRate = 1f;
		light.TargetIntencity = 0f;
		axeLight = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(6996223), 1.5f, 0.7f, this);
		axeLight.ChangeRate = 0.1f;
		axeLight.FollowRate = 1f;
		axeLight.Offset = new Vector2(dir * 16 * 2, 0f);
		axeLight.TargetIntencity = 0f;
		sm = new StateMachine<SS, SE>();
		sm.State(SS.Sleeping).IsInitial().On(SE.WakeUp, SS.Awake)
			.On(SE.WakeUpGreen, SS.AwakeGreen);
		sm.State(SS.Awake).On(SE.Hit, SS.Aiming).On(SE.FallAsleep, SS.Sleeping);
		sm.State(SS.AwakeGreen);
		sm.State(SS.Aiming).After(20 + desc["aim-delay"]).AutoTransitionTo(SS.Hitting);
		sm.State(SS.Hitting).After(60 * desc["hit-delay"]).AutoTransitionTo(SS.Backing);
		sm.State(SS.Backing).After(30).AutoTransitionTo(SS.Awake);
		sm.State(SS.Broken).ForcedOn(SE.Break);
		sm.Start();
	}

	public override void Update()
	{
		anim.Update();
		sm.Update();
		int num = (int)Math.Abs(base.core.CurrentPlayState.Player.WorldCoordinates.Y - base.WorldCoordinates.Y);
		if (base.core.CurrentPlayState.Player.Dead)
		{
			num = 10;
		}
		switch (sm.CurrentState)
		{
		case SS.Sleeping:
			if (num <= 3 && living)
			{
				if (base.core.CurrentPlayState.Player is MedusaChar && !Unbreakable)
				{
					sm.Trigger(SE.WakeUpGreen);
				}
				else
				{
					sm.Trigger(SE.WakeUp);
				}
			}
			break;
		case SS.Awake:
			if (num > 3)
			{
				sm.Trigger(SE.FallAsleep);
			}
			if (num == 0 && !base.core.CurrentPlayState.Player.Dead && (int)(base.core.CurrentPlayState.Player.WorldCoordinates.X - base.WorldCoordinates.X) * dir <= 2)
			{
				sm.Trigger(SE.Hit);
			}
			break;
		}
		if (sm.JustEnteredState)
		{
			switch (sm.CurrentState)
			{
			case SS.Awake:
				if (sm.PrevState == SS.Sleeping)
				{
					anim.Play("wake");
					SendMessage(new PlayWorldSoundMessage(SoundName.statue_wake, base.WorldCenter));
					light.TargetIntencity = 0.7f;
					axeLight.TargetIntencity = 0.7f;
				}
				break;
			case SS.AwakeGreen:
				if (sm.PrevState == SS.Sleeping)
				{
					anim.Play("wake-green");
					SendMessage(new PlayWorldSoundMessage(SoundName.statue_wake, base.WorldCenter));
					light.TargetIntencity = 0.7f;
					axeLight.TargetIntencity = 0.7f;
					light.Color = default(Color).FromRgb(9502015);
					axeLight.Color = default(Color).FromRgb(9502015);
				}
				break;
			case SS.Sleeping:
				anim.Play("sleep");
				SendMessage(new PlayWorldSoundMessage(SoundName.statue_sleep, base.WorldCenter));
				light.TargetIntencity = 0f;
				axeLight.TargetIntencity = 0f;
				break;
			case SS.Aiming:
				anim.Play("aim");
				break;
			case SS.Hitting:
				anim.Play("hit");
				SendMessage(new PlayWorldSoundMessage(SoundName.statue_hit, base.WorldCenter), 15);
				break;
			case SS.Backing:
				anim.Play("back");
				SendMessage(new PlayWorldSoundMessage(SoundName.statue_back, base.WorldCenter));
				break;
			case SS.Broken:
				anim.Play("break");
				SendMessage(new PlayWorldSoundMessage(SoundName.statue_break, base.WorldCenter));
				if (sm.PrevState != SS.Hitting)
				{
					anim.FrameForward(10);
				}
				light.TargetRadius = 0f;
				break;
			}
		}
		if (sm.CurrentState == SS.Hitting && sm.TicksInState == 4)
		{
			box1 = new StatueHitbox(x + (float)(dir * 2), y, this);
			SendMessage(new SpawnEntityMessage(box1, CurrentPlatform));
		}
		if (sm.CurrentState == SS.Hitting && sm.TicksInState == 2)
		{
			box2 = new StatueHitbox(x + (float)dir, y, this);
			SendMessage(new SpawnEntityMessage(box2, CurrentPlatform));
		}
		if (sm.CurrentState == SS.Backing && sm.TicksInState == 5)
		{
			if (box1 != null && !box1.Unloaded)
			{
				SendMessage(new RemoveEntityMessage(box1));
				box1 = null;
			}
			if (box2 != null && !box2.Unloaded)
			{
				SendMessage(new RemoveEntityMessage(box2));
				box2 = null;
			}
		}
		if (awake)
		{
			float num2 = 0f;
			int currentFrameNumber = anim.GetCurrentFrameNumber();
			num2 = sm.CurrentState switch
			{
				SS.Aiming => (currentFrameNumber == 2 || currentFrameNumber == 3) ? 0.9f : 0f, 
				SS.Hitting => (currentFrameNumber == 0) ? 1.2f : ((currentFrameNumber >= 1) ? 1.6f : 1f), 
				SS.Backing => (currentFrameNumber == 0) ? 1.5f : 1f, 
				_ => 1f, 
			};
			axeLight.Offset = new Vector2((float)(dir * 16) * num2, 0f);
		}
		base.Update();
	}

	public override void Break(Entity offender)
	{
		if (!IsBroken && living)
		{
			IsBroken = true;
			sm.Trigger(SE.Break);
			if (box1 != null && !box1.Unloaded)
			{
				SendMessage(new RemoveEntityMessage(box1));
				box1 = null;
			}
			if (box2 != null && !box2.Unloaded)
			{
				SendMessage(new RemoveEntityMessage(box2));
				box2 = null;
			}
			FragmentEntity fragmentEntity = new FragmentEntity(base.WorldCoordinates.Shift((float)dir * 1.5f, 0f), SpriteName.statue_axe_fragment);
			SendMessage(new SpawnEntityMessage(fragmentEntity, null));
			axeLight.Follow(fragmentEntity);
			base.Break(offender);
		}
	}

	public override void Draw()
	{
		Sprite currentFrame = anim.GetCurrentFrame();
		base.core.Renderer[base.Z, !awake].DrawSpriteW(currentFrame, base.WorldPosition + posShift, null, null, 0f, (dir <= 0) ? SpriteFlip.Horizontal : SpriteFlip.None);
		base.core.Renderer["bg", base.Z + 64, false].DrawSpriteW(currentFrame, base.WorldPosition + posShift.Shift(0f, currentFrame.Height - 8), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, (dir < 0) ? (SpriteFlip.Horizontal | SpriteFlip.Vertical) : SpriteFlip.Vertical);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return other is FragmentEntity;
	}
}
