using System;
using System.IO;
#if DEBUG
using Knighter.QA;
#endif

// O jogo legado carrega vários JSONs/PNGs por caminho relativo. Um executável do
// Windows pode ser iniciado por um atalho, launcher ou ferramenta com outro diretório
// de trabalho, então fixamos a raiz no diretório publicado do próprio jogo.
string launchDirectory = Environment.CurrentDirectory;
// SDL extended Bluetooth reports enable DualShock / DualSense rumble.
Environment.SetEnvironmentVariable("SDL_JOYSTICK_HIDAPI_PS4_RUMBLE", "1");
Environment.SetEnvironmentVariable("SDL_JOYSTICK_HIDAPI_PS5_RUMBLE", "1");
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
	try
	{
		string logPath = Path.Combine(AppContext.BaseDirectory, "crash_log.txt");
		File.WriteAllText(logPath, e.ExceptionObject?.ToString() ?? "Unknown unhandled crash");
		Knighter.Gameplay.Rumble.Stop();
	}
	catch { }
	Environment.Exit(1);
};

Exception failure = null;
try
{
#if DEBUG
	QaSession.Configure(args, launchDirectory);
#endif
	using var game = new Knighter.MobileGame();
	game.Run();
}
catch (Exception ex)
{
	failure = ex;
	try
	{
		string logPath = Path.Combine(AppContext.BaseDirectory, "crash_log.txt");
		File.WriteAllText(logPath, ex.ToString());
	}
	catch { }
	Console.WriteLine("ERRO CAPTURADO - veja crash_log.txt:");
	Console.WriteLine(ex);
	try
	{
		Knighter.Gameplay.Rumble.Stop();
	}
	catch { }
}
finally
{
#if DEBUG
	QaSession.Shutdown(failure);
#endif
	if (failure != null)
	{
		Environment.Exit(1);
	}
}
