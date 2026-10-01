using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace XIVSpeedTrainer.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;

    public MainWindow(Plugin plugin) : base("XIVSpeedTrainer")
    {
        _plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 100),
            MaximumSize = new Vector2(400, 200)
        };
    }

    public override void Draw()
    {
        // Checkbox to enable/disable the speed hack.
        var enabled = _plugin.Configuration.EnableMovementSpeedHack;
        if (ImGui.Checkbox("Enable Speed Hack", ref enabled))
        {
            _plugin.Configuration.EnableMovementSpeedHack = enabled;

            // If enabled, apply the saved multiplier. If disabled, apply 1.0f
            // to the hook to disable the effect, but DO NOT change the saved multiplier.
            _plugin.ApplyMultiplier(enabled ? _plugin.Configuration.MovementSpeedMultiplier : 1.0f);

            _plugin.Configuration.Save();
        }

        ImGui.Separator();

        // Slider for the speed multiplier. This is always interactive.
        var multiplier = _plugin.Configuration.MovementSpeedMultiplier;
        if (ImGui.SliderFloat("Speed Multiplier", ref multiplier, 0.5f, 10.0f, "%.2f"))
        {
            _plugin.Configuration.MovementSpeedMultiplier = multiplier;

            // Only apply the change to the hook if the hack is currently enabled.
            // If it's disabled, we just save the value for later.
            if (_plugin.Configuration.EnableMovementSpeedHack)
            {
                _plugin.ApplyMultiplier(multiplier);
            }

            _plugin.Configuration.Save();
        }

        // Display the current effective speed.
        // If disabled, it should show 1.00x, otherwise the saved multiplier.
        var effectiveSpeed = _plugin.Configuration.EnableMovementSpeedHack
            ? _plugin.Configuration.MovementSpeedMultiplier
            : 1.0f;

        ImGui.Text($"Current effective speed: {effectiveSpeed:F2}x");
    }

    public void Dispose() { }
}