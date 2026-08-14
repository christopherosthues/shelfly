namespace Shelfly.Configuration;

public record KeycloakConfiguration(
    string Id,
    string IssuerUrl,
    string Audience,
    string JwksEndpoint,
    string AdminClientId,
    string AdminClientSecret)
{
    public const string DefaultId = "keycloak";

    public static KeycloakConfiguration Create(
        string issuerUrl,
        string audience,
        string jwksEndpoint,
        string adminClientId,
        string adminClientSecret) =>
        new(DefaultId, issuerUrl, audience, jwksEndpoint, adminClientId, adminClientSecret);
}
