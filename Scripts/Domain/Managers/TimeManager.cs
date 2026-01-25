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
using Timer = Godot.Timer;

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Manages game logic timers such as movement updates, HUD flashing, and health updates in the game.
/// </summary>
public partial class TimeManager : GodotObject
{
    #region Fields

    private Timer _moveTimer;
    private Timer _hudFlashTimer;
    private Timer _healthTimer;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the TimeManager by setting the provided timer instances for movement, HUD flashing, and health updates.
    /// </summary>
    /// <param name="moveTimer">The timer responsible for controlling movement updates.</param>
    /// <param name="hudFlashTimer">The timer responsible for managing periodic HUD flashing actions.</param>
    /// <param name="healthTimer">The timer responsible for managing periodic health-related updates.</param>
    public void Initialize(Timer moveTimer, Timer hudFlashTimer, Timer healthTimer)
    {
        _moveTimer = moveTimer;
        _hudFlashTimer = hudFlashTimer;
        _healthTimer = healthTimer;
    }

    #endregion

    #region Move Timer

    /// <summary>
    /// Starts the move timer, enabling periodic movement updates in the game logic.
    /// </summary>
    public void StartMoveTimer() => _moveTimer.Start();

    /// <summary>
    /// Stops the move timer, halting the periodic movement updates in the game logic.
    /// </summary>
    public void StopMoveTimer() => _moveTimer.Stop();

    /// <summary>
    /// Sets the wait time interval for the move timer, controlling the delay between movements in the game logic.
    /// </summary>
    /// <param name="waitTime">The desired wait time interval for the move timer, in seconds.</param>
    public void SetMoveTimerWaitTime(double waitTime) => _moveTimer.WaitTime = waitTime;

    /// <summary>
    /// Retrieves the current wait time interval configured for the move timer.
    /// Use this to determine the delay between movements in the game logic.
    /// </summary>
    /// <returns>The wait time interval of the move timer in seconds.</returns>
    public double GetMoveTimerWaitTime() => _moveTimer.WaitTime;

    #endregion

    #region HUD Flash Timer

    /// <summary>
    /// Initiates the HUD flash timer, activating time-based flashing functionality
    /// associated with it. Use this to enable recurring HUD flash effects
    /// dependent on the timer's operation.
    /// </summary>
    public void StartHudFlashTimer() => _hudFlashTimer.Start();

    /// <summary>
    /// Stops the HUD flash timer, disabling any active time-based flashing functionality
    /// associated with it. Use this to halt the recurring HUD flash effects that depend
    /// on the timer's operation.
    /// </summary>
    public void StopHudFlashTimer() => _hudFlashTimer.Stop();

    #endregion

    #region Health Timer

    /// <summary>
    /// Starts the health timer, enabling time-based logic or updates associated with health.
    /// Use this to initiate recurring health-related events driven by the timer.
    /// </summary>
    public void StartHealthTimer() => _healthTimer.Start();

    /// <summary>
    /// Stops the health timer, preventing further timeout signals from being emitted.
    /// This can be used to halt health-related updates or logic tied to the timer.
    /// </summary>
    public void StopHealthTimer() => _healthTimer.Stop();

    #endregion
}
