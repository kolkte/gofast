using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace XIVSpeedTrainer.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;

    private static readonly float[] Rates = { 1.0f, 1.05f, 1.15f, 3.0f, 9.99f };
    private static readonly string[] OptionLabels = { "1.00", "1.05", "1.15", "3.00", "9.99" };

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
        ImGui.TextColored(ImGuiColors.HealerGreen, "Tool for testing purposes.");
        ImGui.SameLine();
        ImGui.Text($"Current speed rate: {Rates[_plugin.Configuration.SelectedSpeedOption]:F2}");
        ImGui.Separator();

        for (var i = 0; i < OptionLabels.Length; i++)
        {
            var isSelected = _plugin.Configuration.SelectedSpeedOption == i;
            if (ImGui.RadioButton(OptionLabels[i], isSelected))
            {
                _plugin.Configuration.SelectedSpeedOption = i;
                _plugin.Configuration.MovementSpeedMultiplier = Rates[i];
                _plugin.Configuration.EnableMovementSpeedHack = true;
                _plugin.Configuration.Save();
                _plugin.ApplyMultiplier(Rates[i]);
            }
            ImGui.SameLine();
        }
    }

    public void Dispose() { }
}