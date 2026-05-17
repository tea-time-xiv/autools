using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Autools.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MainWindow(Plugin plugin)
        : base("Autools###AutoolsMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 150),
            MaximumSize = new Vector2(500, 300)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Autools - FFXIV Automation Utilities");
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted("Features:");
        ImGui.BulletText("Auto Priority Aetheryte Pass");
        ImGui.BulletText("No Jog (auto-cancel Jog buff)");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var autoPriorityPass = plugin.Configuration.AutoPriorityPassEnabled;
        if (ImGui.Checkbox("Auto-use Priority Aetheryte Pass", ref autoPriorityPass))
        {
            plugin.Configuration.AutoPriorityPassEnabled = autoPriorityPass;
            plugin.Configuration.Save();
        }

        var noJog = plugin.Configuration.NoJogEnabled;
        if (ImGui.Checkbox("Cancel Jog buff automatically", ref noJog))
        {
            plugin.Configuration.NoJogEnabled = noJog;
            plugin.Configuration.Save();
        }

        ImGui.Spacing();

        if (ImGui.Button("Open Settings"))
        {
            plugin.ToggleConfigUi();
        }

        ImGui.SameLine();

        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "  |  Commands: /passauto /nojog");
    }
}
