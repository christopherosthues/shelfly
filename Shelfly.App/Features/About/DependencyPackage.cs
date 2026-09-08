namespace Shelfly.App.Features.About;

public record DependencyPackage(
    string PackageId,
    string PackageVersion,
    string? Authors,
    string? License,
    Uri? LicenseUrl,
    LicenseInformationOrigin LicenseOrigin)
{
    public bool IsFileBased => LicenseOrigin == LicenseInformationOrigin.File;
}
