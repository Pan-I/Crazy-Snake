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
using Snake.Scripts.Interfaces;

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Manages the score and combo system for the game.
/// This includes tracking the player's score, managing combo points,
/// and handling the state of the combo system.
/// </summary>
public partial class ScoreManager : GodotObject, IScoreManager
{
    #region Signals
    /// <summary>
    /// Represents the delegate for handling the event triggered when the score is updated.
    /// </summary>
    /// <param name="score">The current score value after the update.</param>
    /// <param name="comboX">The current X-axis combo multiplier.</param>
    /// <param name="comboY">The current Y-axis combo multiplier.</param>
    /// <param name="isInCombo">Indicates whether the player is actively in a combo state.</param>
    [Signal]
    public delegate void ScoreChangedEventHandler(double score, double comboX, double comboY, bool isInCombo);

    /// <summary>
    /// Represents the delegate for handling the event triggered when a combo sequence begins.
    /// </summary>
    [Signal]
    public delegate void ComboStartedEventHandler();

    /// <summary>
    /// Represents the delegate for handling the event triggered when a combo sequence has ended.
    /// </summary>
    [Signal]
    public delegate void ComboEndedEventHandler();

    /// <summary>
    /// Represents the delegate for handling the event triggered when a combo is canceled.
    /// </summary>
    [Signal]
    public delegate void ComboCanceledEventHandler();
    #endregion
    
    #region Properties
    /// <summary>
    /// Gets the player's current score in the game.
    /// </summary>
    /// <remarks>
    /// The <c>Score</c> property represents the total score accumulated by the player.
    /// It is updated whenever points are added through game actions, and ensures the score
    /// is rounded to the nearest whole number.
    /// </remarks>
    /// <value>
    /// A <c>double</c> representing the player's score. The value is read-only for external access.
    /// </value>
    public double Score { get; private set; }

    /// <summary>
    /// Gets or sets the multiplier for combo points accumulated during a combo sequence.
    /// </summary>
    /// <remarks>
    /// The <c>ComboPointsX</c> property represents the horizontal multiplier for the player's
    /// combo score calculation. It is updated dynamically based on the player's actions during
    /// a combo and plays a key role in determining the final combo score when the combo ends or is canceled.
    /// </remarks>
    /// <value>
    /// A <c>double</c> representing the horizontal multiplier for the combo score.
    /// The value is updated during gameplay and contributes directly to the final score during a combo.
    /// </value>
    public double ComboPointsX { get; set; }

    /// <summary>
    /// Gets or sets the multiplier value for calculating score during a combo event.
    /// </summary>
    /// <remarks>
    /// The <c>ComboPointsY</c> property represents a critical factor in the score calculation
    /// when a combo is active. It is typically initialized to 1 at the start of a combo
    /// and may be dynamically updated to reflect changes in the combo's progression.
    /// </remarks>
    /// <value>
    /// A <c>double</c> representing the multiplier for the Y-axis in combo score calculations.
    /// This value is writable and impacts the final score when the combo ends.
    /// </value>
    public double ComboPointsY { get; set; }

    /// <summary>
    /// Gets or sets the total number of combos executed by the player.
    /// </summary>
    /// <remarks>
    /// The <c>ComboTally</c> property tracks the cumulative count of combos performed during gameplay.
    /// It is incremented whenever a new combo begins and is reset when the combo sequence ends or is canceled.
    /// This property can also influence combo-related mechanics, such as multipliers or bonuses.
    /// </remarks>
    /// <value>
    /// An <c>int</c> representing the total number of combos. The value is mutable and can be updated during gameplay.
    /// </value>
    public int ComboTally { get; set; }

    /// <summary>
    /// Indicates whether the player is currently in a combo state.
    /// </summary>
    /// <remarks>
    /// The <c>IsInCombo</c> property determines if the player is actively participating
    /// in a scoring combo. Combos are initiated by specific game actions and affect
    /// the scoring mechanism.
    /// The property is updated by <c>StartCombo</c> and <c>EndCombo</c> methods in
    /// the <c>ScoreManager</c> class to reflect the current combo state.
    /// </remarks>
    /// <value>
    /// A <c>bool</c> that is <c>true</c> if the player is in a combo, and <c>false</c> otherwise.
    /// The value is read-only for external access and can only be modified internally.
    /// </value>
    public bool IsInCombo { get; private set; }
    #endregion
    
    #region Score Management
    /// <summary>
    /// Resets the score manager's state to its initial values.
    /// This includes setting the Score, ComboPointsX, ComboPointsY, and ComboTally
    /// to zero, marking IsInCombo as false, and emitting the score changed signal.
    /// </summary>
    public void Reset()
    {
        Score = 0;
        ComboPointsX = 0;
        ComboPointsY = 0;
        ComboTally = 0;
        IsInCombo = false;
        EmitScoreChanged();
    }

