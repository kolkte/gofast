using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Numerics;
using XIVSpeedTrainer.Helpers;
using XIVSpeedTrainer.Windows;

namespace XIVSpeedTrainer;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "XIVSpeedTrainer";

    private const string CommandName = "/xst";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    public Configuration Configuration { get; init; }
    private readonly WindowSystem _windowSystem = new("XIVSpeedTrainer");
    private readonly MainWindow _mainWindow;

    // Track whether the player is currently moving. Set by the Framework update.
    public static volatile bool IsPlayerMoving = false;
    private Vector3 _lastPosition = Vector3.Zero;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _mainWindow = new MainWindow(this);
        _windowSystem.AddWindow(_mainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the XIVSpeedTrainer window."
        });

        PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += () => _mainWindow.IsOpen = true;

        // Hook the framework update so we can track player movement each frame.
        Framework.Update += OnFrameworkUpdate;

        // Apply the last saved speed setting.
        SingletonThreadHelper.SetRate(Enums.Constants.GetSelectedOption(Configuration.SelectedSpeedOption));

        Log.Information("XIVSpeedTrainer loaded.");
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        var player = ObjectTable.LocalPlayer;
        if (player == null)
        {
            IsPlayerMoving = false;
            return;
        }

        var current = player.Position;
        // Compare squared distance to avoid an expensive sqrt.
        // Threshold chosen so that minor positional jitter (e.g., from animations) doesn't count.
        IsPlayerMoving = Vector3.DistanceSquared(current, _lastPosition) > 0.0001f;
        _lastPosition = current;
    }

    private void OnCommand(string command, string args)
    {
        _mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _windowSystem.RemoveAllWindows();
        _mainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }
}