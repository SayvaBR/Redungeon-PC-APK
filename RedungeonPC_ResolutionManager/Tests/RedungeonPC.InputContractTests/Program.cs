using Knighter;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
#if DEBUG
using Knighter.QA;
#endif
using Knighter.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

var tests = new (string Name, Action Run)[]
{
	("Lykos: desbloqueio custa 100 e quatro níveis concedem habilidades reais", LykosProgression),
	("Lykos: duração, recarga e tentativa de ativar duas vezes", LykosClock),
	("Lykos: cinco ossos reforçam só a próxima transformação", LykosBones),
	("Wisps: brilho aprimorado nasce ativo e independente do bloom geral", WispGlowOption),
	("Congelamento: proporção dos cristais preservada ao girar a tela", FrostGeometry),
	("primeiro frame não cria bordas fantasmas", FirstFrameHasNoEdges),
	("Medusa: ordem, erro e progresso independente dos padrões", MedusaPatterns),
	("Enter confirma somente na borda", KeyboardConfirmEdge),
	("A confirma e ativa habilidade", GamepadPrimaryAction),
	("B cancela sem pausar", GamepadCancel),
	("Start pausa sem cancelar", GamepadPause),
	("P pausa sem confirmar nem cancelar", KeyboardPause),
	("D-pad e analógico geram direções", GamepadDirections),
	("D-pad touch mantém margens, áreas separadas e a pista livre", TouchDPadGeometry),
	("modo canhoto espelha setas e habilidade sem inverter direções", TouchDPadHandedness),
	("D-pad touch retorna ao mesmo layout após múltiplas rotações", TouchDPadRotationRoundTrip),
	("D-pad PC mantém geometria e modo canhoto em janelas e ultrawide", DesktopDPadGeometry),
	("D-pad PC preserva layout ao redimensionar e voltar à janela original", DesktopDPadResizeRoundTrip),
	("WASD e setas geram direções", KeyboardDirections),
	("Tab e Shift+Tab trocam categoria", CategoryNavigation),
	("LB/RB trocam página sem confirmar; X e Y têm ações distintas", ShoulderAndSecondaryActions),
	("reset após perder foco descarta tecla presa", ResetDiscardsHeldInput),
	("navegação de menu repete com atraso", MenuRepeatTiming),
	("codec de replay preserva teclado mouse e gamepad", ReplayCodecRoundTrip),
	("seed de QA reproduz a sequência aleatória", RandomSeedIsRepeatable),
	("storage de QA fica isolado do save do jogador", QaStorageIsIsolated)
};

int failures = 0;
foreach ((string name, Action run) in tests)
{
	try
	{
		run();
		Console.WriteLine($"PASS  {name}");
	}
	catch (Exception ex)
	{
		failures++;
		Console.Error.WriteLine($"FAIL  {name}: {ex.Message}");
	}
}

Console.WriteLine($"{tests.Length - failures}/{tests.Length} contratos aprovados.");
return failures == 0 ? 0 : 1;

static void FirstFrameHasNoEdges()
{
	InputManager input = NewInput(Keys.Enter);
	False(input.WasPressed(GameAction.Confirm));
}

static void WispGlowOption()
{
	var options = new OptionsData();
	True(options.EnhancedWispGlow);
	False(options.LightBloom);
	options.EnhancedWispGlow = false;
	False(options.EnhancedWispGlow);
}

static void LykosProgression()
{
    var desc = CharDescription.Get[Character.Wolf];
    True(desc.UnlockPrice == 100);
    True(desc.Levels.Count == 4);
    True(desc.EntityClass == typeof(WolfChar));
    True(desc.Portrait == SpriteName.lykos_portrait);
    True(desc.Levels[0].Abilities.SkillLevel[Skill.WolfBite] == 1);
    True(desc.Levels[1].Abilities.SkillLevel[Skill.FullMoon] == 20);
    True(desc.Levels[2].Abilities.SkillLevel[Skill.FullMoon] == 15);
    True(desc.Levels[3].Abilities.SkillLevel[Skill.WolfReinforcement] == 1);
    foreach (var skill in new[] { Skill.WolfBite, Skill.FullMoon, Skill.WolfReinforcement })
        True(Abilities.SkillDesc.ContainsKey(skill));
}

