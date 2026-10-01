using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
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

    public Configuration Configuration { get; init; }
    private readonly WindowSystem _windowSystem = new("XIVSpeedTrainer");
    private readonly MainWindow _mainWindow;

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

        // Apply the last saved speed immediately.
        SingletonThreadHelper.SetRate(Enums.Constants.GetSelectedOption(Configuration.SelectedSpeedOption));

        Log.Information("XIVSpeedTrainer loaded.");
    }

    private void OnCommand(string command, string args)
    {
        _mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _windowSystem.RemoveAllWindows();
        _mainWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }
}