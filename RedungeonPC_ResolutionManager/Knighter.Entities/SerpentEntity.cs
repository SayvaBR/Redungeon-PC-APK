using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SerpentEntity : Entity
{
	public enum SerpentPart
	{
		Head,
		Rib,
		Spine,
		Tail1,
		Tail2
	}

	public static BagOf<SoundName> Sounds;

	private int idleSoundDelay;

	private Vector2 lastCenter;

	private float angle;

	private float angleT;

	private string dirString;

	private int chainN;

	public SerpentPart Part;

	public SerpentEntity Prev;

	public SerpentEntity Next;

	public int BreakT = -1;

	public int BreakD = 3;

	private Light light;

	private bool avoidPlayer;

	private float avoid;

	private float avoidTarget;

	public bool Petrified;

	private string lastSprite;

	public bool IsChineseDragon;

	static SerpentEntity()
	{
		Sounds = new BagOf<SoundName>().Put(SoundName.serpent_move_1).Put(SoundName.serpent_move_2).Put(SoundName.serpent_move_3)
			.Put(SoundName.serpent_move_4)
			.Put(SoundName.serpent_move_5);
	}

	public SerpentEntity(Entity parent, float x, float y, int chainN, bool isDragon)
		: base(x, y, 0.1f, 0.1f)
	{
		lastCenter = Vector2.Zero;
		this.chainN = chainN;
		IsChineseDragon = isDragon;
		Part = ((chainN != 0) ? ((chainN % 2 == 0) ? SerpentPart.Rib : SerpentPart.Spine) : SerpentPart.Head);
	}

	public override void Load()
	{
		if (!IsChineseDragon)
		{
			if (Part == SerpentPart.Head)
			{
				light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(9563694), 2f, 0.7f, this);
				light.ChangeRate *= 0.5f;
			}
		}
		else
		{
			light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16775010), 2f, (Part == SerpentPart.Head) ? 0.7f : 0.15f, this);
			light.ChangeRate *= 0.5f;
		}
		avoid = 0f;
		avoidTarget = 0f;
		PlayerEntity playerEntity = base.core.CurrentPlayState?.Player;
		avoidPlayer = playerEntity != null && playerEntity is RibChar;
		avoidPlayer |= IsChineseDragon;
		base.Load();
	}

	private string DirectionFromAngle(double angle)
	{
		angle %= 360.0;
		if (angle > 180.0)
		{
			angle -= 360.0;
		}
		if (angle < -180.0)
		{
			angle += 360.0;
		}
		string result = "";
		float num = 22.5f;
		if (angle < (double)(1f * num) && angle >= (double)(-1f * num))
		{
			result = "n";
		}
		else if (angle < (double)(3f * num) && angle >= (double)(1f * num))
		{
			result = "nw";
		}
		else if (angle < (double)(5f * num) && angle >= (double)(3f * num))
		{
			result = "w";
		}
		else if (angle < (double)(7f * num) && angle >= (double)(5f * num))
		{
			result = "sw";
		}
		else if (angle < (double)(-7f * num) || angle >= (double)(7f * num))
		{
			result = "s";
		}
		else if (angle < (double)(-5f * num) && angle >= (double)(-7f * num))
		{
			result = "se";
		}
		else if (angle < (double)(-3f * num) && angle >= (double)(-5f * num))
		{
			result = "e";
		}
		else if (angle < (double)(-1f * num) && angle >= (double)(-3f * num))
		{
			result = "ne";
		}
		return result;
	}

	private string DirectionFromAngle16(double angle)
	{
		angle %= 360.0;
		if (angle > 180.0)
		{
			angle -= 360.0;
		}
		if (angle < -180.0)
		{
			angle += 360.0;
		}
		string result = "";
		float num = 11.25f;
		if (angle < (double)(1f * num) && angle >= (double)(-1f * num))
		{
			result = "n";
		}
		else if (angle < (double)(3f * num) && angle >= (double)(1f * num))
		{
			result = "nnw";
		}
		else if (angle < (double)(5f * num) && angle >= (double)(3f * num))
		{
			result = "nw";
		}
		else if (angle < (double)(7f * num) && angle >= (double)(5f * num))
		{
			result = "nww";
		}
		else if (angle < (double)(9f * num) && angle >= (double)(7f * num))
		{
			result = "w";
		}
		else if (angle < (double)(11f * num) && angle >= (double)(9f * num))
		{
			result = "sww";
		}
		else if (angle < (double)(13f * num) && angle >= (double)(11f * num))
		{
			result = "sw";
		}
		else if (angle < (double)(15f * num) && angle >= (double)(13f * num))
		{
			result = "ssw";
		}
		else if (angle < (double)(-15f * num) || angle >= (double)(15f * num))
		{
			result = "s";
		}
		else if (angle < (double)(-13f * num) && angle >= (double)(-15f * num))
		{
			result = "sse";
		}
		else if (angle < (double)(-11f * num) && angle >= (double)(-13f * num))
		{
			result = "se";
		}
		else if (angle < (double)(-9f * num) && angle >= (double)(-11f * num))
		{
			result = "see";
		}
		else if (angle < (double)(-7f * num) && angle >= (double)(-9f * num))
		{
			result = "e";
		}
		else if (angle < (double)(-5f * num) && angle >= (double)(-7f * num))
		{
			result = "nee";
		}
		else if (angle < (double)(-3f * num) && angle >= (double)(-5f * num))
		{
			result = "ne";
		}
		else if (angle < (double)(-1f * num) && angle >= (double)(-3f * num))
		{
			result = "nne";
		}
		return result;
	}

	public override void Update()
	{
		if (!IsChineseDragon && Part == SerpentPart.Head)
		{
			idleSoundDelay--;
			if (idleSoundDelay <= 0)
			{
				SendMessage(new PlayWorldSoundMessage(Sounds.DrawDifferent(), base.WorldCenter, 0.6f));
				idleSoundDelay = Component._rnd(40, 50);
			}
		}
		Vector2 vector = lastCenter - base.Center;
		if (vector.LengthSquared() > 0.001f)
		{
			angleT = (float)(Math.Atan2(vector.X, vector.Y) * 180.0 / Math.PI);
		}
		angle %= 360f;
		if (angle > 180f)
		{
			angle -= 360f;
		}
		if (angle < -180f)
		{
			angle += 360f;
		}
		while (Math.Abs(angle - (angleT - 360f)) < Math.Abs(angle - angleT))
		{
			angleT -= 360f;
		}
		while (Math.Abs(angle - (angleT + 360f)) < Math.Abs(angle - angleT))
		{
			angleT += 360f;
		}
		angle += (angleT - angle) * 0.2f;
		dirString = ((Part == SerpentPart.Head || Part == SerpentPart.Rib) ? DirectionFromAngle16(angle) : DirectionFromAngle(angle));
		lastCenter = base.Center.Clone();
		avoid += (avoidTarget - avoid) * 0.1f;
		if (BreakT > 0)
		{
			BreakT--;
			if (BreakT == 0)
			{
				Break(null);
				if (Next != null)
				{
					Next.BreakT = BreakD;
				}
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		if (IsBroken)
		{
			return;
		}
		avoidTarget = (avoidPlayer ? (900f - Component._m((base.core.CurrentPlayState.Player.WorldCenter - base.WorldCenter).LengthSquared(), 900f)) : 0f);
		float num = 2f * Component._cos((float)(base.worldTicks + chainN * 5) * 0.2f);
		Vector2 vector = base.WorldCenter.Shift(0f, -3f + num - avoid * 0.025f);
		if (BreakT > 0 && Part == SerpentPart.Head)
		{
			vector += new Vector2(Component._rnd(-3, 3), Component._rnd(-3, 3));
		}
		lastSprite = ((Part == SerpentPart.Head) ? "serpent_head_" : ((Part == SerpentPart.Rib) ? "serpent_rib_" : "serpent_spine_")) + dirString;
		if (IsChineseDragon && Part == SerpentPart.Head)
		{
			lastSprite = "ch_dragon_" + dirString;
		}
		Sprite sprite = _(lastSprite);
		float num2 = 0f;
		if (dirString == "e" || dirString == "w")
		{
			num2 = ((Part == SerpentPart.Head) ? 0.2f : ((Part == SerpentPart.Rib || Part == SerpentPart.Tail1 || Part == SerpentPart.Tail1) ? 0.08f : 0f));
			if (dirString == "w")
			{
				num2 *= -1f;
			}
		}
		float num3 = ((Part == SerpentPart.Tail1) ? 0.7f : ((Part == SerpentPart.Tail2) ? 0.6f : 1f));
		if (Part != SerpentPart.Head && IsChineseDragon)
		{
			num3 *= 0.7f;
			int num4 = (int)(base.WorldCoordinates.Y * 16f);
			base.R[num4 - 20].DrawSpriteW(_(SpriteName.circle_13), vector.Shift(0f, -0.5f), default(Color).FromRgb(0), Vector2.One * num3 * 1f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.R[num4 - 15].DrawSpriteW(_(SpriteName.circle_13), vector, default(Color).FromRgb(6688256), Vector2.One * num3 * 0.8f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.R[num4 - 10].DrawSpriteW(_(SpriteName.circle_10), vector.Shift(0f, -1f), default(Color).FromRgb(13633024), Vector2.One * num3, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.R[num4 - 5].DrawSpriteW(_(SpriteName.circle_8), vector.Shift(0f, -2f), default(Color).FromRgb(16277301), Vector2.One * num3, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.R[num4 - 4].DrawSpriteW(_(SpriteName.circle_4), vector.Shift(0f, -3f), default(Color).FromRgb(16753759), Vector2.One * num3, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (Next != null)
			{
				base.R[num4 - 5].DrawSpriteW(_(SpriteName.circle_5), vector.Shift(0f, -4f) + (Next.WorldPosition - base.WorldPosition) * 0.2f, default(Color).FromRgb(0), Vector2.One * num3, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.R[num4 - 4].DrawSpriteW(_(SpriteName.circle_3), vector.Shift(0f, -4f) + (Next.WorldPosition - base.WorldPosition) * 0.2f, default(Color).FromRgb(16775010), Vector2.One * num3 * 0.8f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		else
		{
			int num5 = (int)(base.WorldCoordinates.Y * 16f - (float)((Part == SerpentPart.Spine && chainN < 3) ? 10 : 0));
			if (chainN == 0 && (dirString == "n" || dirString == "nne" || dirString == "nnw"))
			{
				num5 -= 10;
			}
			base.R[num5].DrawSpriteW(sprite, vector, null, Vector2.One * num3, num2 * Component._sin((float)(base.worldTicks + chainN * 5) * 0.2f), SpriteFlip.None, SpriteOrigin.Center);
		}
		if (Part == SerpentPart.Head && !IsChineseDragon)
		{
			Sprite sprite2 = base.core.SpriteManager.TryGetSprite("serpent_eyes_" + dirString);
			if (sprite2 != null)
			{
				base.core.Renderer[(int)(base.WorldCoordinates.Y * 16f - (float)((Part == SerpentPart.Spine && chainN < 3) ? 10 : 0))].DrawSpriteW(sprite2, vector, null, Vector2.One * num3, num2 * Component._sin((float)(base.worldTicks + chainN * 5) * 0.2f), SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		base.core.Renderer["bg", base.Z + 80, false].DrawSpriteW((IsChineseDragon && chainN > 0) ? _(SpriteName.circle_13) : sprite, vector.Shift(0f, 10f + avoid * 0.03f * 2f - num * 2f), Color.Black * 0.2f, new Vector2(1f, 0.8f) * num3, 0f, SpriteFlip.Vertical, SpriteOrigin.Center);
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		if (IsChineseDragon || IsBroken)
		{
			return;
		}
		if (offender is CreepChar)
		{
			BreakT = 60;
			return;
		}
		IsBroken = true;
		SendMessage(new RemoveEntityMessage(this));
		if (offender is MedusaChar)
		{
			Petrified = true;
		}
		if (offender != null)
		{
			SendMessage(new PlayWorldSoundMessage((Part == SerpentPart.Head) ? SoundName.serpent_die : SoundName.serpent_hurt, base.WorldCenter));
		}
		if (Next != null && BreakT < 0)
		{
			SerpentEntity next = Next;
			int num = 1;
			while (next != null)
			{
				next.BreakT = BreakD * num;
				next.Petrified = Petrified;
				next = next.Next;
				num++;
			}
		}
		if (Part == SerpentPart.Head)
		{
			_inc(Stat.SerpentsKilled);
		}
		Light obj = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(9563694), 0.8f, 0.4f, this);
		obj.FollowRate = 1f;
		obj.ChangeRate = 0.07f;
		obj.Radius = 3f;
		obj.Intencity = 0.4f;
		if (Petrified)
		{
			Vector2 worldCenterCoordinates = base.WorldCenterCoordinates;
			Color? tint = default(Color).FromRgb(6789051);
			SendMessage(new SpawnEntityMessage(new FragmentEntity(spriteStr: lastSprite, coordinates: worldCenterCoordinates, spriteName: SpriteName.pixel, ttl: 600, direction: new Vector4(0f, 0f, 2f, Component._rnd(-0.1f, 0.1f)), layer: "", elevation: 0.6f, bounce: 0.6f, fric: 0.95f, bounceSound: SoundName.none, tint: tint).SetTintFlash(default(Color).FromRgb(9895680), default(Color).FromRgb(6789051), 10).OnFall(delegate(FragmentEntity f)
			{
				SendMessage(new RemoveEntityMessage(f));
				for (int i = 0; i < ((Part == SerpentPart.Head) ? 3 : ((Part != SerpentPart.Rib) ? 1 : 2)); i++)
				{
					Vector4 direction = new Vector4(Component._rnd(-0.05f, 0.05f), Component._rnd(-0.05f, 0.05f), Component._rnd(1.5f, 2f), Component._rnd(-1f, 1f));
					SpriteName spriteName = SpriteName.pixel;
					spriteName = i switch
					{
						3 => SpriteName.rock_shard_4, 
						2 => SpriteName.rock_shard_3, 
						1 => SpriteName.rock_shard_2, 
						0 => SpriteName.rock_shard_1, 
						_ => SpriteName.rock_shard, 
					};
					SendMessage(new PlayWorldSoundMessage(PetrifiedEntity.StoneBreakingSounds.DrawDifferent(), base.WorldCenter));
					SendMessage(new SpawnEntityMessage(new FragmentEntity(f.WorldCenterCoordinates, spriteName, 50, direction, "", 0.05f), null));
					base.core.CurrentPlayState.Camera.Shake("petri-fall");
				}
			}), null));
		}
		if (!Petrified)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.pixel, (BreakT < 0) ? 120 : 120, new Vector4(Component._rnd(-0.04f, 0.04f), Component._rnd(-0.04f, 0.04f), 2f, Component._rnd(-0.1f, 0.1f)), "", 0.2f, 0.6f, 0.95f, SoundName.none, lastSprite), null));
		}
		base.Break(offender);
	}

	public SerpentEntity Head()
	{
		SerpentEntity serpentEntity = this;
		while (serpentEntity.Prev != null)
		{
			serpentEntity = serpentEntity.Prev;
		}
		return serpentEntity;
	}

	public override void CollideWith(Entity other)
	{
		if (!avoidPlayer && !IsBroken && BreakT < 0 && Head().BreakT < 0 && Part != SerpentPart.Tail1 && Part != SerpentPart.Tail2)
		{
			if (other is PlayerEntity { FlyingFreely: false, Dead: false } playerEntity)
			{
				playerEntity.Hurt(InjuryType.Serpent, this);
			}
			base.CollideWith(other);
		}
	}
}
