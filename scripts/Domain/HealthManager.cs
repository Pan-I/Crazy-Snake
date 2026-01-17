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

using System.Collections.Generic;
using Godot;

namespace Snake.scripts;

public partial class HealthManager : GodotObject
{
    [Signal] public delegate void HealthDeductedEventHandler();
    [Signal] public delegate void GameOverRequestedEventHandler();
    [Signal] public delegate void HealthSegmentAddedEventHandler(Node2D node);
    [Signal] public delegate void HealthSegmentRemovedEventHandler(Node2D node);

    public int Lives { get; private set; }
    public List<Node2D> HealthNodes { get; } = new();
    public List<Vector2I> HealthData { get; } = new();

    public void Reset()
    {
        HealthNodes.Clear();
        HealthData.Clear();
    }

    public void Initialize(Vector2 startPosition, int cellPixelSize, PackedScene snakeSegmentPs)
    {
        Reset();
        var test = new Vector2I((int)(startPosition.X + 8), (int)(startPosition.Y + 10));
        for (int i = 0; i < 6; i++)
        {
            AddHealthSegment(test + new Vector2I((int)(i * cellPixelSize * 1.5), 0), cellPixelSize, snakeSegmentPs);
        }
        Lives = HealthNodes.Count;
    }

    private void AddHealthSegment(Vector2I position, int cellPixelSize, PackedScene snakeSegmentPs)
    {
        HealthData.Add(position);
        var healthSegment = snakeSegmentPs.Instantiate<AnimatedSprite2D>();
        healthSegment.Position = position;
        healthSegment.Scale = new Vector2((float)1.5, (float)1.5);
        
        HealthNodes.Add(healthSegment);
        if (HealthNodes.Count == 6)
        {
            healthSegment.Frame = 1;
        }
        EmitSignal(SignalName.HealthSegmentAdded, healthSegment);
    }

    public void DeductHealth()
    {
        if (HealthNodes.Count == 0) return;

        HealthData.RemoveAt(HealthData.Count - 1);
        Node2D healthSegment = HealthNodes[^1];
        HealthNodes.RemoveAt(HealthNodes.Count - 1);
        EmitSignal(SignalName.HealthSegmentRemoved, healthSegment);

        if (HealthNodes.Count > 0)
        {
            var lastSegment = (AnimatedSprite2D)HealthNodes[^1];
            lastSegment.Frame = 1;
        }

        if (Lives > 1)
        {
            Lives--;
        }

        EmitSignal(SignalName.HealthDeducted);

        if (Lives <= 1)
        {
            EmitSignal(SignalName.GameOverRequested);
        }
    }
}
