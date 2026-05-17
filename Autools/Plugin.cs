using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Autools.Windows;
using System;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Autools;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static ICondition Condition { get; private set; } = null!;

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;

    private const string PassAutoCommandName = "/passauto";
    private const string NoJogCommandName = "/nojog";

    // Auto Priority Pass constants
    private const uint PriorityAetherytePassItemId = 14954;
    private const uint StatusPriorityAetherytePass = 1061;
    private const uint StatusFcReducedRates = 1235;

    // No Jog constants
    private const uint JogStatusId = 4209;
    private const byte JogParam = 20;
    
    private DateTime lastItemUseAttempt = DateTime.MinValue;
    private const int ItemUseCooldownSeconds = 30;

    private DateTime lastInventoryCheck = DateTime.MinValue;
    private bool lastInventoryHasPass;
    private const double InventoryPollSeconds = 1.0;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Autools");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(PassAutoCommandName, new CommandInfo(OnPassAutoCommand)
        {
            HelpMessage = "Toggle auto-use of Priority Aetheryte Pass"
        });

        CommandManager.AddHandler(NoJogCommandName, new CommandInfo(OnNoJogCommand)
        {
            HelpMessage = "Toggle auto-cancel of Jog buff (after Sprint expires)"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        Framework.Update += OnFrameworkUpdate;

        Log.Information("Autools initialized - Auto Priority Pass available (use /passauto to toggle)");
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        try
        {
            if (!ClientState.IsLoggedIn) return;

            if (Configuration.AutoPriorityPassEnabled)
            {
                var player = ObjectTable.LocalPlayer;
                if (player != null && IsInOverworld() && !HasTeleportReduction(player) && HasPassInInventory())
                {
                    UseItem(PriorityAetherytePassItemId);
                }
            }

            if (Configuration.NoJogEnabled)
            {
                CancelJogIfPresent();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnFrameworkUpdate");
        }
    }

    private void CancelJogIfPresent()
    {
        var player = ObjectTable.LocalPlayer;
        if (player == null) return;

        foreach (var status in player.StatusList)
        {
            if (status.StatusId != JogStatusId || status.Param != JogParam) continue;

            bool inDuty = Condition[ConditionFlag.BoundByDuty]
                       || Condition[ConditionFlag.BoundByDuty56]
                       || Condition[ConditionFlag.BoundByDuty95];
            if (inDuty) return;

            FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(JogStatusId, (uint)status.SourceId);
            Log.Information("Cancelled Jog buff");
            return;
        }
    }

    private bool IsInOverworld()
    {
        if (ClientState.IsPvP || ClientState.IsPvPExcludingDen) return false;
        if (Condition[ConditionFlag.BoundByDuty]
         || Condition[ConditionFlag.BoundByDuty56]
         || Condition[ConditionFlag.BoundByDuty95]) return false;
        return true;
    }

    private bool HasTeleportReduction(Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter player)
    {
        foreach (var status in player.StatusList)
        {
            if (status.StatusId == StatusPriorityAetherytePass ||
                status.StatusId == StatusFcReducedRates)
                return true;
        }
        return false;
    }

    private static readonly InventoryType[] PlayerInventories =
    {
        InventoryType.Inventory1, InventoryType.Inventory2,
        InventoryType.Inventory3, InventoryType.Inventory4,
    };

    private unsafe bool HasPassInInventory()
    {
        if ((DateTime.Now - lastInventoryCheck).TotalSeconds < InventoryPollSeconds)
            return lastInventoryHasPass;
        lastInventoryCheck = DateTime.Now;

        var invManager = InventoryManager.Instance();
        if (invManager == null) { lastInventoryHasPass = false; return false; }

        foreach (var container in PlayerInventories)
        {
            var inv = invManager->GetInventoryContainer(container);
            if (inv == null) continue;

            for (int i = 0; i < inv->Size; i++)
            {
                var slot = inv->GetInventorySlot(i);
                if (slot == null) continue;
                if (slot->ItemId == PriorityAetherytePassItemId && slot->Quantity > 0)
                {
                    lastInventoryHasPass = true;
                    return true;
                }
            }
        }
        lastInventoryHasPass = false;
        return false;
    }

    private unsafe void UseItem(uint itemId)
    {
        var actionManager = ActionManager.Instance();
        if (actionManager == null) return;

        // Check cooldown
        if ((DateTime.Now - lastItemUseAttempt).TotalSeconds < ItemUseCooldownSeconds)
            return;

        // Set the timestamp BEFORE attempting to use the item to prevent rapid-fire calls
        lastItemUseAttempt = DateTime.Now;

        // Try to use the item via ActionManager
        // ActionType.Item with extraParam 65535 is required for using items from inventory
        if (actionManager->UseAction(ActionType.Item, itemId, extraParam: 65535))
        {
            Log.Information($"Using Priority Aetheryte Pass (Item ID: {itemId})");
            ChatGui.Print("Priority Aetheryte Pass used");
        }
        else
        {
            Log.Warning($"Failed to use Priority Aetheryte Pass (Item ID: {itemId})");
            // Reset timestamp if it failed so we can try again sooner
            lastItemUseAttempt = DateTime.Now.AddSeconds(-ItemUseCooldownSeconds + 5);
        }
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(PassAutoCommandName);
        CommandManager.RemoveHandler(NoJogCommandName);
    }

    private void OnPassAutoCommand(string command, string args)
    {
        Configuration.AutoPriorityPassEnabled = !Configuration.AutoPriorityPassEnabled;
        Configuration.Save();
        var status = Configuration.AutoPriorityPassEnabled ? "ON" : "OFF";
        Log.Information($"Auto Priority Pass toggled {status}");
        ChatGui.Print($"Auto Priority Pass: {status}");
    }

    private void OnNoJogCommand(string command, string args)
    {
        Configuration.NoJogEnabled = !Configuration.NoJogEnabled;
        Configuration.Save();
        var status = Configuration.NoJogEnabled ? "ON" : "OFF";
        Log.Information($"No Jog toggled {status}");
        ChatGui.Print($"No Jog: {status}");
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();
}
