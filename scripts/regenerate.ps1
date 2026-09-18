param(
    [string]$Target = "all",
    [string]$Image = "mcr.microsoft.com/openapi/kiota:latest"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Write-Status {
    param([string]$Message, [string]$Color = "Yellow")
    Write-Host "$Message" -ForegroundColor $Color
}

function Invoke-ClientGeneration {
    Write-Status "Generating client-side code (Kiota C#)..." Green

    $servicesPath = Join-Path $RepoRoot "Shelfly.App\Services"

    # Clean existing generated files
    if (Test-Path $servicesPath) {
        Get-ChildItem -Path $servicesPath -Recurse | ForEach-Object {
            if ($_.PSIsContainer) {
                Remove-Item -LiteralPath $_.FullName -Recurse -Force
            }
            else {
                Remove-Item -LiteralPath $_.FullName -Force
            }
        }
    }

    $podmanArgs = @(
        "run", "--rm",
        "-v", "$($RepoRoot):/local",
        $Image,
        "generate",
        "--language", "csharp",
        "--openapi", "/local/shelfly/openapi/shelfly-api.yaml",
        "--output", "/local/shelfly/Shelfly.App/Services",
        "--namespace-name", "Shelfly.Api.Client",
        "--class-name", "ShelflyClient",
        "--clean-output"
    )

    Write-Status "Running: podman $($podmanArgs -join ' ')" Cyan

    $result = & podman @podmanArgs | Write-Host

    if ($LASTEXITCODE -eq 0) {
        Write-Status "Client code generated to Shelfly.App/Services/" Green
    }
    else {
        Write-Status "Client generation failed (exit code: $($LASTEXITCODE))." Red
    }

    return $LASTEXITCODE
}

function Invoke-Validation {
    Write-Status "Validating OpenAPI specification..." Yellow

    $podmanArgs = @(
        "run", "--rm",
        "-v", "$($RepoRoot):/local",
        $Image,
        "show",
        "--openapi", "/local/shelfly/openapi/shelfly-api.yaml"
    )

    & podman @podmanArgs | Write-Host

    return $LASTEXITCODE
}

# Main execution
Write-Host ""
Write-Status "Shelfly OpenAPI Regeneration Script (Kiota)" Magenta
Write-Host ""

switch ($Target) {
    "validate" {
        $exitCode = Invoke-Validation
        if ($exitCode -eq 0) {
            Write-Status "Specification valid." Green
        }
        else {
            Write-Status "Specification validation failed (exit code: $($exitCode))." Red
        }
    }
    "client" {
        $exitCode = Invoke-ClientGeneration
    }
    "all" {
        Write-Status "Running full regeneration (validate → client)..." Yellow

        $validationExit = Invoke-Validation
        if ($validationExit -eq 0) {
            $clientExit = Invoke-ClientGeneration

            if ($clientExit -eq 0) {
                Write-Status "Full regeneration complete." Green
            }
            else {
                Write-Status "Regeneration completed with errors." Yellow
            }
        }
        else {
            Write-Status "Validation failed. Aborting generation." Red
        }
    }
}

Write-Host ""
exit $LASTEXITCODE
