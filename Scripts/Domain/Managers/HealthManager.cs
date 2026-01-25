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
/// Manages the health system for the game, including tracking health points,
/// signaling events related to health changes, and managing associated health segments.
/// </summary>
public partial class HealthManager : GodotObject
{
    /// <summary>
    /// Signal triggered when a deduction in the player's health occurs.
    /// </summary>
    /// <remarks>
    /// This delegate defines an event emitted whenever the player's health is reduced as a response to in-game events such as collisions or penalties.
    /// Subscribers to this signal can implement logic for updating UI elements, sound effects, or other health-related feedback mechanisms.
    /// It enables modular handling of health deductions, supporting a flexible and easily maintainable game architecture.
    /// Common use cases include decrementing health icons or triggering visual cues indicating damage taken.
    /// </remarks>
    [Signal]
    public delegate void HealthDeductedEventHandler();

    /// <summary>
    /// Signal triggered to request the end of the game.
    /// </summary>
    /// <remarks>
    /// This delegate defines an event invoked when the game state reaches a condition requiring termination.
    /// It is used to notify subscribers, such as UI controllers or game state managers, to handle game-over logic.
    /// Typical scenarios for emitting this signal include player health depletion or other defined failure states.
    /// Subscribing to this signal allows for implementing custom cleanup, logging, or user notifications.
    /// </remarks>
    [Signal]
    public delegate void GameOverRequestedEventHandler();

    /// <summary>
    /// Signal triggered upon the addition of a health segment in the game.
    /// </summary>
    /// <remarks>
    /// This delegate defines an event that occurs whenever a new health segment, represented by a Node2D object, is added.
    /// It is primarily used to handle the game's state update when the player's health increases or a new health segment is introduced.
    /// Objects subscribing to this signal may use the provided Node2D parameter to implement additional logic
    /// or manage the visual representation of the new health segment.
    /// </remarks>
    [Signal]
    public delegate void HealthSegmentAddedEventHandler(Node2D node);

    /// <summary>
    /// Signal triggered upon the removal of a health segment in the game.
    /// </summary>
    /// <remarks>
    /// This delegate defines an event that occurs whenever a health segment, represented by a Node2D object, is removed.
    /// It is primarily used to manage and update the game's state when the player's health decreases and a corresponding
    /// segment is removed. Objects subscribing to this signal may use the provided Node2D parameter to apply
    /// additional logic or cleanup related to the removed health segment.
    /// </remarks>
    [Signal]
    public delegate void HealthSegmentRemovedEventHandler(Node2D node);

    /// <summary>
    /// Represents the current number of lives available to the player in the game.
    /// </summary>
    /// <remarks>
    /// This property keeps track of the player's remaining lives. It reflects the current state of health by determining
    /// how many segments of health are active at any given time. Modifications to the player's health, such as damage
    /// taken or health restored, directly impact this value. The value is initialized during the game's setup process and
    /// updated whenever health-related events occur.
    /// </remarks>
    public int Lives { get; private set; }
    private const int HealthCapacity = 6;

    /// <summary>
    /// Contains the collection of health segments represented as <c>Node2D</c> instances.
    /// </summary>
    /// <remarks>
    /// This property maintains the graphical representation of the player's health using <c>Node2D</c> objects.
    /// Each node corresponds to an individual health segment displayed in the game. Modifications to the player's health,
    /// such as adding or removing segments, directly impact this collection. The nodes are initialized and positioned
    /// based on the game's health logic, ensuring the proper visual depiction of the player's remaining lives.
    /// </remarks>
    public List<Node2D> HealthNodes { get; } = new()
    {
        Capacity = HealthCapacity
    };

    /// <summary>
    /// Represents the health data of the player in the form of a list of grid positions.
    /// </summary>
    /// <remarks>
    /// This property stores the positions of health segments as a list of <c>Vector2I</c> instances.
    /// Each position corresponds to a health segment on the game grid. The size of the list is
    /// directly tied to the current number of health segments the player has remaining.
    /// When health is deducted or added, this list is updated accordingly.
    /// </remarks>
    public List<Vector2I> HealthData { get; } = new()
    {
        Capacity = HealthCapacity
    };

    /// <summary>
    /// Resets the health system by clearing all health nodes and health data.
    /// </summary>
    /// <remarks>
    /// This method removes all tracked health nodes and health data, effectively resetting the state of the health system.
    /// It is typically used when reinitializing the health state or restarting the game.
    /// </remarks>
    public void Reset()
    {
        HealthNodes.Clear();
        HealthData.Clear();
    }

    /// <summary>
    /// Initializes the health system by resetting the health state, adding the initial health segments, and updating the total lives count.
    /// </summary>
    /// <remarks>
    /// This method clears the existing health data and nodes, and populates the health system with a predefined number of health segments.
    /// It positions the segments sequentially starting from the specified initial position, with spacing determined by the cell pixel size.
    /// </remarks>
    /// <param name="startPosition">The starting position where the first health segment will be placed.</param>
    /// <param name="cellPixelSize">The size of a single cell, used to calculate the spacing between health segments.</param>
    /// <param name="snakeSegmentPs">The preloaded scene used to instantiate health segment objects.</param>
    public void Initialize(Vector2 startPosition, int cellPixelSize, PackedScene snakeSegmentPs)
    {
        Reset();
        var test = new Vector2I((int)(startPosition.X + 8), (int)(startPosition.Y + 10));
        for (int i = 0; i < HealthCapacity; i++)
        {
            AddHealthSegment(test + new Vector2I((int)(i * cellPixelSize * 1.5), 0), snakeSegmentPs);
        }
        Lives = HealthNodes.Count;
    }

    /// <summary>
    /// Adds a new health segment at the specified position using the provided scene instance.
    /// Emits a signal to notify that a health segment has been added.
    /// </summary>
    /// <remarks>
    /// The method updates the health data and node lists with the new health segment.
    /// The position and scale of the instantiated segment are set based on the given parameters.
    /// If the number of health nodes reaches six, the frame of the segment is adjusted accordingly.
    /// </remarks>
    /// <param name="position">The position where the new health segment will be placed.</param>
    /// <param name="snakeSegmentPs">The preloaded scene used to create the health segment.</param>
    private void AddHealthSegment(Vector2I position, PackedScene snakeSegmentPs)
    {
        HealthData.Add(position);
        var healthSegment = snakeSegmentPs.Instantiate<AnimatedSprite2D>();
        healthSegment.Position = position;
        healthSegment.Scale = new Vector2((float)1.5, (float)1.5);
        
        HealthNodes.Add(healthSegment);
        if (HealthNodes.Count == HealthCapacity)
        {
            healthSegment.Frame = 1;
        }
        EmitSignal(SignalName.HealthSegmentAdded, healthSegment);
    }

    /// <summary>
    /// Reduces the player's health by removing the last health segment.
    /// Emits signals to notify about the health deduction, health segment removal,
    /// or game over if no lives remain.
    /// </summary>
    /// <remarks>
    /// If the health list is empty, this method will return early.
    /// The last health segment node is removed, and the corresponding signal is emitted.
    /// If lives are greater than one, the lives count is decremented, and a health deduction signal is emitted.
    /// If no lives remain, a game-over signal is emitted instead.
    /// </remarks>
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
            EmitSignal(SignalName.HealthDeducted);
        }
        else
        {
            EmitSignal(SignalName.GameOverRequested);
        }
    }
}
