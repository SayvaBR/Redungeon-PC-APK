using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class XmasTreeEntity : Entity
{
	private Light light;

	private int lightIndex;

	public XmasTreeEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(Color.Red, 5f, 0.8f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 1f;
		base.Load();
	}

	public override void Update()
	{
		int num = base.worldTicks % 60;
		lightIndex = ((num >= 20) ? ((num < 40) ? 1 : 2) : 0);
		light.Color = ((lightIndex == 0) ? Color.Chartreuse : ((lightIndex == 1) ? Color.Red : Color.DeepSkyBlue));
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.xmas_tree), base.WorldCenter.Shift(-13f, -22f));
		base.core.Renderer[base.Z].DrawSpriteW(_("xmas_tree_" + ((lightIndex == 0) ? "green" : ((lightIndex == 1) ? "red" : "blue"))), base.WorldCenter.Shift(-13f, -22f));
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(_(SpriteName.xmas_tree), base.WorldCenter.Shift(-13f, -9f), Color.Black * 0.2f, null, 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return false;
	}

	public override void InteractWith(Entity other)
	{
		if (other is PlayerEntity)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.xmas_bells, base.WorldCenter));
		}
	}
}
