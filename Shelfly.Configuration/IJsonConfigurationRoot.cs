namespace Shelfly.Configuration;

public interface IJsonConfigurationRoot
{
    Dictionary<string, string?> ToFlatJsonDictionary();
}