namespace Shelfly.App.Models;

public record DependencyPackage(
    string PackageId,
    string PackageVersion,
    string? Authors,
    string? License,
    Uri? LicenseUrl);
