using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Knighter.Gameplay;

/// <summary>
/// Vibração do gamepad, no mesmo padrão do Camera.Shake: cada "pulso" tem um
/// id, uma força pro motor grave (impacto/peso) e uma pro motor agudo
/// (nitidez), e uma duração em ticks. Se dois pulsos competem no mesmo
/// frame, vence o mais forte; um pulso com o mesmo id só reinicia a duração.
/// Update() precisa ser chamado uma vez por tick (ver Renderer.Update()).
/// </summary>
public static class Rumble
{
	// Platform bridge: invoked once per accepted pulse, never restarted every frame.
	public static System.Action<float, int> PhonePulse;
	public static System.Action PhoneStop;
	public static bool LastGamepadAccepted { get; private set; }
	private static bool pending;
	private static float lowStrength;

	private static float highStrength;

	private static int duration;

	private static string id = string.Empty;

	/// <param name="low">Motor grave (0-1) — peso/impacto.</param>
	/// <param name="high">Motor agudo (0-1) — nitidez/velocidade.</param>
	/// <param name="duration">Duração em ticks (1 tick = 1 Update, 60/s).</param>
	public static void Pulse(string id, float low, float high, int duration = 8)
	{
		low = float.IsFinite(low) ? MathHelper.Clamp(low, 0f, 1f) : 0f;
		high = float.IsFinite(high) ? MathHelper.Clamp(high, 0f, 1f) : 0f;
		if (duration <= 0 || low + high <= 0f) return;
		if (id == Rumble.id)
		{
			lowStrength = low;
			highStrength = high;
			Rumble.duration = duration;
			pending = true;
		}
		else if (Rumble.duration <= 0 || low + high > lowStrength + highStrength)
		{
			lowStrength = low;
			highStrength = high;
			Rumble.duration = duration;
			Rumble.id = id;
			pending = true;
		}
	}

	public static void Update()
	{
		float strength = MathHelper.Clamp(Core.Instance?.OptionsData?.VibrationStrength ?? 0f, 0f, 1f);
		if (strength <= 0f) { if (duration > 0 || pending) Stop(); return; }
		if (duration > 0)
		{
			LastGamepadAccepted = GamePad.SetVibration(PlayerIndex.One, lowStrength * strength, highStrength * strength);
			if (pending)
			{
				PhonePulse?.Invoke(System.Math.Max(lowStrength, highStrength) * strength, System.Math.Max(20, duration * 1000 / 60));
				pending = false;
			}
			duration--;
			return;
		}
		if (lowStrength > 0f || highStrength > 0f)
		{
			GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
			lowStrength = 0f;
			highStrength = 0f;
			id = string.Empty;
		}
	}
	public static void Stop()
	{
		pending = false;
		PhoneStop?.Invoke();
		duration = 0;
		lowStrength = highStrength = 0f;
		id = string.Empty;
		GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
	}
}
