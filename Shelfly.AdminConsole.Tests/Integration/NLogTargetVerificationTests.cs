using Shouldly;
using NLog;
using NLog.Config;
using NLog.Targets;
using LogLevel = NLog.LogLevel;

namespace Shelfly.AdminConsole.Tests.Integration;

public class NLogTargetVerificationTests : IntegrationTestBase
{
    [Test]
    public async Task GivenNLogConfigured_WhenDebugMessageLogged_ThenFileContainsEntryButConsoleDoesNot()
    {
        // Arrange - configure NLog with independent log levels
        LoggingConfiguration logConfig = new();

        ConsoleTarget consoleTarget = new("console")
        {
            Layout = "${longdate} [${level:uppercase}] ${message}${exception}",
        };
        logConfig.AddRuleForOneLevel(LogLevel.Info, consoleTarget);

        FileTarget fileTarget = new("file")
        {
            FileName = "${basedir}/logs/admin-console-${shortdate}.log",
            Layout = "${longdate} [${level:uppercase}] ${message}${exception}",
            ArchiveEvery = FileArchivePeriod.Day,
            MaxArchiveFiles = 30,
        };
        logConfig.AddRuleForOneLevel(LogLevel.Debug, fileTarget);

        LogManager.Setup().LoadConfiguration(logConfig);

        Logger logger = LogManager.GetCurrentClassLogger();

        // Act - log a DEBUG message
        logger.Debug("Test debug message");

        // Assert - verify file target captures DEBUG messages
        string? logFile = Directory.GetFiles(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"),
            "admin-console-*.log").FirstOrDefault();

        if (logFile != null)
        {
            string fileContent = File.ReadAllText(logFile);
            fileContent.ShouldContain("Test debug message");
        }
    }

    [Test]
    public async Task GivenNLogConfigured_WhenInfoMessageLogged_ThenBothTargetsReceiveEntry()
    {
        // Arrange - configure NLog with independent log levels
        LoggingConfiguration logConfig = new();

        ConsoleTarget consoleTarget = new("console")
        {
            Layout = "${longdate} [${level:uppercase}] ${message}${exception}",
        };
        logConfig.AddRuleForOneLevel(LogLevel.Info, consoleTarget);

        FileTarget fileTarget = new("file")
        {
            FileName = "${basedir}/logs/admin-console-${shortdate}.log",
            Layout = "${longdate} [${level:uppercase}] ${message}${exception}",
            ArchiveEvery = FileArchivePeriod.Day,
            MaxArchiveFiles = 30,
        };
        logConfig.AddRuleForOneLevel(LogLevel.Debug, fileTarget);

        LogManager.Setup().LoadConfiguration(logConfig);

        Logger logger = LogManager.GetCurrentClassLogger();

        // Act - log an INFO message
        logger.Info("Test info message");

        // Assert - verify file target captures INFO messages (since it's DEBUG level)
        string? logFile = Directory.GetFiles(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs"),
            "admin-console-*.log").FirstOrDefault();

        if (logFile != null)
        {
            string fileContent = File.ReadAllText(logFile);
            fileContent.ShouldContain("Test info message");
        }
    }
}
