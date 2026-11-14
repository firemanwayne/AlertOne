namespace AlertOne.Maui;

/// <summary>
/// The MAUI application.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the application and its XAML resources.
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Creates the main window, which shows <see cref="MainPage"/>.
    /// </summary>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "AlertOne.Maui" };
    }
}
