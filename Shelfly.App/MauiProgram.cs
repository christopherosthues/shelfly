using Bogus;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
#if RELEASE
using NLog.Extensions.Logging;
#endif
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.About;
using Shelfly.App.Features.BookEditor;
using Shelfly.App.Features.BookmarkEditor;
using Shelfly.App.Features.Library;
using Shelfly.App.Features.Trash;
using Shelfly.App.Migrations;

namespace Shelfly.App;

public static class MauiProgram
{

    private static string GenerateIsbn(Randomizer random)
    {
        long prefix = random.Int(978, 979);
        long body = random.Long(0, 999999999L);
        long isbn12 = (prefix * 1_000_000_000L) + body;

        long checksum = 0;
        for (int i = 0; i < 12; i++)
        {
            long digit = isbn12 % 10;
            isbn12 /= 10;
            checksum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (int)(10 - (checksum % 10));
        return $"{prefix}{body:D9}{checkDigit}";
    }

    public static MauiApp CreateMauiApp()
    {
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nlog.config");
        if (File.Exists(configPath))
        {
            LogManager.Setup().LoadConfigurationFromFile(configPath);
        }

        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#else
        builder.Logging.ClearProviders().AddNLog();
#endif

        builder.Services.AddDbContext<LocalDbContext>(options =>
        {
            string databasePath = Path.Combine(FileSystem.AppDataDirectory, "shelfly.db");
            options.UseSqlite($"Data Source={databasePath}",
                sql => sql.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
            options.AddInterceptors(new AuditTimestampInterceptor());

            options.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                if (!await context.Set<BookEntity>().AnyAsync(cancellationToken))
                {
                    Faker<BookEntity> faker = new Faker<BookEntity>()
                        .RuleFor(a => a.Id, f => Guid.CreateVersion7())
                        .RuleFor(a => a.Title, f => f.Commerce.ProductName())
                        .RuleFor(a => a.Author, f => f.Name.FullName())
                        .RuleFor(a => a.ISBN, f => GenerateIsbn(f.Random))
                        .RuleFor(a => a.Publisher, f => f.Company.CompanyName())
                        .RuleFor(a => a.PublishDate, f => f.Date.Past())
                        .RuleFor(a => a.CreatedAt, f => f.Date.Past())
                        .RuleFor(a => a.LastModifiedAt,
                            (f, b) => f.Random.Bool(20)
                                ? f.Date.Between(b.CreatedAt, DateTime.UtcNow)
                                : null);

                    List<BookEntity> books = faker.Generate(6);

                    // Soft-delete 3 of them
                    foreach (BookEntity book in books.Take(3))
                    {
                        book.DeletedAt = DateTime.UtcNow;
                    }

                    context.Set<BookEntity>().AddRange(books);

                    // Add bookmarks to some books (not all)
                    Faker<BookmarkEntity> bookmarkFaker = new Faker<BookmarkEntity>()
                        .RuleFor(b => b.Id, f => Guid.CreateVersion7())
                        .RuleFor(b => b.BookId, f => f.Random.Guid())
                        .RuleFor(b => b.StartPage, f => f.Random.Int(1, 500))
                        .RuleFor(b => b.EndPage,
                            (f, b) => f.Random.Bool(40) ? b.StartPage + f.Random.Int(0, 500) : null)
                        .RuleFor(b => b.Note, f => f.Random.Bool(60) ? f.Lorem.Sentence() : null)
                        .RuleFor(b => b.CreatedAt, f => f.Date.Recent())
                        .RuleFor(b => b.LastModifiedAt,
                            (f, b) => f.Random.Bool(20)
                                ? f.Date.Between(b.CreatedAt, DateTime.UtcNow)
                                : null);

                    List<BookmarkEntity> bookmarksToAdd = new List<BookmarkEntity>();

                    // Add 3 bookmarks to book[3]
                    foreach (BookmarkEntity bm in bookmarkFaker.Generate(3))
                    {
                        bm.BookId = books[3].Id;
                        bookmarksToAdd.Add(bm);
                    }

                    // Add 2 bookmarks to book[4]
                    foreach (BookmarkEntity bm in bookmarkFaker.Generate(2))
                    {
                        bm.BookId = books[4].Id;
                        bookmarksToAdd.Add(bm);
                    }

                    context.Set<BookmarkEntity>().AddRange(bookmarksToAdd);

                    await context.SaveChangesAsync(cancellationToken);
                }
            });
        });
        builder.Services.AddScoped<AuditTimestampInterceptor>();
        builder.Services.AddScoped<LibraryService>();
        builder.Services.AddScoped<LibraryExportService>();
        builder.Services.AddScoped<TrashService>();
        builder.Services.AddScoped<LicenseDataService>();

        builder.Services.AddScopedWithShellRoute<BookListPage, BookListViewModel>(Routes.BookListPage);
        builder.Services.AddScopedWithShellRoute<BookEditPage, BookEditViewModel>(Routes.BookEditPage);
        builder.Services.AddScopedWithShellRoute<BookDetailPage, BookDetailViewModel>(Routes.BookDetailPage);
        builder.Services.AddScopedWithShellRoute<BookmarkEditPage, BookmarkEditViewModel>(Routes.BookmarkEditPage);
        builder.Services.AddScopedWithShellRoute<TrashListPage, TrashListViewModel>(Routes.TrashListPage);
        builder.Services.AddScopedWithShellRoute<TrashBookDetailPage, TrashBookDetailViewModel>(Routes.TrashBookDetailPage);
        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddScopedWithShellRoute<AboutPage, AboutViewModel>(Routes.AboutPage);

        return builder.Build();
    }
}
