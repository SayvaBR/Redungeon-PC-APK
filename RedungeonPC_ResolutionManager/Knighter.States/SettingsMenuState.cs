using System;
using System.Collections.Generic;
using System.Linq;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

/// <summary>
/// Novo menu de Configurações, organizado em categorias (Áudio, Vídeo,
/// Controles, Idioma, Acessibilidade). Construído sobre o framework
/// genérico em Knighter.UI — cada categoria é só uma lista de widgets.
///
/// Todas as medidas de layout aqui são em pixels lógicos do jogo (a mesma
/// escala minúscula do painel de Opções original — a tela inteira tem só
/// ~266px lógicos de altura). Ver <see cref="Knighter.Graphics.ResolutionManager"/>.
///
/// Fase 2: apenas a categoria Áudio está de fato ligada aos dados reais
/// (OptionsData). As demais aparecem como abas navegáveis, mas mostram um
/// aviso de "Em breve" — é só trocar o conteúdo de cada `case` em
/// <see cref="BuildCategoryWidgets"/> para ligá-las de verdade.
/// </summary>
public class SettingsMenuState : State
{
	protected override bool ShowContextPromptLegend => false;

	private enum Category
	{
		Audio,
		Video,
		Controls,
		Language,
		Achievements,
		Statistics,
		Deaths,
		Progress
	}

	private static readonly Category[] AllCategories = (Category[])Enum.GetValues(typeof(Category));

	// Paleta de destaque dos botões de rodapé — pedra, bronze e dourado,
	// tingindo o painel de madeira (DrawWoodenPanel) de cada botão.
	private static readonly Color StoneTint = new Color(150, 138, 126);
	private static readonly Color BronzeTint = new Color(191, 134, 66);
	private static readonly Color GoldTint = new Color(255, 197, 84);

	private readonly MenuFocusController focusController;

	private Category currentCategory = Category.Audio;
	private float tabAnimT;
	private int optionPage;
	private WindowMode pendingMode;
	private bool pendingVSync;
	private int pendingResolution;
	private static readonly Point[] Resolutions = { new(960, 540), new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440), new(1024, 768), new(1600, 1200), new(1280, 800), new(1920, 1200), new(2560, 1080), new(3440, 1440) };
	private static readonly string[] ResolutionRatios = { "16:9", "16:9", "16:9", "16:9", "16:9", "4:3", "4:3", "16:10", "16:10", "Ultrawide", "Ultrawide" };
	private Point layoutSize;
	private bool sideNavigation;
	private bool rebuildLayout;
	private float RowHeight => 38f * MenuTheme.TextScale;

	private RectangleF panelRect;
	private RectangleF tabsRect;
	private RectangleF listRect;
	private RectangleF footerRect;
	private RectangleF[] tabSlotBounds = new RectangleF[AllCategories.Length];

	public SettingsMenuState()
	{
		base.TransDuration = 25;
		IsOverlay = true;
		ShowCoins = false;
		pendingMode = core.OptionsData.WindowMode;
		pendingVSync = core.OptionsData.VSync;
		pendingResolution = Math.Max(0, Array.FindIndex(Resolutions, p => p.X == core.OptionsData.WindowedWidth && p.Y == core.OptionsData.WindowedHeight));

		focusController = new MenuFocusController(base.core.Input);
		focusController.OnNextCategoryRequested += () => ChangeCategory(1);
		focusController.OnPreviousCategoryRequested += () => ChangeCategory(-1);

		RecalculateLayout();
		BuildCategory(currentCategory, playSound: false);
	}

	public override void Load()
	{
		Screen("settings");
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		base.Load();
	}

	private void RecalculateLayout()
	{
		float logicalWidth = base.core.Renderer.ScreenWidth;
		float logicalHeight = base.core.Renderer.ScreenHeight;

		layoutSize = new Point((int)logicalWidth, (int)logicalHeight);
		float width = Math.Min(logicalWidth - 20f, 620f * MenuTheme.TextScale);
		float height = Math.Min(logicalHeight - 16f, 370f);
		panelRect = new RectangleF((logicalWidth - width) * 0.5f, (logicalHeight - height) * 0.5f, width, height);
		sideNavigation = false;

		tabsRect = new RectangleF(panelRect.X + 6f, panelRect.Y + 28f, panelRect.Width - 12f, 30f);
		footerRect = new RectangleF(panelRect.X + 8f, panelRect.Bottom - 30f, panelRect.Width - 16f, 22f);
		listRect = new RectangleF(panelRect.X + 8f, tabsRect.Bottom + 5f, panelRect.Width - 16f, footerRect.Top - tabsRect.Bottom - 32f);
		if (sideNavigation)
		{
			tabsRect = new RectangleF(panelRect.X + 10f, panelRect.Y + 34f, 72f, 148f);
			listRect = new RectangleF(tabsRect.Right + 14f, tabsRect.Y, panelRect.Right - tabsRect.Right - 26f, footerRect.Top - tabsRect.Y - 32f);
		}

		tabSlotBounds = new RectangleF[AllCategories.Length];
		int visibleTabs = Math.Min(AllCategories.Length, Math.Max(3, (int)(tabsRect.Width / (86f * MenuTheme.TextScale))));
		int firstTab = Math.Clamp((int)currentCategory - visibleTabs / 2, 0, AllCategories.Length - visibleTabs);
		float slotWidth = tabsRect.Width / visibleTabs;
		for (int i = firstTab; i < firstTab + visibleTabs; i++)
		{
			tabSlotBounds[i] = new RectangleF(tabsRect.X + slotWidth * (i - firstTab), tabsRect.Y, slotWidth - 2f, tabsRect.Height);
		}
	}