static void LykosClock()
{
    False(new LykosPower(1).TryActivate());
    foreach (int level in new[] { 2, 3, 4 })
    {
        var p = new LykosPower(level);
        True(p.TryActivate()); False(p.TryActivate());
        int duration = level == 2 ? 180 : 300;
        True(p.RemainingTicks == duration);
        for (int i = 0; i < duration - 1; i++) False(p.Tick());
        True(p.Active); True(p.Tick()); False(p.Active);
        False(p.TryActivate());
        int recharge = level == 2 ? 1200 : 900;
        for (int i = 0; i < recharge - 1; i++) p.Tick();
        False(p.TryActivate()); p.Tick(); True(p.TryActivate());
    }
}

static void LykosBones()
{
    var p = new LykosPower(4);
    for (int i = 0; i < 4; i++) p.EatBone();
    False(p.Reinforced); p.EatBone(); True(p.Reinforced);
    True(p.TryActivate()); True(p.DurationTicks == 420); False(p.Reinforced);
    for (int i = 0; i < 420 + 900; i++) p.Tick();
    True(p.TryActivate()); True(p.DurationTicks == 300);
    var low = new LykosPower(2);
    for (int i = 0; i < 50; i++) low.EatBone();
    False(low.Reinforced);
}

static void FrostGeometry()
{
    foreach (var size in new[] { (540, 960), (960, 540), (1080, 2400), (2400, 1080), (600, 600) })
    foreach (float progress in new[] { 0f, .5f, 1f })
    {
        var a = FrostLayout.Corner(size.Item1, size.Item2, 1024, 768, progress, 0, 0);
        var b = FrostLayout.Corner(size.Item1, size.Item2, 1024, 768, progress, 1, 1);
        True(Math.Abs((float)a.Width / a.Height - 1024f / 768) < .025f);
        True(a.Width <= size.Item1 / 2 + 1 && a.Height <= size.Item2 / 2 + 1);
        True(b.Right == size.Item1 && b.Bottom == size.Item2);
        True(a.Size == b.Size);
    }
}

static void MedusaPatterns()
{
	var first = new DirectionPattern(new[] { 0, 1, 2 });
	var second = new DirectionPattern(new[] { 3, 3 });
	True(first.Submit(0));
	False(first.Complete);
	True(second.Submit(3));
	False(first.Submit(3));
	True(first.Progress == 0);
	True(second.Progress == 1);
	True(second.Submit(3));
	True(second.Complete);
	False(second.Submit(3));
	True(first.Submit(0)); True(first.Submit(1)); True(first.Submit(2));
	True(first.Complete);
}

static void KeyboardConfirmEdge()
{
	InputManager input = NewInput();
	input.Update(Frame(new KeyboardState(Keys.Enter)));
	True(input.WasPressed(GameAction.Confirm));
	input.Update(Frame(new KeyboardState(Keys.Enter)));
	False(input.WasPressed(GameAction.Confirm));
	input.Update(Frame());
	True(input.WasReleased(GameAction.Confirm));
}

static void GamepadPrimaryAction()
{
	InputManager input = NewInput();
	input.Update(Frame(gamePad: Pad(Buttons.A)));
	True(input.WasPressed(GameAction.Confirm));
	True(input.WasPressed(GameAction.Ability));
	False(input.WasPressed(GameAction.Cancel));
}

static void GamepadCancel()
{
	InputManager input = NewInput();
	input.Update(Frame(gamePad: Pad(Buttons.B)));
	True(input.WasPressed(GameAction.Cancel));
	False(input.WasPressed(GameAction.Pause));
}

