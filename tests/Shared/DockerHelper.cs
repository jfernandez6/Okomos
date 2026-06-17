using Docker.DotNet;

namespace Okomos.Tests.Shared;

public static class DockerHelper
{
    private static readonly Lazy<bool> IsAvailableLazy = new(CheckDocker);

    public static bool IsAvailable => IsAvailableLazy.Value;

    private static bool CheckDocker()
    {
        try
        {
            using var client = new DockerClientConfiguration().CreateClient();
            client.System.PingAsync().GetAwaiter().GetResult();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerHelper.IsAvailable)
        {
            Skip = "Docker is not available. Start Docker Desktop to run integration tests.";
        }
    }
}
