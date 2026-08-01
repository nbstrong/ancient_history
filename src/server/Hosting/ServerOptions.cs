namespace WurmStyleGame.Server.Hosting;

public sealed class ServerOptions
{
    public const string SectionName = "Server";

    private static readonly string ProcessInstanceId = Guid.NewGuid().ToString("N");

    public string InstanceId { get; set; } = ProcessInstanceId;

    public string EnvironmentName { get; set; } = string.Empty;

    public int ShutdownDrainTimeoutSeconds { get; set; } = 10;
}
