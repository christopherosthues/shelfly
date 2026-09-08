using System.Text.Json;
using Shelfly.Common;

namespace Shelfly.App.Features.About;

public class LicenseDataService
{
    private const string LicensesFileName = "licenses-release.json";

    public async Task<Result<List<DependencyPackage>>> LoadDependenciesAsync(CancellationToken cancellationToken = default)
    {
        Stream jsonStream;

        try
        {
            jsonStream = await FileSystem.Current.OpenAppPackageFileAsync(LicensesFileName);
        }
        catch (FileNotFoundException ex)
        {
            return Result<List<DependencyPackage>>.Failure($"Failed to load dependency data: {ex.Message}");
        }

        try
        {
            await using (jsonStream)
            {
                using JsonDocument doc = await JsonDocument.ParseAsync(jsonStream, cancellationToken: cancellationToken);

                return Result<List<DependencyPackage>>.Success(ParseDependencies(doc));
            }
        }
        catch (JsonException ex)
        {
            return Result<List<DependencyPackage>>.Failure($"Failed to load dependency data: {ex.Message}");
        }
    }

    private static List<DependencyPackage> ParseDependencies(JsonDocument doc)
    {
        List<DependencyPackage> dependencies = [];

        foreach (JsonElement element in doc.RootElement.EnumerateArray())
        {
            string packageId = element.GetProperty("PackageId").GetString() ?? "Unknown";
            string? version = element.GetProperty("PackageVersion").GetString();
            string? authors = element.GetProperty("Authors").GetString();
            string? license = element.GetProperty("License").GetString();
            string? licenseUrlString = element.GetProperty("LicenseUrl").GetString();

            Uri? licenseUri = licenseUrlString != null ? new Uri(licenseUrlString) : null;
            LicenseInformationOrigin licenseOrigin = ParseLicenseOrigin(element);

            dependencies.Add(new DependencyPackage(
                packageId,
                version ?? "Unknown",
                authors,
                license,
                licenseUri,
                licenseOrigin));
        }

        return dependencies;
    }

    private static LicenseInformationOrigin ParseLicenseOrigin(JsonElement element)
    {
        if (element.TryGetProperty("LicenseInformationOrigin", out JsonElement originElement)
            && originElement.TryGetInt32(out int originValue)
            && Enum.IsDefined(typeof(LicenseInformationOrigin), originValue))
        {
            return (LicenseInformationOrigin)originValue;
        }

        return LicenseInformationOrigin.Unknown;
    }

    public async Task<Result<string>> LoadLicenseFileContentAsync(DependencyPackage package, CancellationToken cancellationToken = default)
    {
        string fileName = $"licenses/{package.PackageId}__{package.PackageVersion}.txt";

        Stream fileStream;

        try
        {
            fileStream = await FileSystem.Current.OpenAppPackageFileAsync(fileName);
        }
        catch (FileNotFoundException ex)
        {
            return Result<string>.Failure($"Failed to load license file for {package.PackageId}: {ex.Message}");
        }

        await using (fileStream)
        {
            using StreamReader reader = new(fileStream);
            string content = await reader.ReadToEndAsync(cancellationToken);

            return Result<string>.Success(content);
        }
    }
}
