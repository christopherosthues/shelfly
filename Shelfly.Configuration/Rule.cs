namespace Shelfly.Configuration;

public record Rule(
    string EndpointPattern,
    List<string> RequiredRoles);
