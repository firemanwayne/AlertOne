using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace AlertOne.Maui;

/// <summary>
/// Entry point for the Tizen app.
/// </summary>
class Program : MauiApplication
{
	/// <summary>
	/// Creates the MAUI app.
	/// </summary>
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	/// <summary>
	/// Starts the application.
	/// </summary>
	static void Main(string[] args)
	{
		var app = new Program();
		app.Run(args);
	}
}
