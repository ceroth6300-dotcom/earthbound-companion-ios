
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.iOS;
using Avalonia.Themes.Fluent;

using Foundation;
using UIKit;
using EarthBoundCompanion;

namespace EarthBoundCompanion.iOS;

[Register("AppDelegate")]
public sealed class AppDelegate
    : AvaloniaAppDelegate<CompanionIosApp>
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
            lifetime.MainView = new MainView(
                mobileSetup: SetupRomAsync
            );
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task SetupRomAsync(
        string romPath,
        bool redux,
        IProgress<string> progress)
    {
        if (redux)
        {
            throw new NotSupportedException(
                "Redux ROM conversion is not yet " +
                "available on iOS.");
        }

        progress.Report(
            "Checking EarthBound ROM...");

        string privateFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder
                    .LocalApplicationData),
            "EarthBoundCompanion",
            "RomImports");

        string validatedRom =
            await IosRomPreparation.PrepareAsync(
                romPath,
                privateFolder);

        progress.Report(
            "ROM validated and saved locally.");

        // The ROM is valid, but the native asset
        // converter is not integrated yet.
        // Do not report setup as completed.

        throw new NotSupportedException(
            "ROM verified successfully at " +
            validatedRom +
            ". The iOS assets.pak converter " +
            "is not implemented yet.");
    }
}

internal static class IosRomPreparation
{
    private const string ExpectedSha256 =
        "A8FE2226728002786D68C27DDDDF0B90A894DB52E4DFE268FDF72A68CAE5F02E";

    private const long RomSize = 3145728;
    private const long HeaderedRomSize = 3146240;

    public static async Task<string> PrepareAsync(
        string romPath,
        string privateDirectory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(romPath) ||
            !File.Exists(romPath))
        {
            throw new FileNotFoundException(
                "Select a valid EarthBound ROM.");
        }

        Directory.CreateDirectory(
            privateDirectory);

        string destination = Path.Combine(
            privateDirectory,
            "earthbound-import.sfc");

        string temporary =
            destination + ".tmp";

        try
        {
            await using (var input =
                File.OpenRead(romPath))
            await using (var output =
                File.Create(temporary))
            {
                if (input.Length == HeaderedRomSize)
                {
                    // Remove optional copier header.
                    input.Position = 512;
                }
                else if (input.Length != RomSize)
                {
                    throw new InvalidDataException(
                        "Unsupported ROM size. " +
                        "Use a clean EarthBound USA ROM.");
                }

                await input.CopyToAsync(
                    output,
                    cancellationToken);
            }

            await using (var stream =
                File.OpenRead(temporary))
            {
                using var sha =
                    SHA256.Create();

                byte[] hash =
                    await sha.ComputeHashAsync(
                        stream,
                        cancellationToken);

                string actual =
                    Convert.ToHexString(hash);

                if (!actual.Equals(
                    ExpectedSha256,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        "ROM verification failed. " +
                        "This is not the supported " +
                        "clean EarthBound USA ROM.");
                }
            }

            File.Move(
                temporary,
                destination,
                overwrite: true);

            return destination;
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}
