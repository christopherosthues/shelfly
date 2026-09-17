using System.Security.Cryptography;

namespace Shelfly.App.Features.Settings.Services;

public sealed class CredentialStore
{
    private readonly string _storePath;
    private static readonly byte[] Salt = Guid.CreateVersion7().ToByteArray();
    private static readonly byte[] Key = Aes.Create().Key;

    public CredentialStore()
    {
        string fileName = Path.Combine(FileSystem.AppDataDirectory, ".shelfly_credentials");
        _storePath = fileName;
    }

    public void SaveCredentials(Guid serverEntryId, string username, string email, string jwtToken)
    {
        Dictionary<string, string> store = LoadStore();

        string entryKey = serverEntryId.ToString("N");
        string plainData = $"{username}|{email}|{jwtToken}";
        string encrypted = Encrypt(plainData);

        store[entryKey] = encrypted;
        SaveStore(store);
    }

    public (string Username, string Email, string JwtToken)? LoadCredentials(Guid serverEntryId)
    {
        Dictionary<string, string> store = LoadStore();
        string entryKey = serverEntryId.ToString("N");

        if (store.TryGetValue(entryKey, out string? encrypted))
        {
            string plainData = Decrypt(encrypted);
            string[] parts = plainData.Split('|');
            return parts.Length >= 3
                ? (parts[0], parts[1], parts[2])
                : null;
        }

        return null;
    }

    public void ClearCredentials(Guid serverEntryId)
    {
        Dictionary<string, string> store = LoadStore();
        string entryKey = serverEntryId.ToString("N");
        store.Remove(entryKey);
        SaveStore(store);
    }

    private Dictionary<string, string> LoadStore()
    {
        if (!File.Exists(_storePath))
        {
            return new Dictionary<string, string>();
        }

        string json = File.ReadAllText(_storePath);
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }

    private void SaveStore(Dictionary<string, string> store)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(store);
        File.WriteAllText(_storePath, json);
    }

    private static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Salt[..16];

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    private static string Decrypt(string cipherText)
    {
        byte[] bytes = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Salt[..16];

        using MemoryStream ms = new(bytes);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new(cs);

        return sr.ReadToEnd();
    }
}
