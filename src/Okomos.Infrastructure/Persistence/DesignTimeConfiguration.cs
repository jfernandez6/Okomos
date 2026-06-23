using Microsoft.Extensions.Configuration;

namespace Okomos.Infrastructure.Persistence;

public static class DesignTimeConfiguration
{
    public static string GetConnectionString(string name)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiPath = FindApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException(
                $"Connection string '{name}' was not found. Configure it in {apiPath}/appsettings.json or appsettings.{environment}.json.");
    }

    private static string FindApiProjectPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var directApiPath = Path.Combine(directory.FullName, "Okomos.Api");
            if (File.Exists(Path.Combine(directApiPath, "appsettings.json")))
                return directApiPath;

            var srcApiPath = Path.Combine(directory.FullName, "src", "Okomos.Api");
            if (File.Exists(Path.Combine(srcApiPath, "appsettings.json")))
                return srcApiPath;

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate Okomos.Api appsettings.json. Run EF commands from the repository root or pass --connection explicitly.");
    }
}
