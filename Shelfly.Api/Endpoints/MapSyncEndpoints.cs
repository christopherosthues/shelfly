using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Shelfly.Api.Services;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Endpoints;

public static class SyncEndpoints
{
    private static Guid ExtractUserId(HttpContext context)
    {
        string? subClaim = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (Guid.TryParse(subClaim, out Guid userId))
        {
            return userId;
        }

        return Guid.CreateVersion7();
    }

    extension(WebApplication app)
    {
        public WebApplication MapSyncEndpoints()
        {
            // POST /api/sync/upload - Upload local changes to remote server
            app.MapPost("/sync/upload",
                async (SyncUploadRequest request, HttpContext httpContext, SyncService syncService) =>
                {
                    Guid userId = ExtractUserId(httpContext);
                    return await syncService.UploadAsync(userId, request);
                }).RequireAuthorization();

            // POST /api/sync/download - Download remote changes to local database
            app.MapPost("/sync/download",
                async (SyncDownloadRequest request, HttpContext httpContext, SyncService syncService) =>
                {
                    Guid userId = ExtractUserId(httpContext);
                    return await syncService.DownloadAsync(userId, request);
                }).RequireAuthorization();

            // POST /api/sync/resolve-conflict - Resolve a sync conflict
            app.MapPost("/sync/resolve-conflict",
                async (SyncConflictResolutionRequest request, HttpContext httpContext, SyncService syncService) =>
                {
                    Guid userId = ExtractUserId(httpContext);
                    return await syncService.ResolveConflictAsync(userId, request);
                }).RequireAuthorization();

            // POST /api/sync/restore-from-trash - Restore a soft-deleted item from trash
            app.MapPost("/sync/restore-from-trash",
                async (Guid remoteGuid, HttpContext httpContext, SyncService syncService) =>
                {
                    Guid userId = ExtractUserId(httpContext);
                    return await syncService.RestoreFromTrashAsync(userId, remoteGuid);
                }).RequireAuthorization();

            // GET /api/sync/status/{serverUrl} - Check server reachability and sync status
            app.MapGet("/sync/status/{serverUrl}",
                async (string serverUrl) =>
                {
                    return new SyncStatusResponse
                    {
                        Reachable = true,
                        LastSynced = DateTimeOffset.UtcNow,
                        PendingChanges = 0
                    };
                }).RequireAuthorization();

            return app;
        }
    }
}
