using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;
using XIVSpeedTrainer.Enums;
using XIVSpeedTrainer.Helpers;

namespace XIVSpeedTrainer.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;
    private IntPtr _hProcess;

    public MainWindow(Plugin plugin) : base("XIVSpeedTrainer")
    {
        _plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(350, 100),
            MaximumSize = new Vector2(350, 100)
        };
    }

    public override void Draw()
    {
        if (_hProcess == IntPtr.Zero)
        {
            var hwnd = Win32.GetGameHwnd();
            _hProcess = Win32.GetProcessHandle(hwnd);
            if (_hProcess != IntPtr.Zero)
                SingletonThreadHelper.InitThread(_hProcess);
        }

        ImGui.TextColored(ImGuiColors.HealerGreen, "Tool for testing purposes, free to use.");
        ImGui.SameLine();
        ImGui.Text($"Current speed rate: {Constants.GetSelectedOption(_plugin.Configuration.SelectedSpeedOption):F2}");
        ImGui.Separator();

        for (var i = 0; i < Constants.OptionLabels.Length; i++)
        {
            var isSelected = _plugin.Configuration.SelectedSpeedOption == i;
            if (ImGui.RadioButton(Constants.OptionLabels[i], isSelected))
            {
                _plugin.Configuration.SelectedSpeedOption = i;
                _plugin.Configuration.Save();
                SingletonThreadHelper.SetRate(Constants.GetSelectedOption(i));
            }
            ImGui.SameLine();
        }
    }

    public void Dispose()
    {
        SingletonThreadHelper.Dispose();
    }
}