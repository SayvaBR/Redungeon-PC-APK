namespace Knighter;

public class OptionsData : Component
{
	public float MusicVolume = 1f;
	public float EffectsVolume = 1f;
	public int PromptStyle;
	public bool DynamicLighting;
	public float LightingStrength = 0.5f;
	public float ControlOpacity = 1f;
	public float VibrationStrength = 0.5f;
	public bool SmoothLighting;
	public bool SoftShadows;
	public bool LightBloom;
	public bool EnhancedWispGlow = true;
	public float BloomStrength = 1f;
	public bool PlayMusic;

	public bool PlaySounds;

	public bool SwipeControl;

	public bool CompactDPad;

	public bool HoldToRun;

	public bool TapToStep;

	public bool LeftHandedMode;

	public bool SeeThroughMode;

	/// <summary>Preferência de dispositivo pra prompts de UI (teclado vs gamepad).</summary>
	public bool PreferGamepad;

	// --- Base para o futuro menu de configurações (vídeo/janela) ---

	public WindowMode WindowMode;

	public int WindowedWidth;

	public int WindowedHeight;

	public bool VSync;

	/// <summary>0 = sem limite.</summary>
	public int FpsLimit;

	/// <summary>Escala da interface, independente da escala de pixel art do ResolutionManager.</summary>
	public float UiScale;

	public OptionsData()
	{
		PlayMusic = true;
		PlaySounds = true;
		SwipeControl = true;
		CompactDPad = false;
		HoldToRun = false;
		TapToStep = true;
		LeftHandedMode = false;
		SeeThroughMode = false;
		PreferGamepad = false;
		WindowMode = WindowMode.Windowed;
		WindowedWidth = 1280;
		WindowedHeight = 720;
		VSync = true;
		FpsLimit = 0;
		UiScale = 1f;
	}

	public void LoadFromStorage()
	{
		core.Storage.TryGetBool("dynamic-lighting", ref DynamicLighting);
		core.Storage.TryGetFloat("lighting-strength", ref LightingStrength);
		LightingStrength = float.IsFinite(LightingStrength) ? System.Math.Clamp(LightingStrength, 0.25f, 1f) : 0.5f;
		core.Storage.TryGetFloat("music-volume", ref MusicVolume);
		core.Storage.TryGetFloat("effects-volume", ref EffectsVolume);
		core.Storage.TryGetInt("prompt-style", ref PromptStyle);
		MusicVolume = float.IsFinite(MusicVolume) ? System.Math.Clamp(MusicVolume, 0f, 1f) : 1f;
		EffectsVolume = float.IsFinite(EffectsVolume) ? System.Math.Clamp(EffectsVolume, 0f, 1f) : 1f;
		PromptStyle = System.Math.Clamp(PromptStyle, 0, 1);
		base.core.Storage.TryGetBool("play-music", ref PlayMusic);
		base.core.Storage.TryGetBool("play-sounds", ref PlaySounds);
		base.core.Storage.TryGetBool("swipe-control", ref SwipeControl);
		base.core.Storage.TryGetBool("compact-dpad", ref CompactDPad);
		base.core.Storage.TryGetBool("hold-to-run", ref HoldToRun);
		base.core.Storage.TryGetBool("tap-to-step", ref TapToStep);
		base.core.Storage.TryGetBool("left-handed", ref LeftHandedMode);
		base.core.Storage.TryGetBool("see-through", ref SeeThroughMode);
		if (!core.Storage.TryGetFloat("control-opacity", ref ControlOpacity)) ControlOpacity = SeeThroughMode ? 0.35f : 1f;
		core.Storage.TryGetFloat("vibration-strength", ref VibrationStrength);
		ControlOpacity = float.IsFinite(ControlOpacity) ? System.Math.Clamp(ControlOpacity, 0f, 1f) : 1f;
		VibrationStrength = float.IsFinite(VibrationStrength) ? System.Math.Clamp(VibrationStrength, 0f, 1f) : 0.5f;
		core.Storage.TryGetBool("smooth-lighting", ref SmoothLighting);
		core.Storage.TryGetBool("soft-shadows", ref SoftShadows);
		core.Storage.TryGetBool("light-bloom", ref LightBloom);
		if (!core.Storage.TryGetBool("enhanced-wisp-glow", ref EnhancedWispGlow)) EnhancedWispGlow = true;
		core.Storage.TryGetFloat("bloom-strength", ref BloomStrength);
		BloomStrength = float.IsFinite(BloomStrength) ? System.Math.Clamp(BloomStrength, 0.25f, 2f) : 1f;
		base.core.Storage.TryGetBool("prefer-gamepad", ref PreferGamepad);

		int windowModeInt = (int)WindowMode;
		base.core.Storage.TryGetInt("window-mode", ref windowModeInt);
		WindowMode = (WindowMode)windowModeInt;
		base.core.Storage.TryGetInt("windowed-width", ref WindowedWidth);
		base.core.Storage.TryGetInt("windowed-height", ref WindowedHeight);
		base.core.Storage.TryGetBool("vsync", ref VSync);
		base.core.Storage.TryGetInt("fps-limit", ref FpsLimit);
		base.core.Storage.TryGetFloat("ui-scale", ref UiScale);
		UiScale = float.IsFinite(UiScale) ? System.Math.Clamp(UiScale, 0.85f, 1.15f) : 1f;
	}

	public void SaveIntoStorage()
	{
		core.Storage.SetBool("dynamic-lighting", DynamicLighting);
		core.Storage.SetFloat("lighting-strength", LightingStrength);
		core.Storage.SetFloat("control-opacity", ControlOpacity);
		core.Storage.SetFloat("vibration-strength", VibrationStrength);
		core.Storage.SetBool("smooth-lighting", SmoothLighting);
		core.Storage.SetBool("soft-shadows", SoftShadows);
		core.Storage.SetBool("light-bloom", LightBloom);
		core.Storage.SetBool("enhanced-wisp-glow", EnhancedWispGlow);
		core.Storage.SetFloat("bloom-strength", BloomStrength);
		core.Storage.SetFloat("music-volume", MusicVolume);
		core.Storage.SetFloat("effects-volume", EffectsVolume);
		core.Storage.SetInt("prompt-style", PromptStyle);
		base.core.Storage.SetBool("play-music", PlayMusic);
		base.core.Storage.SetBool("play-sounds", PlaySounds);
		base.core.Storage.SetBool("swipe-control", SwipeControl);
		base.core.Storage.SetBool("compact-dpad", CompactDPad);
		base.core.Storage.SetBool("hold-to-run", HoldToRun);
		base.core.Storage.SetBool("tap-to-step", TapToStep);
		base.core.Storage.SetBool("left-handed", LeftHandedMode);
		base.core.Storage.SetBool("see-through", SeeThroughMode);
		base.core.Storage.SetBool("prefer-gamepad", PreferGamepad);

		base.core.Storage.SetInt("window-mode", (int)WindowMode);
		base.core.Storage.SetInt("windowed-width", WindowedWidth);
		base.core.Storage.SetInt("windowed-height", WindowedHeight);
		base.core.Storage.SetBool("vsync", VSync);
		base.core.Storage.SetInt("fps-limit", FpsLimit);
		base.core.Storage.SetFloat("ui-scale", UiScale);

		base.core.Storage.Save();
	}
}
