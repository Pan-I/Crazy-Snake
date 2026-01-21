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

using System.Diagnostics;
using Godot;

namespace Snake.Scripts.Domain.Managers;

public partial class ScoreManager : GodotObject
{
    [Signal] public delegate void ScoreChangedEventHandler(double score, double comboX, double comboY, bool isInCombo);
    [Signal] public delegate void ComboStartedEventHandler();
    [Signal] public delegate void ComboEndedEventHandler();
    [Signal] public delegate void ComboCancelledEventHandler();

    public double Score { get; private set; }
    public double ComboPointsX { get; set; }
    public double ComboPointsY { get; set; }
    public int ComboTally { get; set; }
    public bool IsInCombo { get; private set; }

    public void Reset()
    {
        Score = 0;
        ComboPointsX = 0;
        ComboPointsY = 0;
        ComboTally = 0;
        IsInCombo = false;
        EmitScoreChanged();
    }

    public void AddScore(double amount)
    {
        Score += amount;
        Score = Math.Round(Score, 0);
        EmitScoreChanged();
    }

    public void SetScore(double amount)
    {
        Score = amount;
        Score = Math.Round(Score, 0);
        EmitScoreChanged();
    }
    
    public void IncrementComboTally()
    {
        ComboTally++;
    }

    public void StartCombo()
    {
        if (ComboTally < 7)
        {
            ComboTally = 7;
        }
        IsInCombo = true;
        ComboPointsX = Math.Max(1, ComboTally);
        ComboPointsY = 1;
        Debug.Print("Combo Started!");
        EmitComboStarted();
        EmitScoreChanged();
    }

    public void EndCombo()
    {
        if (IsInCombo)
        {
            double comboPoints = (ComboPointsX * ComboPointsY);
            Debug.Print("Combo Ended with Score: " + ComboPointsX + " x " + ComboPointsY + " = " + comboPoints);
            Score += comboPoints > 0 ? comboPoints : Math.Min(ComboPointsX, ComboPointsY);
            Score = Math.Round(Score, 0);
        }
        
        IsInCombo = false;
        ComboTally = 0;
        ComboPointsX = 0;
        ComboPointsY = 0;
        EmitComboEnded();
        EmitScoreChanged();
    }

    public void CancelCombo()
    {
        Debug.Print("Combo Cancelled!");
        IsInCombo = false;
        ComboPointsX = 0;
        ComboPointsY = 0;
        EmitComboCancelled();
        EmitScoreChanged();
    }
    
    public void UpdateComboPointsX(double amount)
    {
        ComboPointsX = amount;
        EmitScoreChanged();
    }
    
    public void UpdateComboPointsY(double amount)
    {
        ComboPointsY = amount;
        EmitScoreChanged();
    }

    private void EmitScoreChanged()
    {
        EmitSignal(SignalName.ScoreChanged, Score, ComboPointsX, ComboPointsY, IsInCombo);
    }

    private void EmitComboStarted()
    {
        EmitSignal(SignalName.ComboStarted, null);
    }
    
    private void EmitComboEnded()
    {
        EmitSignal(SignalName.ComboEnded, null);
    }
    
    private void EmitComboCancelled()
    {
        EmitSignal(SignalName.ComboCancelled, null);
    }
}
