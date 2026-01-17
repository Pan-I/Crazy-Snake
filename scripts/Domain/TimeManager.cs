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

namespace Snake.scripts.Domain;

public partial class TimeManager : GodotObject
{
    private Timer _moveTimer;
    private Timer _hudFlashTimer;
    private Timer _healthTimer;

    public void Initialize(Timer moveTimer, Timer hudFlashTimer, Timer healthTimer)
    {
        _moveTimer = moveTimer;
        _hudFlashTimer = hudFlashTimer;
        _healthTimer = healthTimer;
    }

    public void StartMoveTimer() => _moveTimer.Start();
    public void StopMoveTimer() => _moveTimer.Stop();
    public void SetMoveTimerWaitTime(double waitTime) => _moveTimer.WaitTime = waitTime;
    public double GetMoveTimerWaitTime() => _moveTimer.WaitTime;

    public void StartHudFlashTimer() => _hudFlashTimer.Start();
    public void StopHudFlashTimer() => _hudFlashTimer.Stop();

    public void StartHealthTimer() => _healthTimer.Start();
    public void StopHealthTimer() => _healthTimer.Stop();
}
