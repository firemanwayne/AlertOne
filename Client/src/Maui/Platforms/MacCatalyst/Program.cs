using UIKit;

namespace AlertOne.Maui.Platforms.MacCatalyst;

/// <summary>
/// Entry point for the Mac Catalyst app.
/// </summary>
public class Program
{
    // This is the main entry point of the application.
    /// <summary>
    /// Starts the application.
    /// </summary>
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}