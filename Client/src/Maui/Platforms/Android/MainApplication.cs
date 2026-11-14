using Android.App;
using Android.Runtime;

namespace AlertOne.Maui.Platforms.Android;

/// <summary>
/// The Android application class.
/// </summary>
[Application(Label = "AlertOne")]
public class MainApplication : MauiApplication
{
    /// <summary>
    /// Initializes the application from its native handle.
    /// </summary>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <summary>
    /// Creates the MAUI app.
    /// </summary>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
