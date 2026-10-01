using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using XIVSpeedTrainer.Windows;

namespace XIVSpeedTrainer;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    public string Name => "XIVSpeedTrainer";
    private const string CommandName = "/movespeed";
    private const long SpeedHookOffset = 0x182BC5F;

    public Configuration Configuration { get; init; }
    private bool _hookInitialized;

    // Add these for the UI
    public readonly WindowSystem WindowSystem = new("XIVSpeedTrainer");
    private MainWindow? _mainWindow;

    [DllImport("SpeedHook.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool InitializeHook(IntPtr baseAddress, IntPtr offset);

    [DllImport("SpeedHook.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetMultiplier(float multiplier);

    [DllImport("SpeedHook.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ShutdownHook();

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Initialize the UI
        _mainWindow = new MainWindow(this);
        WindowSystem.AddWindow(_mainWindow);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Toggles the XIVSpeedTrainer window."
        });

        try
        {
            var dllPath = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName!, "SpeedHook.dll");
            if (!File.Exists(dllPath))
            {
                Log.Error($"SpeedHook.dll not found at {dllPath}");
                return;
            }

            var baseAddress = (nint)Process.GetCurrentProcess().MainModule!.BaseAddress;
            bool ok = InitializeHook(baseAddress, (IntPtr)SpeedHookOffset);

            if (!ok)
            {
                Log.Error("InitializeHook returned false");
                return;
            }

            _hookInitialized = true;
            // Apply the saved configuration on startup.
            ApplyMultiplier(Configuration.MovementSpeedMultiplier);
            Log.Information($"SpeedHook initialized. Base=0x{baseAddress:X} Offset=0x{SpeedHookOffset:X}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to init SpeedHook: {ex.Message}");
        }
    }

    private void OnCommand(string command, string args)
    {
        // The command now simply toggles the window.
        ToggleConfigUi();
    }

    private void ToggleConfigUi()
    {
        _mainWindow?.Toggle();
    }

    /// <summary>
    /// Pushes the multiplier into the native SpeedHook. Called by the UI.
    /// </summary>
    internal void ApplyMultiplier(float multiplier)
    {
        if (_hookInitialized)
            SetMultiplier(multiplier);
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();
        _mainWindow?.Dispose();

        if (_hookInitialized)
        {
            try { ShutdownHook(); } catch { }
        }
        CommandManager.RemoveHandler(CommandName);
    }
}