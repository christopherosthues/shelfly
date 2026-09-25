namespace Shelfly.Configuration;

public interface IJsonConfiguration
{
    Dictionary<string, string?> ToFlatJsonDictionary(string prefix, Dictionary<string, string?> dictionary);
}