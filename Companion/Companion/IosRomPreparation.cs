
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace EarthBoundCompanion;

internal static class IosRomPreparation
{
    private const string ExpectedSha256 =
        "A8FE2226728002786D68C27DDDDF0B90A894DB52E4DFE268FDF72A68CAE5F02E";

    public static async Task<string> PrepareAsync(
        string romPath,
        string privateDirectory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(romPath) ||
            !File.Exists(romPath))
        {
            throw new FileNotFoundException(
                "Choose your EarthBound USA ROM.");
        }

        Directory.CreateDirectory(privateDirectory);

        string destination = Path.Combine(
            privateDirectory,
            "earthbound-import.sfc");

        string temporary = destination + ".tmp";

        try
        {
            await using (var input = File.OpenRead(romPath))
            await using (var output = File.Create(temporary))
            {
                if (input.Length == 3146240)
                {
                    // Skip the 512-byte copier header.
                    input.Position = 512;
                }
                else if (input.Length != 3145728)
                {
                    throw new InvalidDataException(
                        "Unsupported ROM size.");
                }

                await input.CopyToAsync(
                    output,
                    cancellationToken);
            }

            await using (var stream = File.OpenRead(temporary))
            {
                using var sha = SHA256.Create();

                byte[] hash = await sha.ComputeHashAsync(
                    stream,
                    cancellationToken);

                string actual = Convert.ToHexString(hash);

                if (!actual.Equals(
                    ExpectedSha256,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        "This is not the supported clean " +
                        "EarthBound USA ROM.");
                }
            }

            File.Move(temporary, destination, true);
            return destination;
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}