static void KeyboardPause()
{
	InputManager input = NewInput();
	input.Update(Frame(new KeyboardState(Keys.P)));
	True(input.WasPressed(GameAction.Pause));
	False(input.WasPressed(GameAction.Confirm));
	False(input.WasPressed(GameAction.Cancel));
	input.Update(Frame(new KeyboardState(Keys.P)));
	False(input.WasPressed(GameAction.Pause));
}

static void GamepadPause()
{
	InputManager input = NewInput();
	input.Update(Frame(gamePad: Pad(Buttons.Start)));
	True(input.WasPressed(GameAction.Pause));
	False(input.WasPressed(GameAction.Cancel));
}

static void GamepadDirections()
{
	InputManager input = NewInput();
	input.Update(Frame(gamePad: Pad(Buttons.DPadLeft)));
	True(input.TryGetPressedDirection(out Vector2 dpad));
	Equal(new Vector2(-1f, 0f), dpad);

	input.Update(Frame());
	input.Update(Frame(gamePad: PadStick(Vector2.UnitY)));
	True(input.TryGetPressedDirection(out Vector2 stick));
	Equal(new Vector2(0f, -1f), stick);
}

static void KeyboardDirections()
{
	InputManager input = NewInput();
	input.Update(Frame(new KeyboardState(Keys.D)));
	True(input.TryGetPressedDirection(out Vector2 wasd));
	Equal(new Vector2(1f, 0f), wasd);

	input.Update(Frame());
	input.Update(Frame(new KeyboardState(Keys.Down)));
	True(input.TryGetPressedDirection(out Vector2 arrows));
	Equal(new Vector2(0f, 1f), arrows);
}

static void TouchDPadGeometry()
{
	ForEachTouchViewport(AssertDPadGeometry);
}

static void AssertDPadGeometry(float width, float height, bool leftHanded, bool compact)
{
	TouchDPadLayout layout = TouchDPadLayout.Create(width, height, leftHanded, compact);
	RectangleF[] directions = { layout.North, layout.East, layout.South, layout.West };
	foreach (RectangleF direction in directions)
	{
		AssertTouchBounds(direction, width, height);
		AssertNoIntersection(direction, layout.Action);
	}
	AssertTouchBounds(layout.Action, width, height);
	if (compact)
	{
		AssertTouchBounds(layout.CompactBounds, width, height);
		foreach (RectangleF direction in directions)
			AssertSameRectangle(layout.CompactBounds, direction);
	}
	else
	{
		for (int first = 0; first < directions.Length; first++)
		for (int second = first + 1; second < directions.Length; second++)
			AssertNoIntersection(directions[first], directions[second]);
		True(float.IsFinite(layout.ArrowSize) && layout.ArrowSize > 0f);
		True(layout.North.Center.Y < layout.West.Center.Y);
		True(layout.South.Center.Y > layout.West.Center.Y);
		True(layout.West.Center.X < layout.North.Center.X);
		True(layout.East.Center.X > layout.North.Center.X);
		Near(layout.West.Center.Y, layout.East.Center.Y);
		Near(layout.North.Center.X, layout.South.Center.X);
		if (height > width)
			Near(width * 0.5f, layout.North.Center.X);
	}

	// Landscape controls must occupy the side gutter, not the central path.
	if (width >= height)
	{
		foreach (RectangleF direction in directions)
		{
			if (leftHanded)
				True(direction.Left >= width * 0.66f - 0.001f);
			else
				True(direction.Right <= width * 0.34f + 0.001f);
		}
	}
}

static void TouchDPadHandedness()
{
	ForEachTouchViewport(AssertDPadHandedness);
}

