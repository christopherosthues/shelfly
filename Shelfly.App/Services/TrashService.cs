using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Data.Repositories;

namespace Shelfly.App.Services;

public class TrashService(
    IBookRepository bookRepository,
    ILogger<TrashService> logger) : IDisposable
{
    private readonly PeriodicTimer _cleanupTimer = new(TimeSpan.FromHours(1));
    private bool _cleanupEnabled = false;
    private int _retentionDays = 30;

    public ICommand CleanupCommand { get; }
    public ICommand RestoreBookCommand { get; }

    public TrashService() : this(
        new BookRepository(new LocalDbContext((DbContextOptions<LocalDbContext>)new DbContextOptionsBuilder().UseSqlite($"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "shelfly_local.db")}").Options)),
        new Logger<TrashService>(new LoggerFactory()))
    {
        CleanupCommand = new Command(async () => await CleanupAsync());
        RestoreBookCommand = new Command<Guid>(async bookId => await RestoreBookAsync(bookId));
    }

    public void Configure(bool cleanupEnabled, int retentionDays)
    {
        _cleanupEnabled = cleanupEnabled;
        _retentionDays = retentionDays;

        if (_cleanupEnabled)
        {
            Task.Run(async () =>
            {
                while (await _cleanupTimer.WaitForNextTickAsync())
                {
                    await CleanupAsync();
                }
            });
        }
    }

    public async Task<List<LocalBook>> GetTrashItemsAsync()
    {
        List<LocalBook>? allBooks = await bookRepository.GetAllAsync();
        return (allBooks?.Where(b => b.DeletionStatus == Shelfly.Common.Enums.DeletionStatus.SoftDeleted).ToList()) ?? [];
    }

    public async Task CleanupAsync()
    {
        if (!_cleanupEnabled)
        {
            return;
        }

        try
        {
            List<LocalBook>? trashItems = await GetTrashItemsAsync();
            DateTimeOffset cutoffTime = DateTimeOffset.UtcNow - TimeSpan.FromDays(_retentionDays);

            foreach (LocalBook book in trashItems)
            {
                if (book.LastModified < cutoffTime)
                {
                    // Permanently delete books older than retention period (physical row removal — hard delete)
                    await bookRepository.DeleteAsync(book.LocalGuid);

                    logger.LogInformation("Hard-deleted book '{Title}' from trash", book.Title);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during trash cleanup");
        }
    }

    public async Task<LocalBook?> RestoreBookAsync(Guid bookId)
    {
        LocalBook? book = await bookRepository.GetByIdAsync(bookId);
        if (book != null && book.DeletionStatus == Shelfly.Common.Enums.DeletionStatus.SoftDeleted)
        {
            book.DeletionStatus = Shelfly.Common.Enums.DeletionStatus.Active;
            await bookRepository.UpdateAsync(book);

            logger.LogInformation("Restored book '{Title}' from trash", book.Title);
        }

        return book;
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }
}
