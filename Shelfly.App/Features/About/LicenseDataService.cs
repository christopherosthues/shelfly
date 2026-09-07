using System.Text.Json;

namespace Shelfly.App.Features.About;

public class LicenseDataService
{
    private readonly string _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "licenses-release.json");

    public async Task<List<DependencyPackage>> LoadDependenciesAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_jsonFilePath))
        {
            return [new DependencyPackage("Error", "", "Failed to load dependency data", "", null)];
        }

        string jsonContent = await File.ReadAllTextAsync(_jsonFilePath, cancellationToken);

        using JsonDocument doc = JsonDocument.Parse(jsonContent);

        List<DependencyPackage> dependencies = [];

        foreach (JsonElement element in doc.RootElement.EnumerateArray())
        {
            string packageId = element.GetProperty("PackageId").GetString() ?? "Unknown";
            string? version = element.GetProperty("PackageVersion").GetString();
            string? authors = element.GetProperty("Authors").GetString();
            string? license = element.GetProperty("License").GetString();
            string? licenseUrlString = element.GetProperty("LicenseUrl").GetString();

            Uri? licenseUri = licenseUrlString != null ? new Uri(licenseUrlString) : null;

            dependencies.Add(new DependencyPackage(
                packageId,
                version ?? "Unknown",
                authors,
                license,
                licenseUri));
        }

        return dependencies;
    }
}
