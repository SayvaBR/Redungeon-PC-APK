using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ZappedEffectEntity : Entity
{
	private Vector2 playerLastPos;

	private Light light;

	private int zapAnim;

	private int zapDuration = 60;

	private Sprite zappedSprite;

	public ZappedEffectEntity(PlayerEntity player)
		: base(player.WorldCoordinates.X, player.WorldCoordinates.Y, 1f, 1f)
	{
		player.TrySpawnLeftovers(player.WorldCenterCoordinates);
		playerLastPos = player.WorldCenter;
		zappedSprite = _(player.ZappedSprite);
		zapAnim = zapDuration;
	}

	public override void Load()
	{
		Light obj = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(9818367), 7f);
		obj.ChangeRate = 0.003f;
		obj.Position = playerLastPos;
		obj.Die();
		base.Load();
	}

	public override void Update()
	{
		zapAnim--;
		if (zapAnim == 50)
		{
			base.core.ParticleManager.AddEmitter(inWorld: true, playerLastPos.Shift(0f, -10f), 6f).OnSpawn(delegate(Particle p)
			{
				p.Velocity.X = Component._rnd(0.5f, 1.5f);
			}).OnUpdate(delegate(Particle p)
			{
				p.Dead = p.Age == 50;
				p.Position.Y -= p.Velocity.X;
				p.Position.X += Component._sin(p.Age);
			})
				.OnDraw(delegate(Particle p)
				{
					base.core.Renderer[base.Z].DrawDotW(p.Position.X, p.Position.Y, default(Color).FromRgb(9356269), (1f - (float)p.Age / 30f) * 1f);
				})
				.Emit(20, 2);
		}
		if (zapAnim == zapDuration - 2)
		{
			base.core.CurrentPlayState.MakeGameplayScreenshot(0, evenIfDead: true);
		}
		if (zapAnim == 0)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		base.Update();
	}

	public override void Draw()
	{
		if (!base.core.TakingScreenshot)
		{
			base.core.Renderer["fg", -2, false].FillScreen(default(Color).FromRgb(9356269) * (Component._M(zapAnim - 30, 0f) / 30f));
		}
		base.core.Renderer[base.Z + 1].DrawSpriteW(zappedSprite, playerLastPos.Shift(0f, -10f), Color.White * (Component._m(zapAnim, 30f) / 30f), null, Component._rnd(-0.5f, 0.5f), SpriteFlip.None, SpriteOrigin.Center);
		base.Draw();
	}
}
