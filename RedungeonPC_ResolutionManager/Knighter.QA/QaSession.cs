#if DEBUG
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Knighter.Helpers;
using Knighter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Knighter.QA;

/// <summary>
/// Sessão determinística de QA ativada exclusivamente por linha de comando em
/// builds Debug. Grava a leitura bruta dos dispositivos uma vez por frame e as
/// mudanças de State; na reprodução, compara a linha do tempo observada com a
/// original. Nunca usa o save normal do jogador.
/// </summary>
public static class QaSession
{
	private const int FormatVersion = 1;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};

	private static QaMode mode;
	private static string sessionPath;
	private static string resultPath;
	private static StreamWriter writer;
	private static List<QaFrameRecord> replayFrames;
	private static List<QaStateRecord> expectedStates;
	private static readonly List<QaStateRecord> actualStates = new();
	private static readonly List<string> mismatches = new();
	private static string previousState;
	private static int replayFrameIndex;
	private static int expectedStateIndex;
	private static int recordedFrameCount;
	private static int seed;
	private static int maxFrames;

	public static string StoragePath { get; private set; }

	public static bool ReplayComplete { get; private set; }

	public static bool ExitWhenReplayCompletes { get; private set; } = true;

	public static bool IsActive => mode != QaMode.None;

	public static bool ShouldExitAfterCurrentFrame =>
		(mode == QaMode.Record && maxFrames > 0 && recordedFrameCount >= maxFrames)
		|| (mode == QaMode.Replay && ReplayComplete && ExitWhenReplayCompletes);

	/// <summary>
	/// Verifica o mesmo codec usado no arquivo de sessão sem acessar os
	/// dispositivos reais. Existe apenas em Debug e é consumido pelos testes
	/// de contrato do port.
	/// </summary>
	public static InputSnapshot RoundTripSnapshotForTests(InputSnapshot snapshot)
	{
		return QaFrameRecord.FromSnapshot(0, snapshot).ToSnapshot();
	}

	public static void Configure(string[] args, string launchDirectory)
	{
		if (args == null || args.Length == 0)
		{
			return;
		}

		string recordArgument = ReadArgument(args, "--qa-record");
		string replayArgument = ReadArgument(args, "--qa-replay");
		if (!string.IsNullOrWhiteSpace(recordArgument) && !string.IsNullOrWhiteSpace(replayArgument))
		{
			throw new ArgumentException("Use somente --qa-record ou --qa-replay por execução.");
		}
		if (string.IsNullOrWhiteSpace(recordArgument) && string.IsNullOrWhiteSpace(replayArgument))
		{
			return;
		}

		ExitWhenReplayCompletes = !args.Contains("--qa-keep-open", StringComparer.OrdinalIgnoreCase);
		if (!string.IsNullOrWhiteSpace(recordArgument))
		{
			ConfigureRecording(ResolvePath(recordArgument, launchDirectory), args);
		}
		else
		{
			ConfigureReplay(ResolvePath(replayArgument, launchDirectory));
		}
	}

	public static IInputSource CreateInputSource()
	{
		return mode switch
		{
			QaMode.Record => new RecordingInputSource(new MonoGameInputSource()),
			QaMode.Replay => new ReplayInputSource(),
			_ => new MonoGameInputSource()
		};
	}

	public static void ObserveState(int gameTick, string stateName)
	{
		if (!IsActive || string.IsNullOrEmpty(stateName) || stateName == previousState)
		{
			return;
		}

		previousState = stateName;
		int frame = Math.Max(0, CurrentFrameIndex);
		QaStateRecord actual = new()
		{
			Type = "state",
			Frame = frame,
			GameTick = gameTick,
			Name = stateName
		};
		actualStates.Add(actual);

		if (mode == QaMode.Record)
		{
			WriteLine(actual);
			return;
		}

		if (expectedStateIndex >= expectedStates.Count)
		{
			mismatches.Add($"Estado extra no frame {frame}: {stateName}.");
			return;
		}

		QaStateRecord expected = expectedStates[expectedStateIndex++];
		if (!string.Equals(expected.Name, stateName, StringComparison.Ordinal))
		{
			mismatches.Add($"Estado {expectedStateIndex}: esperado {expected.Name}, recebido {stateName}.");
		}
		if (expected.Frame != frame)
		{
			mismatches.Add($"Frame de {stateName}: esperado {expected.Frame}, recebido {frame}.");
		}
	}

	public static void Shutdown(Exception failure = null)
	{
		if (!IsActive)
		{
			return;
		}

		try
		{
			writer?.Flush();
			writer?.Dispose();
			writer = null;

			if (mode == QaMode.Replay && expectedStateIndex < expectedStates.Count)
			{
				mismatches.Add($"Faltaram {expectedStates.Count - expectedStateIndex} transições de estado.");
			}
			if (failure != null)
			{
				mismatches.Add($"Exceção: {failure.GetType().Name}: {failure.Message}");
			}

			QaResult result = new()
			{
				Mode = mode.ToString(),
				Success = failure == null && mismatches.Count == 0,
				Seed = seed,
				Frames = mode == QaMode.Record ? recordedFrameCount : replayFrameIndex,
				ReplayComplete = mode != QaMode.Replay || ReplayComplete,
				ExpectedStateTransitions = expectedStates?.Count ?? actualStates.Count,
				ActualStateTransitions = actualStates.Count,
				Mismatches = mismatches.ToArray()
			};
			File.WriteAllText(resultPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
		}
		finally
		{
			mode = QaMode.None;
		}
	}

	private static int CurrentFrameIndex => mode == QaMode.Record
		? recordedFrameCount - 1
		: replayFrameIndex - 1;

	private static void ConfigureRecording(string path, string[] args)
	{
		mode = QaMode.Record;
		sessionPath = path;
		resultPath = path + ".result.json";
		seed = ReadIntArgument(args, "--qa-seed") ?? Environment.TickCount;
		maxFrames = ReadIntArgument(args, "--qa-max-frames") ?? 0;
		if (maxFrames < 0)
		{
			throw new ArgumentOutOfRangeException("--qa-max-frames", "O limite de frames não pode ser negativo.");
		}
		SciHelper.SetRandomSeed(seed);

		PrepareDirectory(path);
		StoragePath = path + ".save.dat";
		string initialStoragePath = path + ".initial-save.dat";
		DeleteIfExists(StoragePath);
		File.WriteAllText(initialStoragePath, string.Empty);

		writer = new StreamWriter(File.Create(path));
		WriteLine(new QaHeaderRecord
		{
			Type = "header",
			Version = FormatVersion,
			Seed = seed,
			CreatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
		});
	}

	private static void ConfigureReplay(string path)
	{
		if (!File.Exists(path))
		{
			throw new FileNotFoundException("Sessão de QA não encontrada.", path);
		}

		mode = QaMode.Replay;
		sessionPath = path;
		resultPath = path + ".replay-result.json";
		replayFrames = new List<QaFrameRecord>();
		expectedStates = new List<QaStateRecord>();
		QaHeaderRecord header = null;

		foreach (string line in File.ReadLines(path))
		{
			if (string.IsNullOrWhiteSpace(line))
			{
				continue;
			}
			using JsonDocument document = JsonDocument.Parse(line);
			if (!document.RootElement.TryGetProperty("Type", out JsonElement typeElement))
			{
				throw new InvalidDataException("Linha sem campo Type na sessão de QA.");
			}
			switch (typeElement.GetString())
			{
			case "header":
				header = JsonSerializer.Deserialize<QaHeaderRecord>(line, JsonOptions);
				break;
			case "frame":
				replayFrames.Add(JsonSerializer.Deserialize<QaFrameRecord>(line, JsonOptions));
				break;
			case "state":
				expectedStates.Add(JsonSerializer.Deserialize<QaStateRecord>(line, JsonOptions));
				break;
			}
		}

		if (header == null || header.Version != FormatVersion)
		{
			throw new InvalidDataException($"Formato de sessão incompatível; esperado {FormatVersion}.");
		}
		if (replayFrames.Count == 0)
		{
			throw new InvalidDataException("A sessão não contém frames de input.");
		}

		seed = header.Seed;
		SciHelper.SetRandomSeed(seed);
		StoragePath = path + ".replay-save.dat";
		string initialStoragePath = path + ".initial-save.dat";
		if (File.Exists(initialStoragePath))
		{
			File.Copy(initialStoragePath, StoragePath, overwrite: true);
		}
		else
		{
			File.WriteAllText(StoragePath, string.Empty);
		}
	}

	private static string ResolvePath(string path, string launchDirectory)
	{
		return Path.GetFullPath(path, launchDirectory);
	}

	private static void PrepareDirectory(string path)
	{
		string directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(directory))
		{
			Directory.CreateDirectory(directory);
		}
	}

	private static void DeleteIfExists(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	private static string ReadArgument(string[] args, string name)
	{
		for (int i = 0; i < args.Length - 1; i++)
		{
			if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
			{
				return args[i + 1];
			}
		}
		return null;
	}

	private static int? ReadIntArgument(string[] args, string name)
	{
		string value = ReadArgument(args, name);
		if (value == null)
		{
			return null;
		}
		if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
		{
			throw new ArgumentException($"{name} requer um número inteiro.");
		}
		return result;
	}

	private static void WriteLine<T>(T value)
	{
		writer.WriteLine(JsonSerializer.Serialize(value, JsonOptions));
		writer.Flush();
	}

	private sealed class RecordingInputSource : IInputSource
	{
		private readonly IInputSource inner;

		public RecordingInputSource(IInputSource inner)
		{
			this.inner = inner;
		}

		public InputSnapshot Capture()
		{
			InputSnapshot snapshot = UiFixtureInput.Capture(recordedFrameCount, inner.Capture());
			QaFrameRecord record = QaFrameRecord.FromSnapshot(recordedFrameCount, snapshot);
			recordedFrameCount++;
			WriteLine(record);
			return snapshot;
		}
	}

	private sealed class ReplayInputSource : IInputSource
	{
		public InputSnapshot Capture()
		{
			if (replayFrameIndex >= replayFrames.Count)
			{
				ReplayComplete = true;
				return default;
			}

			InputSnapshot snapshot = replayFrames[replayFrameIndex++].ToSnapshot();
			if (replayFrameIndex >= replayFrames.Count)
			{
				ReplayComplete = true;
			}
			return snapshot;
		}
	}

	private enum QaMode
	{
		None,
		Record,
		Replay
	}

	private sealed class QaHeaderRecord
	{
		public string Type { get; set; }
		public int Version { get; set; }
		public int Seed { get; set; }
		public string CreatedUtc { get; set; }
	}

	private sealed class QaFrameRecord
	{
		private static readonly Buttons[] AllButtons = Enum.GetValues<Buttons>()
			.Where(value => value != Buttons.None)
			.ToArray();

		public string Type { get; set; } = "frame";
		public int Frame { get; set; }
		public int[] Keys { get; set; }
		public QaMouseRecord Mouse { get; set; }
		public QaGamePadRecord GamePad { get; set; }

		public static QaFrameRecord FromSnapshot(int frame, InputSnapshot snapshot)
		{
			return new QaFrameRecord
			{
				Frame = frame,
				Keys = snapshot.Keyboard.GetPressedKeys().Select(key => (int)key).ToArray(),
				Mouse = QaMouseRecord.FromState(snapshot.Mouse),
				GamePad = QaGamePadRecord.FromState(snapshot.GamePad, AllButtons)
			};
		}

		public InputSnapshot ToSnapshot()
		{
			KeyboardState keyboard = new((Keys ?? Array.Empty<int>()).Select(value => (Keys)value).ToArray());
			return new InputSnapshot(keyboard, Mouse?.ToState() ?? default, GamePad?.ToState() ?? default);
		}
	}

	private sealed class QaMouseRecord
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Scroll { get; set; }
		public int HorizontalScroll { get; set; }
		public bool Left { get; set; }
		public bool Middle { get; set; }
		public bool Right { get; set; }
		public bool X1 { get; set; }
		public bool X2 { get; set; }

		public static QaMouseRecord FromState(MouseState state)
		{
			return new QaMouseRecord
			{
				X = state.X,
				Y = state.Y,
				Scroll = state.ScrollWheelValue,
				HorizontalScroll = state.HorizontalScrollWheelValue,
				Left = state.LeftButton == ButtonState.Pressed,
				Middle = state.MiddleButton == ButtonState.Pressed,
				Right = state.RightButton == ButtonState.Pressed,
				X1 = state.XButton1 == ButtonState.Pressed,
				X2 = state.XButton2 == ButtonState.Pressed
			};
		}

		public MouseState ToState()
		{
			return new MouseState(
				X,
				Y,
				Scroll,
				ToButtonState(Left),
				ToButtonState(Middle),
				ToButtonState(Right),
				ToButtonState(X1),
				ToButtonState(X2),
				HorizontalScroll);
		}

		private static ButtonState ToButtonState(bool pressed)
		{
			return pressed ? ButtonState.Pressed : ButtonState.Released;
		}
	}

	private sealed class QaGamePadRecord
	{
		public bool Connected { get; set; }
		public float LeftX { get; set; }
		public float LeftY { get; set; }
		public float RightX { get; set; }
		public float RightY { get; set; }
		public float LeftTrigger { get; set; }
		public float RightTrigger { get; set; }
		public int[] Buttons { get; set; }

		public static QaGamePadRecord FromState(GamePadState state, Buttons[] allButtons)
		{
			return new QaGamePadRecord
			{
				Connected = state.IsConnected,
				LeftX = state.ThumbSticks.Left.X,
				LeftY = state.ThumbSticks.Left.Y,
				RightX = state.ThumbSticks.Right.X,
				RightY = state.ThumbSticks.Right.Y,
				LeftTrigger = state.Triggers.Left,
				RightTrigger = state.Triggers.Right,
				Buttons = allButtons.Where(state.IsButtonDown).Select(button => (int)button).ToArray()
			};
		}

		public GamePadState ToState()
		{
			if (!Connected)
			{
				return default;
			}
			return new GamePadState(
				new Vector2(LeftX, LeftY),
				new Vector2(RightX, RightY),
				LeftTrigger,
				RightTrigger,
				(Buttons ?? Array.Empty<int>()).Select(value => (Microsoft.Xna.Framework.Input.Buttons)value).ToArray());
		}
	}

	private sealed class QaStateRecord
	{
		public string Type { get; set; }
		public int Frame { get; set; }
		public int GameTick { get; set; }
		public string Name { get; set; }
	}

	private sealed class QaResult
	{
		public string Mode { get; set; }
		public bool Success { get; set; }
		public int Seed { get; set; }
		public int Frames { get; set; }
		public bool ReplayComplete { get; set; }
		public int ExpectedStateTransitions { get; set; }
		public int ActualStateTransitions { get; set; }
		public string[] Mismatches { get; set; }
	}
}
#endif
