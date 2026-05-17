using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Autools.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("Autools Configuration###AutoolsConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(360, 260);
        SizeCondition = ImGuiCond.Always;

        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.TextUnformatted("Auto Priority Pass");
        ImGui.Separator();
        ImGui.Spacing();

        var autoPriorityPass = configuration.AutoPriorityPassEnabled;
        if (ImGui.Checkbox("Auto-use Priority Aetheryte Pass", ref autoPriorityPass))
        {
            configuration.AutoPriorityPassEnabled = autoPriorityPass;
            configuration.Save();
        }
        
        ImGui.TextWrapped("Automatically uses Priority Aetheryte Pass when in overworld without teleport reduction buffs.");

        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Tip: You can also use /passauto to toggle");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted("No Jog");
        ImGui.Separator();
        ImGui.Spacing();

        var noJog = configuration.NoJogEnabled;
        if (ImGui.Checkbox("Auto-cancel Jog buff", ref noJog))
        {
            configuration.NoJogEnabled = noJog;
            configuration.Save();
        }

        ImGui.TextWrapped("Cancels the Jog buff that auto-applies after Sprint expires. Skipped in duties.");

        ImGui.Spacing();
        ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Tip: also toggleable via /nojog");
    }
}