static void AssertDPadHandedness(float width, float height, bool _, bool compact)
{
	TouchDPadLayout rightHanded = TouchDPadLayout.Create(width, height, false, compact);
	TouchDPadLayout leftHanded = TouchDPadLayout.Create(width, height, true, compact);
	AssertMirroredRectangle(rightHanded.North, leftHanded.North, width);
	AssertMirroredRectangle(rightHanded.South, leftHanded.South, width);
	AssertMirroredRectangle(rightHanded.West, leftHanded.East, width);
	AssertMirroredRectangle(rightHanded.East, leftHanded.West, width);
	AssertMirroredRectangle(rightHanded.Action, leftHanded.Action, width);
	Near(rightHanded.ArrowSize, leftHanded.ArrowSize);
	if (compact)
		AssertMirroredRectangle(rightHanded.CompactBounds, leftHanded.CompactBounds, width);
}

static void DesktopDPadGeometry()
{
	ForEachDesktopViewport((width, height, leftHanded, compact) =>
	{
		ResolutionManager resolution = new();
		resolution.Recalculate(width, height);
		AssertDPadGeometry(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
		AssertDPadHandedness(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
	});
}

static void DesktopDPadResizeRoundTrip()
{
	ForEachDesktopViewport((width, height, leftHanded, compact) =>
	{
		ResolutionManager resolution = new();
		resolution.Recalculate(width, height);
		TouchDPadLayout original = TouchDPadLayout.Create(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
		foreach ((int resizedWidth, int resizedHeight) in DesktopWindowSizes())
		{
			resolution.Recalculate(resizedWidth, resizedHeight);
			TouchDPadLayout resized = TouchDPadLayout.Create(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
			AssertDPadGeometry(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
			ResolutionManager freshResolution = new();
			freshResolution.Recalculate(resizedWidth, resizedHeight);
			TouchDPadLayout fresh = TouchDPadLayout.Create(freshResolution.LogicalWidth, freshResolution.LogicalHeight, leftHanded, compact);
			AssertSameTouchLayout(fresh, resized, compact);
		}
		resolution.Recalculate(width, height);
		TouchDPadLayout restored = TouchDPadLayout.Create(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
		AssertSameTouchLayout(original, restored, compact);
	});
}

static (int Width, int Height)[] DesktopWindowSizes() =>
	new[] { (1280, 720), (1920, 1080), (2560, 1440), (3440, 1440), (720, 1280) };

static void ForEachDesktopViewport(Action<int, int, bool, bool> assertion)
{
	bool previousTouch = Settings.IsTouchDevice;
	float previousGuiScale = Settings.GuiScale;
	try
	{
		Settings.IsTouchDevice = false;
		Settings.GuiScale = 0.85f;
		foreach ((int width, int height) in DesktopWindowSizes())
		foreach (bool leftHanded in new[] { false, true })
		foreach (bool compact in new[] { false, true })
		{
			try
			{
				assertion(width, height, leftHanded, compact);
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException($"PC {width}x{height}, canhoto={leftHanded}, compacto={compact}: {exception.Message}", exception);
			}
		}
	}
	finally
	{
		Settings.IsTouchDevice = previousTouch;
		Settings.GuiScale = previousGuiScale;
	}
}

static void TouchDPadRotationRoundTrip()
{
	bool previousTouch = Settings.IsTouchDevice;
	float previousGuiScale = Settings.GuiScale;
	try
	{
		Settings.IsTouchDevice = true;
		Settings.GuiScale = 1.15f;
		foreach ((int width, int height) in TouchPhoneSizes())
		foreach (bool leftHanded in new[] { false, true })
		foreach (bool compact in new[] { false, true })
		{
			ResolutionManager resolution = new();
			resolution.Recalculate(width, height);
			TouchDPadLayout original = TouchDPadLayout.Create(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
			for (int rotation = 0; rotation < 6; rotation++)
			{
				bool landscape = rotation % 2 == 0;
				resolution.Recalculate(landscape ? height : width, landscape ? width : height);
				TouchDPadLayout rotated = TouchDPadLayout.Create(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
				AssertTouchBounds(rotated.North, resolution.LogicalWidth, resolution.LogicalHeight);
				AssertTouchBounds(rotated.South, resolution.LogicalWidth, resolution.LogicalHeight);
				AssertTouchBounds(rotated.Action, resolution.LogicalWidth, resolution.LogicalHeight);
				if (!landscape)
					AssertSameTouchLayout(original, rotated, compact);
			}
		}
	}
	finally
	{
		Settings.IsTouchDevice = previousTouch;
		Settings.GuiScale = previousGuiScale;
	}
}

static (int Width, int Height)[] TouchPhoneSizes() =>
	new[] { (1220, 2712), (1080, 2400), (720, 1280), (1080, 1920), (1440, 3200) };

static void ForEachTouchViewport(Action<float, float, bool, bool> assertion)
{
	bool previousTouch = Settings.IsTouchDevice;
	float previousGuiScale = Settings.GuiScale;
	try
	{
		Settings.IsTouchDevice = true;
		Settings.GuiScale = 1.15f;
		foreach ((int phoneWidth, int phoneHeight) in TouchPhoneSizes())
		foreach (bool landscape in new[] { false, true })
		foreach (bool leftHanded in new[] { false, true })
		foreach (bool compact in new[] { false, true })
		{
			int width = landscape ? phoneHeight : phoneWidth;
			int height = landscape ? phoneWidth : phoneHeight;
			ResolutionManager resolution = new();
			resolution.Recalculate(width, height);
			try
			{
				assertion(resolution.LogicalWidth, resolution.LogicalHeight, leftHanded, compact);
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException($"{width}x{height}, lógico {resolution.LogicalWidth}x{resolution.LogicalHeight}, canhoto={leftHanded}, compacto={compact}: {exception.Message}", exception);
			}
		}
	}
	finally
	{
		Settings.IsTouchDevice = previousTouch;
		Settings.GuiScale = previousGuiScale;
	}
}

static void AssertTouchBounds(RectangleF rectangle, float width, float height)
{
	const float margin = 12f;
	const float tolerance = 0.001f;
	True(rectangle != null && float.IsFinite(rectangle.Left) && float.IsFinite(rectangle.Top) &&
		float.IsFinite(rectangle.Width) && float.IsFinite(rectangle.Height));
	True(rectangle.Width > 0f && rectangle.Height > 0f);
	if (rectangle.Left < margin - tolerance || rectangle.Top < margin - tolerance ||
		rectangle.Right > width - margin + tolerance || rectangle.Bottom > height - margin + tolerance)
		throw new InvalidOperationException($"área {rectangle.Left},{rectangle.Top}–{rectangle.Right},{rectangle.Bottom} invade margem de {margin}px em {width}x{height}");
}

static void AssertNoIntersection(RectangleF first, RectangleF second)
{
	const float tolerance = 0.001f;
	bool overlap = MathF.Min(first.Right, second.Right) - MathF.Max(first.Left, second.Left) > tolerance &&
		MathF.Min(first.Bottom, second.Bottom) - MathF.Max(first.Top, second.Top) > tolerance;
	if (overlap)
		throw new InvalidOperationException($"áreas de toque se sobrepõem: {first.Left},{first.Top}–{first.Right},{first.Bottom} e {second.Left},{second.Top}–{second.Right},{second.Bottom}");
}

static void AssertSameRectangle(RectangleF expected, RectangleF actual)
{
	Near(expected.Left, actual.Left, 0.001f);
	Near(expected.Top, actual.Top, 0.001f);
	Near(expected.Width, actual.Width, 0.001f);
	Near(expected.Height, actual.Height, 0.001f);
}

static void AssertMirroredRectangle(RectangleF original, RectangleF mirrored, float width)
{
	Near(width - original.Right, mirrored.Left, 0.001f);
	Near(original.Top, mirrored.Top, 0.001f);
	Near(original.Width, mirrored.Width, 0.001f);
	Near(original.Height, mirrored.Height, 0.001f);
}

static void AssertSameTouchLayout(TouchDPadLayout expected, TouchDPadLayout actual, bool compact)
{
	AssertSameRectangle(expected.North, actual.North);
	AssertSameRectangle(expected.East, actual.East);
	AssertSameRectangle(expected.South, actual.South);
	AssertSameRectangle(expected.West, actual.West);
	AssertSameRectangle(expected.Action, actual.Action);
	Near(expected.ArrowSize, actual.ArrowSize);
	if (compact)
		AssertSameRectangle(expected.CompactBounds, actual.CompactBounds);
}

static void CategoryNavigation()
{
	InputManager input = NewInput();
	input.Update(Frame(new KeyboardState(Keys.Tab)));
	True(input.WasPressed(GameAction.NextCategory));
	False(input.WasPressed(GameAction.PreviousCategory));

	input.Update(Frame());
	input.Update(Frame(new KeyboardState(Keys.LeftShift, Keys.Tab)));
	True(input.WasPressed(GameAction.PreviousCategory));
	False(input.WasPressed(GameAction.NextCategory));
}

static void ResetDiscardsHeldInput()
{
	InputManager input = NewInput();
	input.Update(Frame(new KeyboardState(Keys.W)));
	True(input.WasPressed(GameAction.MoveUp));
	input.Reset();
	input.Update(Frame(new KeyboardState(Keys.W)));
	False(input.WasPressed(GameAction.MoveUp));
}

static void ShoulderAndSecondaryActions()
{
	InputManager input = NewInput();
	input.Update(Frame(gamePad: Pad(Buttons.LeftShoulder)));
	True(input.WasPressed(GameAction.PreviousCategory));
	False(input.WasPressed(GameAction.NextCategory));
	False(input.WasPressed(GameAction.Confirm));
	input.Update(Frame(gamePad: Pad(Buttons.LeftShoulder)));
	False(input.WasPressed(GameAction.PreviousCategory));
	input.Update(Frame(gamePad: Pad(Buttons.RightShoulder)));
	True(input.WasPressed(GameAction.NextCategory));
	False(input.WasPressed(GameAction.PreviousCategory));
	input.Update(Frame(gamePad: Pad(Buttons.X)));
	True(input.WasPressed(GameAction.Screenshot));
	False(input.WasPressed(GameAction.Secondary));
	input.Update(Frame(gamePad: Pad(Buttons.Y)));
	True(input.WasPressed(GameAction.Secondary));
	False(input.WasPressed(GameAction.Screenshot));
}

static void MenuRepeatTiming()
{
	InputManager input = NewInput();
	MenuInputRouter router = new(input);
	input.Update(Frame(new KeyboardState(Keys.Down)));
	router.Update(1f / 60f);
	EqualMenu(MenuAction.MoveDown, router.ConsumeDirectionalAction());

	router.Update(0.2f);
	EqualMenu(MenuAction.None, router.ConsumeDirectionalAction());
	router.Update(0.16f);
	EqualMenu(MenuAction.MoveDown, router.ConsumeDirectionalAction());
	router.Update(0.13f);
	EqualMenu(MenuAction.MoveDown, router.ConsumeDirectionalAction());
}

static void ReplayCodecRoundTrip()
{
#if DEBUG
	MouseState mouse = new(
		321,
		123,
		456,
		ButtonState.Pressed,
		ButtonState.Released,
		ButtonState.Pressed,
		ButtonState.Released,
		ButtonState.Pressed,
		789);
	GamePadState gamePad = new(
		new Vector2(-0.75f, 0.25f),
		new Vector2(0.5f, -0.5f),
		0.3f,
		0.8f,
		new[] { Buttons.A, Buttons.DPadLeft, Buttons.RightShoulder });
	InputSnapshot result = QaSession.RoundTripSnapshotForTests(
		new InputSnapshot(new KeyboardState(Keys.W, Keys.Enter), mouse, gamePad));

	True(result.Keyboard.IsKeyDown(Keys.W));
	True(result.Keyboard.IsKeyDown(Keys.Enter));
	EqualValue(321, result.Mouse.X);
	EqualValue(789, result.Mouse.HorizontalScrollWheelValue);
	EqualValue(ButtonState.Pressed, result.Mouse.LeftButton);
	EqualValue(ButtonState.Pressed, result.Mouse.RightButton);
	EqualValue(ButtonState.Pressed, result.Mouse.XButton2);
	True(result.GamePad.IsConnected);
	True(result.GamePad.IsButtonDown(Buttons.A));
	True(result.GamePad.IsButtonDown(Buttons.DPadLeft));
	True(result.GamePad.IsButtonDown(Buttons.RightShoulder));
	Near(-0.75f, result.GamePad.ThumbSticks.Left.X);
	Near(0.8f, result.GamePad.Triggers.Right);
#else
	throw new InvalidOperationException("Este contrato exige configuração Debug.");
#endif
}

static void RandomSeedIsRepeatable()
{
	Knighter.Helpers.SciHelper.SetRandomSeed(8675309);
	int[] first = Enumerable.Range(0, 8).Select(_ => Knighter.Helpers.SciHelper.GetRandom()).ToArray();
	Knighter.Helpers.SciHelper.SetRandomSeed(8675309);
	int[] second = Enumerable.Range(0, 8).Select(_ => Knighter.Helpers.SciHelper.GetRandom()).ToArray();
	True(first.SequenceEqual(second));
}

static void QaStorageIsIsolated()
{
	string directory = Path.Combine(Path.GetTempPath(), "redungeon-input-contracts", Guid.NewGuid().ToString("N"));
	string path = Path.Combine(directory, "qa-save.dat");
	try
	{
		Knighter.Storage first = new(path);
		first.SetString("qa-marker", "isolated");
		first.Save();
		Knighter.Storage second = new(path);
		EqualValue("isolated", second.GetField("qa-marker"));
		True(File.Exists(path));
	}
	finally
	{
		if (Directory.Exists(directory))
		{
			Directory.Delete(directory, recursive: true);
		}
	}
}

static InputManager NewInput(params Keys[] keys)
{
	InputManager input = new();
	input.Update(Frame(new KeyboardState(keys)));
	return input;
}

static InputSnapshot Frame(KeyboardState keyboard = default, GamePadState gamePad = default)
{
	return new InputSnapshot(keyboard, default, gamePad);
}

static GamePadState Pad(params Buttons[] buttons)
{
	return new GamePadState(Vector2.Zero, Vector2.Zero, 0f, 0f, buttons);
}

static GamePadState PadStick(Vector2 leftStick)
{
	return new GamePadState(leftStick, Vector2.Zero, 0f, 0f, Array.Empty<Buttons>());
}

static void True(bool value)
{
	if (!value)
	{
		throw new InvalidOperationException("esperado true");
	}
}

static void False(bool value)
{
	if (value)
	{
		throw new InvalidOperationException("esperado false");
	}
}

static void Equal(Vector2 expected, Vector2 actual)
{
	if (expected != actual)
	{
		throw new InvalidOperationException($"esperado {expected}, recebido {actual}");
	}
}

static void EqualValue<T>(T expected, T actual)
{
	if (!EqualityComparer<T>.Default.Equals(expected, actual))
	{
		throw new InvalidOperationException($"esperado {expected}, recebido {actual}");
	}
}

static void Near(float expected, float actual, float tolerance = 0.0001f)
{
	if (Math.Abs(expected - actual) > tolerance)
	{
		throw new InvalidOperationException($"esperado {expected}, recebido {actual}");
	}
}

static void EqualMenu(MenuAction expected, MenuAction actual)
{
	if (expected != actual)
	{
		throw new InvalidOperationException($"esperado {expected}, recebido {actual}");
	}
}
