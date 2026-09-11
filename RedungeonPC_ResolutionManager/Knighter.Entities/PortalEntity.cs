using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PortalEntity : Entity
{
	private Animation anim;

	private bool collapsing;

	private int maxAge;

	public Light Light;

	public PortalEntity(int x, int y, int maxAge = 100)
		: base(x, y, 1f, 1f)
	{
		anim = new Animation(0.25f);
		anim.Add("live", "portal_", "12345");
		anim.Add("spawn", "portal_spawn_", "1234");
		anim.Add("despawn", "portal_despawn_", "12345");
		anim.Play("spawn");
		anim.Loop = false;
		this.maxAge = maxAge;
	}

	public override void Load()
	{
		Light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(7395583), 3f, 0.9f, this);
		SendMessage(new PlayWorldSoundMessage(SoundName.aether_portal_open, base.WorldCenter));
		base.Load();
	}

	public override void Update()
	{
		anim.Update();
		if (anim.Paused)
		{
			if (!collapsing)
			{
				anim.Play("live");
				anim.Loop = true;
			}
			else
			{
				SendMessage(new RemoveEntityMessage(this));
			}
		}
		if (base.Age > maxAge && !collapsing)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.aether_portal_close, base.WorldCenter));
			Collapse();
		}
		base.Update();
	}

	private void Collapse()
	{
		collapsing = true;
		anim.Loop = false;
		anim.Play("despawn");
	}

	public override void Draw()
	{
		float num = 1f;
		if (base.Age > maxAge - 40 && base.Age < maxAge)
		{
			num = 0.6f + 0.4f * Component._cos(0.9f * (float)(base.Age - (maxAge - 40)));
		}
		base.core.Renderer[base.Z].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter.Shift(-15f, -29f), Color.White * num);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (!collapsing)
		{
			if (other is MageChar mageChar)
			{
				Light.Follow(mageChar);
				Light.Intencity = 0.7f;
				Light.Radius = 15f;
				Light.ChangeRate = 0.007f;
				Light.Die();
				mageChar.Teleport();
				Collapse();
			}
			base.CollideWith(other);
		}
	}
}
