using System.Net.Http.Json;
using Shelfly.Common.DTOs;

namespace Shelfly.App.Services;

public class ServerConnectionService
{
    private string _serverUrl = "http://localhost:5000/";
    public string ServerUrl => _serverUrl;
    public bool IsConnected { get; private set; }
    public DateTimeOffset? LastSynced { get; private set; }

    public async Task<bool> SetServerUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        _serverUrl = url.TrimEnd('/') + "/";
        IsConnected = await CheckReachabilityAsync();
        return IsConnected;
    }

    public async Task<bool> CheckReachabilityAsync()
    {
        try
        {
            HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(_serverUrl);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            IsConnected = false;
            return false;
        }
    }

    public async Task<SyncStatusResponse?> GetSyncStatusAsync()
    {
        try
        {
            HttpClient client = new();
            SyncStatusResponse? status = await client.GetFromJsonAsync<SyncStatusResponse>($"{_serverUrl}api/sync/status/{Uri.EscapeDataString(_serverUrl)}");
            if (status != null)
            {
                LastSynced = status.LastSynced;
            }
            return status;
        }
        catch (Exception)
        {
            IsConnected = false;
            return new SyncStatusResponse
            {
                Reachable = false,
                PendingChanges = 0
            };
        }
    }

    public void ClearConnection()
    {
        _serverUrl = "http://localhost:5000/";
        IsConnected = false;
        LastSynced = null;
    }

    internal void UpdateLastSynced(DateTimeOffset timestamp)
    {
        LastSynced = timestamp;
    }
}
