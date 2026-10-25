
 // SPDX-License-Identifier: GPL-3.0-or-later
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
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        Settings.OverrideRoot = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EarthBoundCompanion");
        return base.CustomizeAppBuilder(builder).WithInterFont();
    }
}

public sealed class CompanionIosApp : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
        {
            // Native SDL gameplay launch and ROM setup are not yet implemented.
            // A missing mobileLaunch callback disables the gameplay launch path;
            // this is an interface-integration checkpoint, not a playable release.
            lifetime.MainView = new MainView();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