    /// <summary>
    /// Adds the specified amount to the current score while rounding the result to 0 decimal places.
    /// Triggers the score changed signal after updating the score.
    /// </summary>
    /// <param name="amount">The amount to be added to the current score.</param>
    public void AddScore(double amount)
    {
        Score += amount;
        Score = Math.Round(Score, 0, MidpointRounding.AwayFromZero);
        EmitScoreChanged();
    }

    /// <summary>
    /// Sets the score to the specified amount and rounds it to the nearest whole number.
    /// After setting the score, emits the score changed signal.
    /// </summary>
    /// <param name="amount">The new score value to be set.</param>
    public void SetScore(double amount)
    {
        Score = amount;
        Score = Math.Round(Score, 0, MidpointRounding.AwayFromZero);
        EmitScoreChanged();
    }
    #endregion

    #region Combo Management
    /// <summary>
    /// Increments the tally of consecutive actions performed within the current combo.
    /// This method increases the ComboTally value by one, typically reflecting an update
    /// in the player's progress during a combo sequence.
    /// </summary>
    public void IncrementComboTally()
    {
        ComboTally++;
    }

    /// <summary>
    /// Initiates a combo sequence by ensuring the combo tally is at least 7,
    /// marking the object as being in a combo state, and initializing the
    /// X and Y combo point multipliers. This method also triggers the combo
    /// started and score changed signals for relevant observers.
    /// </summary>
    public void StartCombo()
    {
        if (ComboTally < 7)
        {
            ComboTally = 7;
        }
        IsInCombo = true;
        ComboPointsX = Math.Max(1, ComboTally);
        ComboPointsY = 1;
        EmitComboStarted();
        EmitScoreChanged();
    }

    /// <summary>
    /// Ends the current combo session if a combo is active.
    /// This method calculates the score accumulated during the combo,
    /// adds it to the total score, and resets combo-related values, including
    /// ComboPointsX, ComboPointsY, and ComboTally. It also marks IsInCombo as false
    /// and emits the ComboEnded and ScoreChanged signals to notify listeners of the state changes.
    /// </summary>
    public void EndCombo()
    {
        if (IsInCombo)
        {
            double comboPoints = (ComboPointsX * ComboPointsY);
            Score += comboPoints > 0 ? comboPoints : Math.Min(ComboPointsX, ComboPointsY);
            Score = Math.Round(Score, 0, MidpointRounding.AwayFromZero);
        }
        
        IsInCombo = false;
        ComboTally = 0;
        ComboPointsX = 0;
        ComboPointsY = 0;
        EmitComboEnded();
        EmitScoreChanged();
    }

    /// <summary>
    /// Cancels the current combo in progress and resets all combo-related values.
    /// This includes setting `IsInCombo` to false, resetting `ComboPointsX` and `ComboPointsY` to zero,
    /// and emitting the ComboCanceled and ScoreChanged signals to notify listeners of the change.
    /// </summary>
    public void CancelCombo()
    {
        IsInCombo = false;
        ComboPointsX = 0;
        ComboPointsY = 0;
        EmitComboCancelled();
        EmitScoreChanged();
    }

    /// <summary>
    /// Updates the value of ComboPointsX to the specified amount
    /// and triggers the score changed signal.
    /// </summary>
    /// <param name="amount">The new value to set for ComboPointsX.</param>
    public void UpdateComboPointsX(double amount)
    {
        ComboPointsX = amount;
        EmitScoreChanged();
    }

    /// <summary>
    /// Updates the value of the ComboPointsY property with the specified amount
    /// and triggers a score changed event.
    /// </summary>
    /// <param name="amount">The value to assign to ComboPointsY.</param>
    public void UpdateComboPointsY(double amount)
    {
        ComboPointsY = amount;
        EmitScoreChanged();
    }
    #endregion
    
    #region Signal Emitting
    /// <summary>
    /// Emits the ScoreChanged signal with the current score, combo X points, combo Y points,
    /// and combo status. This is used to notify listeners of any updates to the score or
    /// combo-related values.
    /// </summary>
    private void EmitScoreChanged()
    {
        EmitSignal(SignalName.ScoreChanged, Score, ComboPointsX, ComboPointsY, IsInCombo);
    }

    /// <summary>
    /// Emits the ComboStarted signal to notify listeners when a combo has started.
    /// This signal is usually triggered as part of the StartCombo method
    /// when the combo state is initialized and the required conditions are satisfied.
    /// </summary>
    private void EmitComboStarted()
    {
        EmitSignal(SignalName.ComboStarted, null);
    }

    /// <summary>
    /// Emits the ComboEnded signal, notifying listeners that a combo has ended.
    /// This method is called internally when the combo sequence is successfully terminated.
    /// </summary>
    private void EmitComboEnded()
    {
        EmitSignal(SignalName.ComboEnded, null);
    }

    /// <summary>
    /// Emits the ComboCanceled signal to notify listeners that the current combo has been canceled.
    /// This is typically used internally when a combo sequence ends prematurely or is invalidated.
    /// </summary>
    private void EmitComboCancelled()
    {
        EmitSignal(SignalName.ComboCanceled, null);
    }
    #endregion
}
