using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ChestLotteryEntity : Entity
{
	private List<ChestEntity> chests;

	private bool done;

	private bool showedMessage;

	private int anim;

	private const int animDuration = 120;

	public ChestLotteryEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
		chests = new List<ChestEntity>();
	}

	public override void Load()
	{
		int num = Component._rnd(-1, 1);
		for (int i = -1; i <= 1; i++)
		{
			chests.Add(new ChestEntity((int)x + i, (int)y, (num == i) ? new ChestContents(ItemType.GoldCoin, 150) : null));
		}
		foreach (ChestEntity chest in chests)
		{
			SendMessage(new SpawnEntityMessage(chest, CurrentPlatform));
		}
		base.Load();
	}

	public override void Update()
	{
		if (!done)
		{
			foreach (ChestEntity chest in chests)
			{
				if (chest.Looted)
				{
					Done();
					break;
				}
			}
			if (!showedMessage)
			{
				PlayerEntity player = base.core.CurrentPlayState.Player;
				if (player != null && !player.Flying && player.WorldCoordinates.Y <= base.WorldCoordinates.Y + 2f)
				{
					base.core.CurrentPlayState.Hud.ShowAlert("lottery", __(SId.OBJECT_LOTTERY_pick_a_chest), Color.DodgerBlue);
					showedMessage = true;
					anim = 120;
				}
			}
			if (anim > 70)
			{
				anim--;
			}
		}
		base.Update();
	}

	private void Done()
	{
		done = true;
		foreach (ChestEntity chest in chests)
		{
			if (!chest.Looted)
			{
				base.core.ParticleManager.AddEmitter(inWorld: true, chest.WorldCenter, 4f).OnSpawn(delegate
				{
				}).OnUpdate(delegate(Particle p)
				{
					p.Position += new Vector2((0f - p.Offset.X) / 70f, -3f + p.Offset.Y / 5f);
					p.Dead = p.Age > 70;
				})
					.OnDraw(delegate(Particle p)
					{
						base.core.Renderer[base.Z + 5].DrawSpriteW(_(SpriteName.glow_big), p.Position, ((p.Offset.Y > 2f) ? default(Color).FromRgb(8803911) : ((p.Offset.Y > -1.8f) ? default(Color).FromRgb(13658911) : default(Color).FromRgb(5452593))) * ((float)(70 - p.Age) / 70f), new Vector2(0.08f, (float)(p.Age + 10) / 70f), 0f, SpriteFlip.None, SpriteOrigin.Center);
					})
					.Emit(1, 1, once: true, 30);
				SendMessage(new RemoveEntityMessage(chest));
			}
			else if (chest.Contents == null)
			{
				SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.OBJECT_LOTTERY_sorry), Color.White), CurrentPlatform), 30);
			}
		}
	}

	public override void Draw()
	{
		if (!done && anim > 0)
		{
			float num = (float)anim / 120f;
			for (int i = -1; i <= 1; i++)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.web_arrow_s), base.WorldCenter.Shift(i * 16, -30f + Component._m(17f, 200f * (1f - num)) + 2f * Component._sin((float)(base.worldTicks + 90 * i) * 0.2f)), default(Color).FromRgb(7395583) * (num * num * num + 0.3f + 0.4f * Component._sin((float)(base.worldTicks + 90 * i) * 0.2f)), Vector2.One * (0.5f + 0.4f * num), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			}
		}
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}
}
