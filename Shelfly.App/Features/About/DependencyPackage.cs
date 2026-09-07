namespace Shelfly.App.Features.About;

public record DependencyPackage(
    string PackageId,
    string PackageVersion,
    string? Authors,
    string? License,
    Uri? LicenseUrl);
