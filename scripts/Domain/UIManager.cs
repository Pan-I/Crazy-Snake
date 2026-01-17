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

using System;
using Godot;

namespace Snake.scripts;

public partial class UIManager : GodotObject
{
    private CanvasLayer _hud;
    private CanvasLayer _gameOverMenu;
    private AnimatedSprite2D _background;

    public void Initialize(CanvasLayer hud, CanvasLayer gameOverMenu, AnimatedSprite2D background)
    {
        _hud = hud;
        _gameOverMenu = gameOverMenu;
        _background = background;
    }

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

    public void ShowGameOver(double score)
    {
        _gameOverMenu.Visible = true;
        string message = $"\n\nssSss!\n\nGame Over!\n\nScore: {score}"; //$"\n\nssSss!\n\nGame Over!\n\nScore: {score}"
        _gameOverMenu.GetNode<Panel>("GameOverPanel").GetNode<Label>("GameOverLabel").Text = message;
    }

    public void HideGameOver()
    {
        _gameOverMenu.Visible = false;
    }

    public void SetBackgroundVisible(bool visible) => _background.Visible = visible;
    public void SetBackgroundFrame(int frame) => _background.Frame = frame;

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

    public void ResetHudPanels()
    {
        StyleBoxFlat styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color("05395241");
        styleBox.BorderColor = new Color("1e3553");
        styleBox.SetBorderWidthAll(10);
        ApplyStyleToPanels(styleBox);
    }

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
