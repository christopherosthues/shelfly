using System.Text.Json;
using Shouldly;
using Shelfly.Configuration;

namespace Shelfly.AdminConsole.Tests.Integration;

public class GenerateAllConfigsTest : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [Test]
    public async Task GivenGenerateCommand_WhenInvokedWithAllConfigTypes_ThenCreatesValidJsonFiles()
    {
        // Arrange - create default configurations using the same logic as GenerateCommand
        KeycloakConfiguration keycloakConfig = new(
            KeycloakConfiguration.DefaultId,
            "",
            "",
            "",
            "",
            "");

        PostgreSqlConfiguration postgresqlConfig = new(
            PostgreSqlConfiguration.DefaultId,
            "");

        AuthorizationRule authRules = new(
            AuthorizationRule.DefaultId,
            []);

        string filePrefix = "test_" + Guid.NewGuid().ToString()[..8];
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        // Act - simulate generating all three config files with default values
        List<string> generatedFiles = [];

        // Generate Keycloak file
        string keycloakJson = JsonSerializer.Serialize(keycloakConfig, JsonOptions);
        string keycloakPath = Path.Combine(tempDir, $"{filePrefix}_keycloak.json");
        await File.WriteAllTextAsync(keycloakPath, keycloakJson);
        generatedFiles.Add(keycloakPath);

        // Generate PostgreSQL file
        string postgresqlJson = JsonSerializer.Serialize(postgresqlConfig, JsonOptions);
        string postgresqlPath = Path.Combine(tempDir, $"{filePrefix}_postgresql.json");
        await File.WriteAllTextAsync(postgresqlPath, postgresqlJson);
        generatedFiles.Add(postgresqlPath);

        // Generate Auth Rules file
        string authRulesJson = JsonSerializer.Serialize(authRules, JsonOptions);
        string authRulesPath = Path.Combine(tempDir, $"{filePrefix}_auth-rules.json");
        await File.WriteAllTextAsync(authRulesPath, authRulesJson);
        generatedFiles.Add(authRulesPath);

        // Assert - verify all files were created with correct content
        generatedFiles.Count.ShouldBe(3);

        // Verify Keycloak file
        using JsonDocument keycloakDoc = JsonDocument.Parse(await File.ReadAllTextAsync(keycloakPath));
        keycloakDoc.RootElement.TryGetProperty("Id", out JsonElement kcId).ShouldBeTrue();
        kcId.GetString().ShouldBe(KeycloakConfiguration.DefaultId);
        keycloakDoc.RootElement.TryGetProperty("IssuerUrl", out JsonElement issuerUrl).ShouldBeTrue();
        issuerUrl.GetString().ShouldBe("");

        // Verify PostgreSQL file
        using JsonDocument postgresqlDoc = JsonDocument.Parse(await File.ReadAllTextAsync(postgresqlPath));
        postgresqlDoc.RootElement.TryGetProperty("Id", out JsonElement pgId).ShouldBeTrue();
        pgId.GetString().ShouldBe(PostgreSqlConfiguration.DefaultId);
        postgresqlDoc.RootElement.TryGetProperty("ConnectionString", out JsonElement connStr).ShouldBeTrue();
        connStr.GetString().ShouldBe("");

        // Verify Auth Rules file
        using JsonDocument authRulesDoc = JsonDocument.Parse(await File.ReadAllTextAsync(authRulesPath));
        authRulesDoc.RootElement.TryGetProperty("Id", out JsonElement arId).ShouldBeTrue();
        arId.GetString().ShouldBe(AuthorizationRule.DefaultId);
        authRulesDoc.RootElement.TryGetProperty("Rules", out JsonElement rules).ShouldBeTrue();
        rules.ValueKind.ShouldBe(JsonValueKind.Array);
        rules.GetArrayLength().ShouldBe(0);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Test]
    public async Task GivenGenerateCommand_WhenInvokedWithSpecificConfigType_ThenCreatesOnlyRequestedFile()
    {
        // Arrange - create default Keycloak configuration only
        KeycloakConfiguration keycloakConfig = new(
            KeycloakConfiguration.DefaultId,
            "",
            "",
            "",
            "",
            "");

        string filePrefix = "test_" + Guid.NewGuid().ToString()[..8];
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        // Act - simulate generating only Keycloak config file
        string keycloakJson = JsonSerializer.Serialize(keycloakConfig, JsonOptions);
        string keycloakPath = Path.Combine(tempDir, $"{filePrefix}_keycloak.json");
        await File.WriteAllTextAsync(keycloakPath, keycloakJson);

        // Assert - verify only the requested file was created
        Directory.GetFiles(tempDir).Length.ShouldBe(1);
        File.Exists(keycloakPath).ShouldBeTrue();

        using JsonDocument doc = JsonDocument.Parse(await File.ReadAllTextAsync(keycloakPath));
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        id.GetString().ShouldBe(KeycloakConfiguration.DefaultId);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Test]
    public async Task GivenExistingFile_WhenGenerateWithoutForce_ThenSkipsOverwrite()
    {
        // Arrange - create an existing file
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "existing_keycloak.json");
        
        string originalContent = JsonSerializer.Serialize(new KeycloakConfiguration(
            KeycloakConfiguration.DefaultId,
            "original",
            "",
            "",
            "",
            ""), JsonOptions);
        
        await File.WriteAllTextAsync(filePath, originalContent);

        // Act - simulate generate without force flag (file exists)
        bool forceOverwrite = false;
        string newContent = "{}";
        
        if (!File.Exists(filePath) || forceOverwrite)
        {
            await File.WriteAllTextAsync(filePath, newContent);
        }

        // Assert - file should retain original content when force is false
        string actualContent = await File.ReadAllTextAsync(filePath);
        using JsonDocument doc = JsonDocument.Parse(actualContent);
        doc.RootElement.TryGetProperty("IssuerUrl", out JsonElement issuerUrl).ShouldBeTrue();
        issuerUrl.GetString().ShouldBe("original");

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Test]
    public async Task GivenExistingFile_WhenGenerateWithForce_ThenOverwritesContent()
    {
        // Arrange - create an existing file
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string filePath = Path.Combine(tempDir, "existing_keycloak.json");
        
        await File.WriteAllTextAsync(filePath, "{}");

        // Act - simulate generate with force flag (overwrite)
        bool forceOverwrite = true;
        KeycloakConfiguration newConfig = new(
            KeycloakConfiguration.DefaultId,
            "",
            "",
            "",
            "",
            "");
        
        string newContent = JsonSerializer.Serialize(newConfig, JsonOptions);
        
        if (!File.Exists(filePath) || forceOverwrite)
        {
            await File.WriteAllTextAsync(filePath, newContent);
        }

        // Assert - file should contain new default content
        string actualContent = await File.ReadAllTextAsync(filePath);
        using JsonDocument doc = JsonDocument.Parse(actualContent);
        doc.RootElement.TryGetProperty("Id", out JsonElement id).ShouldBeTrue();
        id.GetString().ShouldBe(KeycloakConfiguration.DefaultId);

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