#if DEBUG
	internal void OpenAchievementsForQA() => BuildCategory(Category.Achievements, false);
	internal void OpenCategoryForQA(string name)
	{
		if (Enum.TryParse(name, true, out Category category)) BuildCategory(category, false);
	}
#endif
	private void ChangeCategory(int direction)
	{
		int index = Array.IndexOf(AllCategories, currentCategory);
		index = ((index + direction) % AllCategories.Length + AllCategories.Length) % AllCategories.Length;
		BuildCategory(AllCategories[index], playSound: true);
	}

	private void BuildCategory(Category category, bool playSound)
	{
		if (currentCategory != category) optionPage = 0;
		currentCategory = category;
		RecalculateLayout();
		tabAnimT = 12f;
		if (playSound)
		{
			SendMessage(new PlaySoundMessage(SoundName.swoosh_2, 0.5f));
		}

		List<IMenuWidget> widgets = BuildCategoryWidgets(category);
		float y = listRect.Y;
		bool browser = widgets.Count == 1 && widgets[0] is FactsBrowserWidget;
		int capacity = Math.Max(1, (int)((listRect.Height - 22f) / (RowHeight + 4)));
		MenuPageWidget pager = null;
		if (!browser && widgets.Count > capacity)
		{
			int pages = (widgets.Count + capacity - 1) / capacity;
			optionPage = Math.Clamp(optionPage, 0, pages - 1);
			widgets = widgets.Skip(optionPage * capacity).Take(capacity).ToList();
			pager = new MenuPageWidget(() => optionPage, () => pages, direction => {
				optionPage = (optionPage + direction + pages) % pages; BuildCategory(currentCategory, true);
			}) { Bounds = new RectangleF(listRect.X, y, listRect.Width, 20f) };
			y += 22f;
		}
		foreach (IMenuWidget widget in widgets)
		{
			float rowHeight = browser ? listRect.Height : RowHeight;
			float widgetWidth = browser ? Math.Min(listRect.Width, 478f) : listRect.Width;
			widget.Bounds = new RectangleF(listRect.Center.X - widgetWidth / 2, y, widgetWidth, rowHeight);
			y += rowHeight + 3f;
		}
		if (pager != null) widgets.Insert(0, pager);
		AddFooterButtons(widgets);
		focusController.SetWidgets(widgets);
	}

	/// <summary>
	/// Único lugar que precisa mudar para adicionar opções novas numa
	/// categoria existente, ou ligar de vez uma categoria que hoje só
	/// mostra "Em breve".
	/// </summary>
	private List<IMenuWidget> BuildCategoryWidgets(Category category)
	{
		List<IMenuWidget> widgets = new List<IMenuWidget>();

		switch (category)
		{
		case Category.Achievements:
			widgets.Add(new FactsBrowserWidget(FactsState.FactsPage.Achievements));
			break;
		case Category.Statistics:
			widgets.Add(new FactsBrowserWidget(FactsState.FactsPage.Facts));
			break;
		case Category.Deaths:
			widgets.Add(new FactsBrowserWidget(FactsState.FactsPage.Deaths));
			break;
		case Category.Progress:
			widgets.Add(new MenuFooterButtonWidget(SId.PC_reset_progress,
				() => SendMessage(new PushStateMessage(new ResetProgressState())), MenuTheme.Accent));
			break;
		case Category.Audio:
			widgets.Add(new MenuValueSelectorWidget(SId.PC_music_volume, SId.PC_volume_desc, SpriteName.icon_music_on, 21,
				() => (int)Math.Round(core.OptionsData.MusicVolume * 20), i => { core.OptionsData.MusicVolume = i / 20f; core.SaveOptions(); }, i => $"{i * 5}%", wrap: false));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_effects_volume, SId.PC_volume_desc, SpriteName.icon_sound_on, 21,
				() => (int)Math.Round(core.OptionsData.EffectsVolume * 20), i => { core.OptionsData.EffectsVolume = i / 20f; core.SaveOptions(); }, i => $"{i * 5}%", wrap: false));
			widgets.Add(new MenuToggleWidget(
				SId.OPTIONS_music_title, SId.OPTIONS_music_desc, SpriteName.icon_music_on,
				() => base.core.OptionsData.PlayMusic,
				value =>
				{
					base.core.OptionsData.PlayMusic = value;
					base.core.ApplyOptions();
					base.core.SaveOptions();
				},
				SId.PC_enabled, SId.PC_disabled));

			widgets.Add(new MenuToggleWidget(
				SId.OPTIONS_sound_effects_title, SId.OPTIONS_sound_effects_desc, SpriteName.icon_sound_on,
				() => base.core.OptionsData.PlaySounds,
				value =>
				{
					base.core.OptionsData.PlaySounds = value;
					base.core.ApplyOptions();
					base.core.SaveOptions();
					if (value)
					{
						SendMessage(new PlaySoundMessage(SoundName.coin));
					}
				},
				SId.PC_enabled, SId.PC_disabled));
			break;
		case Category.Controls:
			widgets.Add(new MenuValueSelectorWidget(SId.PC_style, SId.PC_style_desc, SpriteName.icon_control_dpad, 2,
				() => core.OptionsData.PromptStyle, i => { core.OptionsData.PromptStyle = i; core.SaveOptions(); }, i => i == 0 ? "Xbox" : "PlayStation"));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_control_scheme, SId.PC_control_scheme_desc, SpriteName.icon_control_dpad, 3,
				() => core.OptionsData.SwipeControl ? 0 : core.OptionsData.CompactDPad ? 2 : 1,
				i => { core.OptionsData.SwipeControl = i == 0; core.OptionsData.CompactDPad = i == 2; core.SaveOptions(); core.CurrentPlayState?.RefreshControls(); },
				i => __(i == 0 ? SId.PC_swipe : i == 1 ? SId.PC_dpad : SId.PC_compact_dpad)));
			widgets.Add(new MenuToggleWidget(SId.CONTROLS_left_handed, SId.PC_left_handed_desc, SpriteName.icon_control_dpad,
				() => core.OptionsData.LeftHandedMode, v => { core.OptionsData.LeftHandedMode = v; core.SaveOptions(); core.CurrentPlayState?.RefreshControls(); }, SId.PC_enabled, SId.PC_disabled));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_control_opacity, SId.PC_volume_desc, SpriteName.icon_control_dpad, 21,
				() => (int)MathF.Round(core.OptionsData.ControlOpacity * 20f), i => { core.OptionsData.ControlOpacity = i / 20f; core.OptionsData.SeeThroughMode = false; core.SaveOptions(); core.CurrentPlayState?.RefreshControls(); }, i => $"{i * 5}%", false));
			widgets.Add(new MenuToggleWidget(SId.CONTROLS_tap_to_step, SId.PC_tap_step_desc, SpriteName.icon_control_dpad,
				() => core.OptionsData.TapToStep, v => { core.OptionsData.TapToStep = v; core.SaveOptions(); }, SId.PC_enabled, SId.PC_disabled));
			widgets.Add(new MenuToggleWidget(SId.CONTROLS_hold_to_run, SId.PC_hold_run_desc, SpriteName.icon_control_dpad,
				() => core.OptionsData.HoldToRun, v => { core.OptionsData.HoldToRun = v; core.SaveOptions(); }, SId.PC_enabled, SId.PC_disabled));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_vibration, SId.PC_vibration_desc, SpriteName.icon_control_dpad, 21,
				() => (int)MathF.Round(core.OptionsData.VibrationStrength * 20f), i => { core.OptionsData.VibrationStrength = i / 20f; core.SaveOptions(); Knighter.Gameplay.Rumble.Pulse("preview", 1f, 1f, 18); }, i => $"{i * 5}%", false));
			break;
		case Category.Language:
			Language[] languages = ((Language[])Enum.GetValues(typeof(Language)))
				.Where(lang => lang != Language.ja_JP)
				.ToArray();
			int currentIndex = Array.IndexOf(languages, base.core.LocaleManager.CurrentLocale);
			widgets.Add(new MenuValueSelectorWidget(
				SId.OPTIONS_language_title, SId.OPTIONS_language_desc, SpriteName.icon_globe,
				languages.Length,
				() => Math.Max(currentIndex, 0),
				index =>
				{
					currentIndex = index;
					base.core.LocaleManager.SetCurrentLocale(languages[index]);
					base.core.ProfileData.SaveIntoStorage();
				},
				index => base.core.LocaleManager.Locales[languages[index]].LanguageName));
			break;
		case Category.Video:
			widgets.Add(new MenuValueSelectorWidget(SId.PC_ui_size, SId.PC_ui_size_desc, SpriteName.icon_camera, 3,
				() => core.OptionsData.UiScale < 0.95f ? 0 : core.OptionsData.UiScale > 1.05f ? 2 : 1,
				i => { core.OptionsData.UiScale = 0.85f + i * 0.15f; core.SaveOptions(); rebuildLayout = true; },
				i => __(i == 0 ? SId.PC_ui_small : i == 1 ? SId.PC_ui_medium : SId.PC_ui_large), false));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_dynamic_lighting, SId.PC_dynamic_lighting_desc, SpriteName.icon_options, 5,
				() => core.OptionsData.DynamicLighting ? (int)MathF.Round(core.OptionsData.LightingStrength * 4f) : 0,
				i => { core.OptionsData.DynamicLighting = i > 0; if (i > 0) core.OptionsData.LightingStrength = i / 4f; core.SaveOptions(); },
				i => i == 0 ? __(SId.PC_disabled) : $"{i * 25}%", wrap: false));
			widgets.Add(new MenuToggleWidget(SId.PC_smooth_light, SId.PC_smooth_light_desc, SpriteName.icon_camera,
				() => core.OptionsData.SmoothLighting, v => { core.OptionsData.SmoothLighting = v; core.SaveOptions(); }, SId.PC_enabled, SId.PC_disabled));
			widgets.Add(new MenuToggleWidget(SId.PC_soft_shadows, SId.PC_soft_shadows_desc, SpriteName.icon_camera,
				() => core.OptionsData.SoftShadows, v => { core.OptionsData.SoftShadows = v; core.SaveOptions(); }, SId.PC_enabled, SId.PC_disabled));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_light_bloom, SId.PC_light_bloom_desc, SpriteName.icon_camera, 8,
				() => (int)MathF.Round(core.OptionsData.BloomStrength * 4f),
				i => { core.OptionsData.BloomStrength = i / 4f; core.OptionsData.LightBloom = i > 0; core.SaveOptions(); },
				i => i == 0 ? __(SId.PC_disabled) : $"{i * 25}%", wrap: false));
			widgets.Add(new MenuToggleWidget(SId.PC_wisp_glow, SId.PC_wisp_glow_desc, SpriteName.icon_camera,
				() => core.OptionsData.EnhancedWispGlow, v => { core.OptionsData.EnhancedWispGlow = v; core.SaveOptions(); }, SId.PC_enabled, SId.PC_disabled));
