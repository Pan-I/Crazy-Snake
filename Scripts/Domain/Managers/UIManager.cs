/*
 Crazy Snake a remake of the traditional game "Snake" with many twists.
Copyright (C) 2025  Ian Pommer

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

The author can be contacted at pan.i.githubcontact@gmail.com
*/

using Godot;

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Manages the user interface (UI) elements of the game, including the HUD, game over menu, and background animations.
/// Provides functionality to update, display, and reset various UI components in response to game events.
/// </summary>
public partial class UiManager : GodotObject
{
    private CanvasLayer _hud;
    private CanvasLayer _gameOverMenu;
    private AnimatedSprite2D _background;

    /// <summary>
    /// Initializes the UIManager with the specified HUD, game over menu, and background components.
    /// </summary>
    /// <param name="hud">The CanvasLayer representing the HUD.</param>
    /// <param name="gameOverMenu">The CanvasLayer representing the game over menu.</param>
    /// <param name="background">The AnimatedSprite2D representing the background.</param>
    public void Initialize(CanvasLayer hud, CanvasLayer gameOverMenu, AnimatedSprite2D background)
    {
        _hud = hud;
        _gameOverMenu = gameOverMenu;
        _background = background;
    }

    /// <summary>
    /// Updates the score display on the HUD, including the current score, combo multipliers,
    /// and the visibility of combo-related elements.
    /// </summary>
    /// <param name="score">The current score to be displayed on the HUD.</param>
    /// <param name="comboX">The first combo multiplier value contributing to the score.</param>
    /// <param name="comboY">The second combo multiplier value contributing to the score.</param>
    /// <param name="isInCombo">Indicates whether the player is currently in a combo sequence.</param>
    public void UpdateScore(double score, double comboX, double comboY, bool isInCombo)
    {
        _hud.GetNode<Panel>("ScorePanel").GetNode<Label>("ScoreLabel").Text = $"Score: {score} ";
        _hud.GetNode<Panel>("ComboPanel").GetNode<Label>("ComboLabel").Text = $"CRRAAAZZY Combo: {comboX} * {comboY}";
        _hud.GetNode<Panel>("ComboPanel").GetNode<Label>("ComboLabel").Visible = isInCombo;

        if (!isInCombo)
        {
            _hud.GetNode<Panel>("ComboPanel").GetNode<Control>("ComboMeter").Visible = false;
        }
    }

    /// <summary>
    /// Updates the combo meter display based on the current combo tally and combo state.
    /// </summary>
    /// <param name="comboTally">The number of items collected in the current combo sequence.</param>
    /// <param name="isInCombo">Indicates whether the player is actively in a combo state.</param>
    public void UpdateComboMeter(int comboTally, bool isInCombo)
    {
        var comboMeter = _hud.GetNode<Panel>("ComboPanel").GetNode<Control>("ComboMeter");
        if (comboTally >= 2)
        {
            comboMeter.Visible = true;
            comboMeter.Modulate = isInCombo ? new Color("ffffff") : new Color("ffffff7f");
            comboMeter.GetNode<TextureProgressBar>("TextureProgressBar").Value = isInCombo ? 100 : comboTally / 7.0 * 100; // Adjusted logic
        }
    }

    /// <summary>
    /// Displays the game over menu and updates its text with the final score.
    /// </summary>
    /// <param name="score">The final score to be displayed in the game over menu.</param>
    public void ShowGameOver(double score)
    {
        _gameOverMenu.Visible = true;
        string message = $"\n\nssSss!\n\nGame Over!\n\nScore: {score}"; //$"\n\nssSss!\n\nGame Over!\n\nScore: {score}"
        _gameOverMenu.GetNode<Panel>("GameOverPanel").GetNode<Label>("GameOverLabel").Text = message;
    }

    /// <summary>
    /// Hides the game over menu, making it no longer visible in the UI.
    /// </summary>
    public void HideGameOver()
    {
        _gameOverMenu.Visible = false;
    }

    /// <summary>
    /// Sets the visibility of the background element in the UI.
    /// </summary>
    /// <param name="visible">A boolean indicating whether the background should be visible (true) or hidden (false).</param>
    public void SetBackgroundVisible(bool visible) => _background.Visible = visible;

    /// <summary>
    /// Sets the frame of the animated background sprite to the specified frame index.
    /// </summary>
    /// <param name="frame">The index of the frame to display for the animated background.</param>
    public void SetBackgroundFrame(int frame) => _background.Frame = frame;

    /// <summary>
    /// Applies a visual flash effect to the HUD panels based on the specified event type.
    /// </summary>
    /// <param name="type">An integer representing the type of event causing the flash:
    /// 0 for a regular egg consumption, 1 for health deduction, or 2 for other item consumption.</param>
    public void HudFlash(int type)
    {
        StyleBoxFlat styleBox = new StyleBoxFlat();
        switch (type)
        {
            case 0: // Regular Egg-Eat
                styleBox.BgColor = new Color("#05ff4d70");
                styleBox.BorderColor = new Color("#1e3553");
                break;
            case 1: // Deduct Health
                styleBox.BgColor = new Color("#cd39528c");
                styleBox.BorderColor = new Color("#591d47");
                break;
            case 2: // Other Item Eat
                styleBox.BgColor = new Color("958e4685");
                styleBox.BorderColor = new Color("39411b");
                break;
        }
        styleBox.SetBorderWidthAll(10);
        ApplyStyleToPanels(styleBox);
    }

    /// <summary>
    /// Resets the HUD panels by applying a default style to refresh their appearance.
    /// </summary>
    public void ResetHudPanels()
    {
        StyleBoxFlat styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color("05395241");
        styleBox.BorderColor = new Color("1e3553");
        styleBox.SetBorderWidthAll(10);
        ApplyStyleToPanels(styleBox);
    }

    /// <summary>
    /// Applies the specified StyleBox to the panels within the HUD.
    /// </summary>
    /// <param name="styleBox">The StyleBox containing the style properties to apply to the panels.</param>
    private void ApplyStyleToPanels(StyleBox styleBox)
    {
        foreach (string panelName in new[] { "RightPanel", "BottomPanel" })
        {
            Panel parentPanel = _hud.GetNode<Panel>(panelName);
            foreach (Node child in parentPanel.GetChildren())
            {
                if (child is Panel childPanel)
                {
                    childPanel.AddThemeStyleboxOverride("panel", styleBox);
                }
            }
        }
    }

    /// <summary>
    /// Updates the appearance of the window dressing panel based on the game's health and game-over state.
    /// </summary>
    /// <param name="lowHealth">Indicates whether the player's health is low.</param>
    /// <param name="isGameOver">Indicates whether the game is in a game-over state. Defaults to false.</param>
    public void UpdateWindowDressing(bool lowHealth, bool isGameOver = false)
    {
        var panel = _hud.GetNode<Panel>("WindowDressingPanel");
        panel.RemoveThemeStyleboxOverride("panel");
        
        StyleBoxFlat styleBox = new StyleBoxFlat();
        if (isGameOver || lowHealth)
        {
            styleBox.BgColor = isGameOver ? new Color("d4414a9e") : new Color("f60c1315");
            styleBox.BorderColor = isGameOver ? new Color("7d2549") : new Color("b32e42");
        }
        else
        {
            styleBox.BgColor = new Color("64c2f809");
            styleBox.BorderColor = new Color("3d3c95");
        }
        styleBox.SetBorderWidthAll(10);
        panel.AddThemeStyleboxOverride("panel", styleBox);
    }
}
