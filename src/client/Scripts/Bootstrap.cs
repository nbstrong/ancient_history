using System.Reflection;
using Godot;

public partial class Bootstrap : Control
{
    private const string StatusLabelPath = "CenterContainer/VBoxContainer/StatusLabel";
    private const string BuildLabelPath = "CenterContainer/VBoxContainer/BuildLabel";

    public override void _EnterTree()
    {
        GD.Print("{\"component\":\"client\",\"event\":\"startup\"}");
    }

    public override void _Ready()
    {
        GetNode<Label>(StatusLabelPath).Text = "Offline";

        string? informationalVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        GetNode<Label>(BuildLabelPath).Text = string.IsNullOrWhiteSpace(informationalVersion)
            ? "Build: unavailable"
            : $"Build: {informationalVersion}";
    }

    public override void _ExitTree()
    {
        GD.Print("{\"component\":\"client\",\"event\":\"shutdown\"}");
    }
}