#if !ANDROID
			widgets.Add(new MenuValueSelectorWidget(SId.PC_window_mode, SId.PC_window_desc, SpriteName.icon_camera, 3,
				() => (int)pendingMode, i => pendingMode = (WindowMode)i,
				i => __(i == 0 ? SId.PC_windowed : i == 1 ? SId.PC_fullscreen : SId.PC_borderless)));
			widgets.Add(new MenuValueSelectorWidget(SId.PC_resolution, SId.PC_resolution_desc, SpriteName.icon_camera, Resolutions.Length,
				() => pendingResolution, i => pendingResolution = i, i => $"{Resolutions[i].X}x{Resolutions[i].Y}\n{ResolutionRatios[i]}"));
			widgets.Add(new MenuToggleWidget(SId.PC_vsync, SId.PC_vsync_desc, SpriteName.icon_camera,
				() => pendingVSync, value => pendingVSync = value, SId.PC_enabled, SId.PC_disabled));
#endif
			break;
		default:
			widgets.Add(new MenuSeparatorWidget(SId.OPTIONS_coming_soon_title, big: true));
			break;
		}

		return widgets;
	}

	private void AddFooterButtons(List<IMenuWidget> widgets)
	{
		float gap = 4f;
		float buttonWidth = (footerRect.Width - gap * 2f) / 3f;

		widgets.Add(new MenuFooterButtonWidget(SId.OPTIONS_footer_back, Close, StoneTint)
		{
			Bounds = new RectangleF(footerRect.X, footerRect.Y, buttonWidth, footerRect.Height)
		});
		widgets.Add(new MenuFooterButtonWidget(SId.OPTIONS_footer_restore, RestoreDefaults, BronzeTint)
		{
			Bounds = new RectangleF(footerRect.X + buttonWidth + gap, footerRect.Y, buttonWidth, footerRect.Height)
		});
		widgets.Add(new MenuFooterButtonWidget(SId.OPTIONS_footer_apply, ApplySettings, GoldTint)
		{
			Bounds = new RectangleF(footerRect.X + (buttonWidth + gap) * 2f, footerRect.Y, buttonWidth, footerRect.Height)
		});
	}

	private void RestoreDefaults()
	{
		if (currentCategory == Category.Video) { pendingMode = WindowMode.Windowed; pendingResolution = 1; pendingVSync = true; core.OptionsData.UiScale = 1f; core.OptionsData.DynamicLighting = false; core.OptionsData.LightingStrength = 0.5f; core.OptionsData.BloomStrength = 1f; core.OptionsData.SmoothLighting = core.OptionsData.SoftShadows = core.OptionsData.LightBloom = false; core.OptionsData.EnhancedWispGlow = true; }
		if (currentCategory == Category.Controls)
		{
			core.OptionsData.PromptStyle = 0; core.OptionsData.PreferGamepad = false;
			core.OptionsData.SwipeControl = true; core.OptionsData.CompactDPad = false;
			core.OptionsData.LeftHandedMode = false; core.OptionsData.SeeThroughMode = false;
			core.OptionsData.ControlOpacity = 1f; core.OptionsData.VibrationStrength = 0.5f;
			core.OptionsData.TapToStep = true; core.OptionsData.HoldToRun = false;
			core.CurrentPlayState?.RefreshControls();
		}
		if (currentCategory == Category.Audio)
		{
		core.OptionsData.MusicVolume = 1f;
		core.OptionsData.EffectsVolume = 1f;
		base.core.OptionsData.PlayMusic = true;
		base.core.OptionsData.PlaySounds = true;
		}
		base.core.ApplyOptions();
		base.core.SaveOptions();
		BuildCategory(currentCategory, playSound: true);
	}

	private void Close()
	{
		OnBackButtonPressed();
	}

	private void ApplySettings()
	{
		core.Game.SetWindowedResolution(Resolutions[pendingResolution].X, Resolutions[pendingResolution].Y);
		core.Game.SetWindowMode(pendingMode);
		core.OptionsData.WindowMode = pendingMode;
		core.OptionsData.WindowedWidth = Resolutions[pendingResolution].X;
		core.OptionsData.WindowedHeight = Resolutions[pendingResolution].Y;
		core.OptionsData.VSync = pendingVSync;
		core.ApplyOptions();
		core.SaveOptions();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None)
		{
			HandleTabClicks();
			focusController.Update();
		}
		base.HandleInput();
	}

	private void HandleTabClicks()
	{
		foreach (TouchLocation touch in base.core.TouchState)
		{
			if (touch.State != TouchLocationState.Released)
			{
				continue;
			}
			for (int i = 0; i < tabSlotBounds.Length; i++)
			{
					if (tabSlotBounds[i]?.Contains(touch.Position) == true && AllCategories[i] != currentCategory)
				{
					BuildCategory(AllCategories[i], playSound: true);
					break;
				}
			}
		}
	}

	public override void Update()
	{
		if (rebuildLayout || layoutSize != new Point(R.ScreenWidth, R.ScreenHeight))
		{
			rebuildLayout = false;
			RecalculateLayout();
			BuildCategory(currentCategory, false);
		}
		IsOpaque = Transition == TransType.None;
		if (tabAnimT > 0f)
		{
			tabAnimT -= 1f;
		}
		base.Update();
	}

	public override void Draw()
	{
		float fadeT = 1f - (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 2000, false].FillScreen(Color.Black * (1f - fadeT * fadeT * fadeT));

		float progress = (float)base.Trans / base.TransDuration;
		float inset = (1f - progress) * 12f;
		RectangleF animatedPanel = panelRect.Grow(inset, inset, -inset, -inset);

		MenuTheme.Panel(R, animatedPanel, 2001);

		DrawHeader();
		ContextPromptLegend.DrawButton(R, new Vector2(panelRect.X + 8f, panelRect.Y + 3f), PromptAction.PreviousCategory);
		ContextPromptLegend.DrawButton(R, new Vector2(panelRect.Right - 24f, panelRect.Y + 3f), PromptAction.NextCategory);
		DrawTabs();
		focusController.Draw();
		ContextPromptLegend.Draw(
			base.core.Renderer["fg", 3500, false],
			new Vector2(panelRect.X + 10f, footerRect.Y - 24f),
			new ContextPrompt(PromptAction.Confirm, __(SId.PROMPT_confirm)),
			new ContextPrompt(PromptAction.Cancel, __(SId.PROMPT_cancel)));

		base.Draw();
	}

	private void DrawHeader()
	{
		string title = __(SId.OPTIONS_settings_title);

		TextProfile titleProfile = TextProfile.OrangeBoldText.Alter(
			boxAlignment: Alignment2D.Center,
			textAlignment: Alignment2D.Middle,
			width: (int)panelRect.Width - 10,
			height: 10,
			scale: 0.78f);
		base.core.Renderer["fg", 2002, false].DrawTextS(title.ToUpper(), new Vector2(panelRect.Center.X, panelRect.Y + 6f), titleProfile);

		float lineY = panelRect.Y + 17f;
		base.core.Renderer["fg", 2002, false].DrawRectangleS(new RectangleF(panelRect.X + 10f, lineY, panelRect.Width - 20f, 1f), TextProfile.GravestoneScript);
	}

	private void DrawTabs()
	{
		for (int i = 0; i < AllCategories.Length; i++)
		{
			bool isActive = AllCategories[i] == currentCategory;
			RectangleF slot = tabSlotBounds[i];
			if (slot == null) continue;
			float t = MathHelper.Clamp(tabAnimT / 12f, 0f, 1f);
			float lift = isActive ? 2f * t * t : 0f;
			RectangleF drawSlot = new RectangleF(slot.X, slot.Y - lift, slot.Width, slot.Height + lift);

			int tabDepth = 2010 + i * 10;
			Renderer renderer = base.core.Renderer["fg", tabDepth, false];
			MenuTheme.Button(renderer, drawSlot, tabDepth, opacity: isActive ? 1f : 0.58f);


			Sprite icon = _(CategoryIcon(AllCategories[i]));
			Vector2 iconScale = Vector2.One * Math.Min(1f, 12f / Math.Max(icon.Width, icon.Height));
			Vector2 iconSize = icon.Size * iconScale;
			Vector2 iconPos = new Vector2(drawSlot.Center.X - iconSize.X / 2f, drawSlot.Y + 3f);
			if (AllCategories[i] == Category.Progress)
			{
				Vector2 p = new(drawSlot.Center.X - 6, drawSlot.Y + 3);
				R["fg", tabDepth + 3, false].DrawRectangleS(new RectangleF(p.X, p.Y, 12, 12), MenuTheme.Accent);
				R["fg", tabDepth + 4, false].DrawRectangleS(new RectangleF(p.X + 3, p.Y, 6, 4), MenuTheme.Surface);
				R["fg", tabDepth + 4, false].DrawRectangleS(new RectangleF(p.X + 2, p.Y + 7, 8, 5), Color.Wheat);
				R["fg", tabDepth + 5, false].DrawRectangleS(new RectangleF(p.X + 3, p.Y + 9, 6, 1), MenuTheme.Border);
			}
			else R["fg", tabDepth + 3, false].DrawSpriteS(icon, iconPos, new Color(255, 220, 140) * (isActive ? 1f : 0.7f), iconScale);

			TextProfile tabProfile = TextProfile.OrangeBoldText.Alter(
				boxAlignment: Alignment2D.Center,
				textAlignment: Alignment2D.Middle,
				width: (int)drawSlot.Width - 4,
				height: 10,
				scale: 0.55f * MenuTheme.TextScale);
			base.core.Renderer["fg", tabDepth + 3, false].DrawTextS(__(CategoryTitleId(AllCategories[i])).ToUpper(), new Vector2(drawSlot.Center.X, drawSlot.Y + 17f), tabProfile);
		}
	}

	private static SId CategoryTitleId(Category category)
	{
		return category switch
		{
			Category.Audio => SId.OPTIONS_category_audio,
			Category.Achievements => SId.PC_achievements,
			Category.Statistics => SId.FACTS_facts,
			Category.Deaths => SId.FACTS_deaths,
			Category.Progress => SId.PC_progress,
			Category.Video => SId.OPTIONS_category_video,
			Category.Controls => SId.OPTIONS_category_controls,
			Category.Language => SId.OPTIONS_category_language,
			_ => SId.OPTIONS_category_audio,
		};
	}

	private static SpriteName CategoryIcon(Category category)
	{
		return category switch
		{
			Category.Audio => SpriteName.icon_music_on,
			Category.Achievements => SpriteName.icon_achievements,
			Category.Statistics => SpriteName.facts_field_time,
			Category.Deaths => SpriteName.facts_field_death,
			Category.Progress => SpriteName.icon_options,
			Category.Video => SpriteName.icon_camera,
			Category.Controls => SpriteName.icon_control_dpad,
			Category.Language => SpriteName.icon_globe,
			_ => SpriteName.icon_options,
		};
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		TransitionOut(CoreEvent.HideOptions);
		base.OnBackButtonPressed();
	}
}
