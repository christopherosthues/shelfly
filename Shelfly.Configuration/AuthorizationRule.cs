namespace Shelfly.Configuration;

public record AuthorizationRule(
    string Id,
    List<Rule> Rules)
{
    public const string DefaultId = "auth-rules";

    public static AuthorizationRule Create(List<Rule> rules) =>
        new(DefaultId, rules);
}
