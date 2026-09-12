using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using System.IO;

#pragma warning disable CA1422
[Activity(Label = "Redungeon", MainLauncher = true, Exported = true,
    Theme = "@android:style/Theme.NoTitleBar.Fullscreen",
    ScreenOrientation = ScreenOrientation.Sensor,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
public sealed class MainActivity : AndroidGameActivity
{
    private Knighter.MobileGame game;
    protected override void OnCreate(Bundle state)
    {
        base.OnCreate(state);
        var vibrator = (Vibrator)GetSystemService(VibratorService);
        Knighter.Gameplay.Rumble.PhonePulse = (strength, milliseconds) =>
        {
            if (vibrator?.HasVibrator != true) return;
            int amplitude = vibrator.HasAmplitudeControl ? System.Math.Clamp((int)(strength * 255), 1, 255) : VibrationEffect.DefaultAmplitude;
            using var effect = VibrationEffect.CreateOneShot(milliseconds, amplitude);
            vibrator.Vibrate(effect);
        };
        Knighter.Gameplay.Rumble.PhoneStop = () => vibrator?.Cancel();
        HideSystemBars();
        Directory.SetCurrentDirectory(FilesDir.AbsolutePath);
        CopyAssets("Content");
        game = new Knighter.MobileGame();
        SetContentView((View)game.Services.GetService(typeof(View)));
        game.Run();
    }
    private void CopyAssets(string path)
    {
        var children = Assets.List(path);
        if (children.Length > 0)
        {
            Directory.CreateDirectory(path);
            foreach (var child in children) CopyAssets(path + "/" + child);
        }
        else
        {
            using var input = Assets.Open(path);
            using var output = File.Create(path);
            input.CopyTo(output);
        }
    }
    protected override void OnPause()
    {
        game?.OnEnteringBackground();
        base.OnPause();
    }

    protected override void OnDestroy()
    {
        Knighter.Gameplay.Rumble.Stop();
        Knighter.Gameplay.Rumble.PhonePulse = null;
        Knighter.Gameplay.Rumble.PhoneStop = null;
        base.OnDestroy();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        if (hasFocus) HideSystemBars();
    }

    private void HideSystemBars()
    {
        Window.AddFlags(WindowManagerFlags.Fullscreen);
        Window.DecorView.SystemUiFlags =
            SystemUiFlags.Fullscreen |
            SystemUiFlags.HideNavigation |
            SystemUiFlags.ImmersiveSticky |
            SystemUiFlags.LayoutStable |
            SystemUiFlags.LayoutFullscreen |
            SystemUiFlags.LayoutHideNavigation;
    }
}
