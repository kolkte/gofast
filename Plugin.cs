using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

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

    // Kept for the old SingletonThreadHelper. The new hook doesn't use it.
    public static volatile bool IsPlayerMoving = false;

    [DllImport("SpeedHook.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool InitializeHook(IntPtr baseAddress, IntPtr offset);

    [DllImport("SpeedHook.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void SetMultiplier(float multiplier);

    [DllImport("SpeedHook.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ShutdownHook();

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Format: /movespeed <multiplier>. Example: /movespeed 1.5"
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
            SetMultiplier(Configuration.MovementSpeedMultiplier);
            Log.Information($"SpeedHook initialized. Base=0x{baseAddress:X} Offset=0x{SpeedHookOffset:X}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to init SpeedHook: {ex.Message}");
        }
    }

    private void OnCommand(string command, string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            ChatGui.PrintError("Format: /movespeed <multiplier>");
            return;
        }

        if (float.TryParse(args.Trim(), out var multiplier))
        {
            if (multiplier < 0.1f || multiplier > 10f)
            {
                ChatGui.PrintError("Multiplier must be between 0.1 and 10.0");
                return;
            }

            Configuration.MovementSpeedMultiplier = multiplier;
            Configuration.EnableMovementSpeedHack = true;
            Configuration.Save();

            ApplyMultiplier(multiplier);

            ChatGui.Print($"Movement speed multiplier set to {multiplier}");
        }
        else
        {
            ChatGui.PrintError("Invalid multiplier.");
        }
    }

    /// <summary>
    /// Pushes the multiplier into the native SpeedHook. Called by both the chat
    /// command and the UI radio buttons.
    /// </summary>
    internal void ApplyMultiplier(float multiplier)
    {
        if (_hookInitialized)
            SetMultiplier(multiplier);
    }

    public void Dispose()
    {
        if (_hookInitialized)
        {
            try { ShutdownHook(); } catch { }
        }
        CommandManager.RemoveHandler(CommandName);
    }
}