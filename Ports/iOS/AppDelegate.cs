
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.iOS;
using Avalonia.Themes.Fluent;
using Foundation;
using UIKit;
using EarthBoundCompanion;

namespace EarthBoundCompanion.iOS;

[Register("AppDelegate")]
public sealed class AppDelegate : AvaloniaAppDelegate<CompanionIosApp>
{
    protected override AppBuilder CustomizeAppBuilder(
        AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}

public sealed class CompanionIosApp : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant =
            Avalonia.Styling.ThemeVariant.Dark;

        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is
            ISingleViewApplicationLifetime lifetime)
        {
            lifetime.MainView = new MainView();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
