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
using System.Linq;
using Godot;
using Snake.Scripts.Domain.Utilities;
using Snake.Scripts.Interfaces;

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Represents the visual state of a single segment of the snake in the game.
/// This structure contains data related to the appearance and transformation
/// of a snake segment, including frame index, rotation, offset, and flipping status.
/// </summary>
public struct SnakeVisualState
{
	public int Frame;
	public float Rotation;
	public Vector2 Offset;
	public bool FlipH;
	public bool FlipV;
}

/// <summary>
/// Manages the configuration, behavior, and state of the snake in the game.
/// This includes handling movement, growth, direction changes, and collision.
/// </summary>
public partial class SnakeManager : GodotObject, ISnakeManager
{
	#region Signals
	/// <summary>
	/// Handles the event triggered to request the start of the game.
	/// </summary>
	/// <remarks>
	/// This delegate defines the signature for event callbacks that are invoked when there is a request
	/// to start the game. Subscribers can respond to this event to initialize or execute actions
	/// necessary for starting the gameplay session, such as resetting game states, updating UI, or
	/// spawning game elements.
	/// </remarks>
	[Signal]
	public delegate void RequestStartGameEventHandler();

	/// <summary>
	/// Handles the event triggered when the snake's movement direction changes in the game.
	/// </summary>
	/// <remarks>
	/// This delegate defines the signature for event callbacks that are invoked when the snake's
	/// movement direction is updated. It provides the new movement direction as a Vector2I and the
	/// associated action name as a string, allowing subscribers to respond dynamically to directional
	/// changes, such as updating animations, gameplay mechanics, or related features.
	/// </remarks>
	[Signal]
	public delegate void DirectionChangedEventHandler(Vector2I direction, string action);

	/// <summary>
	/// Handles the event triggered when a new snake segment is added to the game.
	/// </summary>
	/// <remarks>
	/// This delegate defines the signature for event callbacks that manage the addition of
	/// a new segment to the snake. It provides the added segment, represented as a Node2D object,
	/// enabling subscribers to execute logic such as updating the game state, visual representation,
	/// or other gameplay-related features when a new segment is appended.
	/// </remarks>
	[Signal]
	public delegate void SegmentAddedEventHandler(Node2D node);

	/// <summary>
	/// Handles the event triggered when a snake segment is removed from the game.
	/// </summary>
	/// <remarks>
	/// This delegate defines the signature for event callbacks that handle the removal of
	/// a segment from the snake. It provides the removed segment, represented as a Node2D object,
	/// allowing subscribers to execute appropriate logic, such as updating the game state or
	/// visual effects when a segment is detached.
	/// </remarks>
	[Signal]
	public delegate void SegmentRemovedEventHandler(Node2D node);
	#endregion

	#region Properties & Fields
	/// <summary>
	/// Represents the movement vector for the snake's upward direction.
	/// </summary>
	/// <remarks>
	/// This constant defines a unit vector used to move the snake one step upward
	/// within the game grid. It ensures consistency in controlling upward movement
	/// and is used throughout the game logic for directional calculations.
	/// Modifying this value impacts all upward movements executed by the snake.
	/// </remarks>
	public static readonly Vector2I UpMove = new(0, -1);

	/// <summary>
	/// Represents the movement vector for the snake's downward direction.
	/// </summary>
	/// <remarks>
	/// This constant defines a unit vector used to move the snake one step downward
	/// within the game grid. It provides a standardized value for controlling
	/// downward movement and integrating directional logic throughout the game.
	/// Modifying this value affects all downward movements of the snake.
	/// </remarks>
	public static readonly Vector2I DownMove = new(0, 1);

	/// <summary>
	/// Represents the movement vector for the snake's leftward direction.
	/// </summary>
	/// <remarks>
	/// This constant defines a unit vector used to move the snake one step to the left
	/// on the game board. It serves as a preconfigured value for consistent movement
	/// calculations and directional mapping throughout the game's logic. Changing this
	/// value impacts all leftward movements of the snake.
	/// </remarks>
	public static readonly Vector2I LeftMove = new(-1, 0);

	/// <summary>
	/// Represents the movement vector for the snake's rightward direction.
	/// </summary>
	/// <remarks>
	/// This constant defines a unit vector used to move the snake one step to the right
	/// on the game board. It serves as a preconfigured value for consistent movement
	/// calculations and directional mapping throughout the game's logic. Changing this
	/// value impacts all rightward movements of the snake.
	/// </remarks>
	public static readonly Vector2I RightMove = new(1, 0);

	/// <summary>
	/// Represents the packed scene used to generate individual segments of the snake.
	/// </summary>
	/// <remarks>
	/// This property defines the template for creating new snake segments during gameplay.
	/// Each segment is instantiated from this packed scene, allowing for flexible customization
	/// of the snake's appearance and behavior.
	/// Modifying this property enables changes to the snake's segment structure across the game.
	/// </remarks>
	public PackedScene SnakeSegmentPs { get; set; }

	/// <summary>
	/// Defines the size of a single cell in pixels on the game grid.
	/// </summary>
	/// <remarks>
	/// This property determines the dimensions of each grid cell on which the game entities, such as the snake, operate.
	/// The value directly impacts how game objects are positioned and rendered within the grid system.
	/// Adjusting this property alters the scale of the grid and the visual size of the playing field.
	/// </remarks>
	public int CellPixelSize { get; set; }

	/// <summary>
	/// Represents the current movement direction of the snake.
	/// </summary>
	/// <remarks>
	/// This property defines the direction in which the snake is moving on the game grid.
	/// It is updated based on player input and is used to calculate the next position of the snake's head.
	/// Movement is typically represented as a vector, where the X and Y components indicate direction along
	/// the horizontal and vertical axes, respectively. The property ensures that the snake cannot reverse
	/// directly into itself by restricting updates to valid directional changes.
	/// </remarks>
	public Vector2I MoveDirection { get; set; }

	/// <summary>
	/// Indicates whether the snake's movement controls are reversed.
	/// </summary>
	/// <remarks>
	/// This property determines if the directional controls for the snake are inverted, affecting how player inputs
	/// translate to the snake's movement. When set to true, directional inputs will have reversed behavior.
	/// Commonly used in gameplay mechanics to introduce challenges or power-up effects.
	/// </remarks>
	public bool ControlsReversed { get; set; }

	/// <summary>
	/// Stores the previous positions of all segments of the snake before the current update.
	/// </summary>
	/// <remarks>
	/// This property keeps a snapshot of the snake's segment positions from the last game update.
	/// It is primarily used to track the snake's previous state for handling operations such as movement reversals,
	/// collision detection, or tail segment manipulation. Each position is represented as a <see cref="Vector2I"/>
	/// that specifies the segment location in grid coordinates.
	/// </remarks>
	public List<Vector2I> OldData { get; set; }

	/// <summary>
	/// Represents the current data of the snake, including the positions of its segments.
	/// </summary>
	/// <remarks>
	/// This property holds the list of positions for all segments of the snake, with each segment represented
	/// as a <see cref="Vector2I"/>. The first entry denotes the head of the snake, while further entries
	/// represent the body segments in the order of their connection. It is used to manage the snake's layout
	/// during gameplay and is updated whenever the snake moves or gains a new segment.
	/// </remarks>
	public List<Vector2I> SnakeData { get; set; }
	internal List<SnakeVisualState> OldSnakeStates; // Changed from List<Node2D>
	internal List<Node2D> SnakeNodes;
	private Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> _headDirection;
	internal List<string> SnakeMoveData;
	internal List<string> OldSnakeMoveData;
	private readonly Vector2I _startPosition = new (14, 16);
	#endregion
	
	#region Initialization & Disposal
	/// <summary>
	/// Initializes the data structures and visual components needed for the snake's functionality
	/// and appearance in the game. This method clears any existing state, resets collections
	/// relevant to the snake's movements and visual representation, and starts the game with
	/// a default snake configuration by adding a predefined number of segments.
	/// </summary>
	internal void GenerateSnake()
	{
		// ReSharper disable once UseCollectionExpression
		OldData = new List<Vector2I>();
		// ReSharper disable once UseCollectionExpression
		SnakeData = new List<Vector2I>();
		// ReSharper disable once UseCollectionExpression
		OldSnakeStates = new List<SnakeVisualState>();
		// ReSharper disable once UseCollectionExpression
		SnakeNodes = new List<Node2D>();
		// ReSharper disable once UseCollectionExpression
		SnakeMoveData = new List<string>();
		// ReSharper disable once UseCollectionExpression
		OldSnakeMoveData = new List<string>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}

	/// <summary>
	/// Cleans up resources and disposes of all currently active snake segments.
	/// Removes all snake segment nodes from the game and clears the internal data structures
	/// that track those nodes, ensuring a clean deallocation of memory and references.
	/// Calls the base Dispose method after completing the cleanup.
	/// </summary>
	public new void Dispose()
	{
		// Clear out all those orphaned clones
		if (SnakeNodes != null)
		{
			foreach (var node in SnakeNodes.Where(GodotObject.IsInstanceValid))
			{
				node.Free();
			}

			SnakeNodes.Clear();
		}
		
		base.Dispose();
	}
	#endregion

	#region Segment Management
	/// <summary>
	/// Adds a new segment to the snake at the specified position.
	/// The segment is visually created and appended to the snake's existing nodes,
	/// updating both the snake's logical and visual representations.
	/// Emits a signal after the segment is successfully added.
	/// </summary>
	/// <param name="position">The grid-based position where the new snake segment will be added.</param>
	public void AddSegment(Vector2I position)

	{
		SnakeData.Add(position);
		SnakeMoveData.Add("");
		var snakeSegment = SnakeSegmentPs.Instantiate<AnimatedSprite2D>();
		snakeSegment.Position = position * CellPixelSize + new Vector2I(0, CellPixelSize);
		switch (SnakeNodes.Count)
		{
			case 0:
				snakeSegment.Frame = 1;
				snakeSegment.Offset = new Vector2(-15, 15);
				snakeSegment.Rotation = 4.7183f;
				break;
			case >= 3:
				snakeSegment.Visible = false;
				break;
		}

		EmitSignal(SignalName.SegmentAdded, snakeSegment);
		SnakeNodes.Add(snakeSegment);
		
	}

	/// <summary>
	/// Removes the first segment of the snake's body, effectively shortening it from the head.
	/// </summary>
	/// <remarks>
	/// This method updates both the internal data representation and visual state of the snake.
	/// It emits a signal for the removed segment, enabling external actions such as cleanup or effects.
	/// </remarks>
	public void RemoveHead()
	{
		if (SnakeNodes.Count <= 0) return;
		var node = SnakeNodes[0];
		SnakeData.RemoveAt(0);
		SnakeNodes.RemoveAt(0);
		EmitSignal(SignalName.SegmentRemoved, node);
	}

	/// <summary>
	/// Removes a portion of the snake's body starting from a specified index, effectively shortening the snake.
	/// </summary>
	/// <remarks>
	/// This method ensures the snake's data and visual states remain consistent when segments are removed.
	/// It emits a signal for each removed segment, allowing additional actions or updates to occur externally.
	/// </remarks>
	/// <param name="index">The zero-based index from which tail segments should be removed.</param>
	public void RemoveTailFrom(int index)
	{
		for (int j = SnakeData.Count - 1; j > index; j--)
		{
			var node = SnakeNodes[j];
			SnakeData.RemoveAt(j);
			OldData.RemoveAt(j);
			SnakeNodes.RemoveAt(j);
			SnakeMoveData.RemoveAt(j);
			OldSnakeMoveData.RemoveAt(j);
			EmitSignal(SignalName.SegmentRemoved, node);
		}
	}
	#endregion

	#region Movement & State Updates
	/// <summary>
	/// Processes player input for directional movement of the snake during gameplay.
	/// This method listens for key presses or input actions mapped to snake movement
	/// and determines the new direction based on the current state of the controls.
	/// If a valid directional input is detected, it emits a signal to update the
	/// snake's movement direction and visual characteristics, such as rotation and offsets.
	/// Additionally, it emits a signal to request the game start if applicable.
	/// </summary>
	internal void KeyPressSnakeDirection()
	{
		foreach (KeyValuePair<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> action 
		         in _headDirection)
		{
			if (!Input.IsActionPressed(action.Key)) continue;
			string effectiveActionKey = InputLogic.GetEffectiveAction(action.Key, ControlsReversed);
			var effectiveDirection = _headDirection[effectiveActionKey];

			if (MoveDirection == -effectiveDirection.direction) continue;
			EmitSignal(SignalName.DirectionChanged, (Vector2I)effectiveDirection.direction, effectiveActionKey);

			var headSprite = (AnimatedSprite2D)SnakeNodes[0];
			headSprite.Rotation = effectiveDirection.rotation;
			headSprite.Offset = effectiveDirection.offset;
			headSprite.FlipV = effectiveDirection.flipV;
			headSprite.FlipH = effectiveDirection.flipH;

			EmitSignal(SignalName.RequestStartGame);
			break; // Exit the loop once a movement is processed
		}
	}

	/// <summary>
	/// Updates the snake's state and data, including its position, visual appearance, and segment arrangement
	/// based on the current movement direction, ensuring proper synchronization during gameplay.
	/// This method manages the movement of the snake head, updates all body segments, and handles
	/// visual state changes like animation frames, rotations, and offsets for the segments.
	/// </summary>
	internal void UpdateSnake()
	{
		OldData = SnakeData.ToList();
		OldSnakeMoveData = SnakeMoveData.ToList();

		// Save current visual states before moving
		// ReSharper disable once UseCollectionExpression
		OldSnakeStates = new List<SnakeVisualState>();
		foreach (var sprite in SnakeNodes.Cast<AnimatedSprite2D>())
		{
			OldSnakeStates.Add(new SnakeVisualState 
			{
				Frame = sprite.Frame,
				Rotation = sprite.Rotation,
				Offset = sprite.Offset,
				FlipH = sprite.FlipH,
				FlipV = sprite.FlipV
			});
		}
		
		SnakeData[0] += MoveDirection;// Update snake's head position data
		// Update other body segments data
		for (int i = 0; i < SnakeData.Count; i++)
		{
			// Copy frame data for other body segments
			AnimatedSprite2D currentSegment;
			if (i > 1 && i < SnakeNodes.Count - 1)
			{
				currentSegment = (AnimatedSprite2D)SnakeNodes[i];
				var previousState = OldSnakeStates[i - 1]; // Reading from struct
				currentSegment.Frame = previousState.Frame;
				currentSegment.Rotation = previousState.Rotation;
				currentSegment.Offset = previousState.Offset;
				currentSegment.FlipH = previousState.FlipH;
				currentSegment.FlipV = previousState.FlipV;
			}
			if (i > 0)
			{
				currentSegment = (AnimatedSprite2D)SnakeNodes[i];
				currentSegment.Visible = true;
				SnakeData[i] = OldData[i - 1];
				SnakeMoveData[i] = OldSnakeMoveData[i - 1];
			}
			HandleTailSegments(i);
			if (SnakeNodes.Count > 4 && i > 4)
			{
				BendTail();
			}
			if (SnakeNodes.Count == 5 && i > 3)
			{
				BendTail(false);
			}
			// Update the position of the segment sprite
			SnakeNodes[i].Position = SnakeData[i] * CellPixelSize + new Vector2I(0, CellPixelSize);
			// Handle new neck bending for the second segment
			if (i == 2)
			{
				BendNeckSegment();
			}
		}
	}
	#endregion

	#region Visuals & Animations
	/// <summary>
	/// Updates the visual state and properties of specific tail segments of the snake based on their
	/// position and direction, ensuring proper orientation, frame selection, and alignment during gameplay.
	/// </summary>
	/// <param name="i">
	/// The index of the snake segment being processed. The method determines the type of tail segment by
	/// comparing this index against the total number of segments in the snake.
	/// </param>
	private void HandleTailSegments(int i)
	{
		AnimatedSprite2D currentSegment;
		if (i == SnakeNodes.Count - 3 && SnakeNodes.Count > 5 || (i > 2 && SnakeNodes.Count < 5) || (i == 3 && SnakeNodes.Count == 5))
		{
			currentSegment = (AnimatedSprite2D)SnakeNodes[i];
			currentSegment.Frame = 6;
			string currentDirection = SnakeMoveData[i];
			foreach (KeyValuePair<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> action 
			         in _headDirection.Where(action => currentDirection == action.Key))
			{
				currentSegment.Rotation = action.Value.rotation;
				currentSegment.Offset = action.Value.offset;
				currentSegment.FlipV = action.Value.flipV;
				currentSegment.FlipH = action.Value.flipH;
			}
		}
		else if (i == SnakeNodes.Count - 2 && SnakeNodes.Count > 5 || (i > 3 && SnakeNodes.Count < 5) || (i == 4 && SnakeNodes.Count == 5))
		{
			currentSegment = (AnimatedSprite2D)SnakeNodes[i];
			currentSegment.Frame = 8;
			string currentDirection = SnakeMoveData[i];
			foreach (KeyValuePair<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> action 
			         in _headDirection.Where(action => currentDirection == action.Key))
			{
				currentSegment.Rotation = action.Value.rotation;
				currentSegment.Offset = action.Value.offset;
				currentSegment.FlipV = action.Value.flipV;
				currentSegment.FlipH = action.Value.flipH;
			}
		}
		else if (i == SnakeNodes.Count - 1 && SnakeNodes.Count > 5)
		{
			currentSegment = (AnimatedSprite2D)SnakeNodes[i];
			currentSegment.Frame = 10;
			string currentDirection = SnakeMoveData[i];
			foreach (KeyValuePair<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> action 
			         in _headDirection.Where(action => currentDirection == action.Key))
			{
				currentSegment.Rotation = action.Value.rotation;
				currentSegment.Offset = action.Value.offset;
				currentSegment.FlipV = action.Value.flipV;
				currentSegment.FlipH = action.Value.flipH;
			}
		}
	}

	/// <summary>
	/// Adjusts the visual appearance and animation states of the tail segments of the snake based on
	/// their relative movements, ensuring proper alignment and accurate visual representation during gameplay.
	/// </summary>
	/// <param name="fullTail">
	/// Indicates whether the tail update includes all tail segments (true) or excludes the last segment (false).
	/// If false, the method assumes the snake's tail has been partially truncated.
	/// </param>
	private void BendTail(bool fullTail = true)
	{
		AnimatedSprite2D tailBase = fullTail ? (AnimatedSprite2D)SnakeNodes[^3] : (AnimatedSprite2D)SnakeNodes[^2];
		AnimatedSprite2D tailShaft = fullTail ? (AnimatedSprite2D)SnakeNodes[^2] : (AnimatedSprite2D)SnakeNodes[^1];
		AnimatedSprite2D tailTip = fullTail ? (AnimatedSprite2D)SnakeNodes[^1] : null;
		string tailBaseMovement = fullTail ? SnakeMoveData[^3] : SnakeMoveData[^2];
		string tailShaftMovement = fullTail ? SnakeMoveData[^2] : SnakeMoveData[^1];
		string tailTipMovement = fullTail ? SnakeMoveData[^1] : null;
		
		tailBase.FlipV = false;
		tailShaft.FlipV = false;
		
		
		//If all segments are going in a straight line
		if (tailBaseMovement == tailShaftMovement && tailShaftMovement == tailTipMovement)
		{
			tailBase.Frame = 6;
			tailShaft.Frame = 8;
			if (tailTip != null)
			{
				tailTip.FlipV = false;
				tailTip.Frame = 10;
			}
		}
		//If the base segment is the only turning segment
		if (tailBaseMovement != tailShaftMovement && (tailShaftMovement == tailTipMovement || tailTipMovement == null))
		{
			tailBase.Frame = 7;
			tailBase.FlipV = tailBaseMovement switch
			{
				"move_right" => tailShaftMovement is "move_up",
				"move_up" => tailShaftMovement is "move_left",
				"move_left" => tailShaftMovement is "move_down",
				"move_down" => tailShaftMovement is "move_right",
				_ => tailBase.FlipV
			};
			tailShaft.Frame = 8;
			if (tailTip != null)
			{
				tailTip.FlipV = false;
				tailTip.Frame = 10;
			}
		}
		//if the shaft is the only turning segment
		if (tailTipMovement != null && tailBaseMovement == tailShaftMovement && tailShaftMovement != tailTipMovement)
		{
			tailBase.Frame = 6;
			tailShaft.Frame = 9;
			tailShaft.FlipV = tailShaftMovement switch
			{
				"move_right" => tailTipMovement is "move_down",
				"move_up" => tailTipMovement is "move_right",
				"move_left" => tailTipMovement is "move_up",
				"move_down" => tailTipMovement is "move_left",
				_ => tailShaft.FlipV
			};

			if (tailTip != null)
			{
				tailTip.FlipV = false;
				tailTip.Frame = 10;
			}
		}
		//if both segments are turning
		// ReSharper disable once InvertIf
		if (tailTipMovement != null && tailBaseMovement != tailShaftMovement && tailShaftMovement != tailTipMovement)
		{
			tailBase.Frame = 7;
			tailBase.FlipV = tailBaseMovement switch
			{
				"move_right" => tailShaftMovement is "move_up",
				"move_up" => tailShaftMovement is "move_left",
				"move_left" => tailShaftMovement is "move_down",
				"move_down" => tailShaftMovement is "move_right",
				_ => tailBase.FlipV
			};
			tailShaft.Frame = 9;
			tailShaft.FlipV = tailShaftMovement switch
			{
				"move_right" => tailTipMovement is "move_down",
				"move_up" => tailTipMovement is "move_right",
				"move_left" => tailTipMovement is "move_up",
				"move_down" => tailTipMovement is "move_left",
				_ => tailShaft.FlipV
			};
			if (tailTip != null) tailTip.Frame = 10;
		}
	}

	/// <summary>
	/// Updates the visual and positional properties of the neck segment of the snake based on the relative positions
	/// of the head, neck, and body segments, ensuring proper visual alignment and animation.
	/// </summary>
	/// <remarks>
	/// This method calculates whether the neck segment and its neighboring segments (head and body) are in a straight
	/// line or turning. If the segments form a straight line, the neck's animation frame, offset, and rotation are reset.
	/// In cases where the snake is turning, the method assigns the corresponding animation frame for the neck segment
	/// based on the direction of the turn derived from the relative positions of the neighboring segments.
	/// This ensures a smooth and accurate visual representation of the snake's movement.
	/// </remarks>
	private void BendNeckSegment()
	{
		AnimatedSprite2D neck = (AnimatedSprite2D)SnakeNodes[1];
		Vector2 headPosition = SnakeNodes[0].Position;
		Vector2 neckPosition = SnakeNodes[1].Position;
		Vector2 bodyPosition = SnakeNodes[2].Position;

		// Check if the snake is in a straight line
		if ((headPosition.X == neckPosition.X && neckPosition.X == bodyPosition.X) ||
			(headPosition.Y == neckPosition.Y && neckPosition.Y == bodyPosition.Y))
		{
			neck.Frame = 0;
			neck.Offset = new Vector2(15, 15);
			neck.Rotation = 0f;
			return;
		}

		// Determine relative positions
		bool isHeadAboveBody = headPosition.Y < bodyPosition.Y;
		bool isHeadBelowBody = headPosition.Y > bodyPosition.Y;
		bool isHeadRightOfBody = headPosition.X > bodyPosition.X;
		bool isHeadLeftOfBody = headPosition.X < bodyPosition.X;

		if (isHeadAboveBody)
		{
			if (isHeadRightOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 4 : 2;
			}
			else if (isHeadLeftOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 5 : 3;
			}
		}
		else if (isHeadBelowBody)
		{
			if (isHeadRightOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 3 : 5;
			}
			else if (isHeadLeftOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 2 : 4;
			}
		}
	}

	/// <summary>
	/// Updates the visual state of the snake's head and its immediate segment to indicate that the snake is dead.
	/// </summary>
	/// <remarks>
	/// This method modifies the visual properties of the first two segments of the snake. It sets specific
	/// rotation, offset, flipping, and animation frame values to reflect the snake's dead state. This
	/// change focuses on visual feedback for the game's end condition.
	/// </remarks>
	public void DeadSnake()
	{
		var test1 = (AnimatedSprite2D)SnakeNodes[0];

	var test2 = (AnimatedSprite2D)SnakeNodes[1];
		test2.Rotation = test1.Rotation;
		test2.Offset = test1.Offset;
		test2.FlipH = test1.FlipH;
		test2.FlipV = test1.FlipV;
		test2.Frame = 11;
	}

	/// <summary>
	/// Creates a deep copy of the specified AnimatedSprite2D instance by replicating its properties and configuration.
	/// </summary>
	/// <param name="original">The AnimatedSprite2D instance to be cloned.</param>
	/// <returns>A new AnimatedSprite2D instance with the same properties and state as the original.</returns>
	internal AnimatedSprite2D CloneAnimatedSprite2D(AnimatedSprite2D original)
	{
		// Create a new instance
		var copy = new AnimatedSprite2D();

		// Copy essential properties
		copy.Name = original.Name;
		copy.Position = original.Position;
		copy.Offset = original.Offset;
		copy.Scale = original.Scale;
		copy.Rotation = original.Rotation;
		copy.Visible = original.Visible;
		copy.SpriteFrames = original.SpriteFrames;

		// Copy animation-related properties
		copy.Frame = original.Frame;
		copy.Animation = original.Animation;
		copy.Autoplay = original.Autoplay;

		return copy;
	}
	#endregion

	#region Mapping & Configuration
	/// <summary>
	/// Maps input actions to corresponding movement directions, visual settings, and head directions for the snake.
	/// </summary>
	/// <remarks>
	/// This method associates specific input actions with their corresponding direction vectors, rotations,
	/// and visual adjustments like flipping and offsets. It serves as the core configuration for determining how
	/// the snake responds to user input during gameplay.
	/// </remarks>
	public void MapDirections()
	{
		// Map input actions to movement directions and settings
		_headDirection = new Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)>
		{
			{ "move_down",  (new Vector2(15, -15), 1.5708f, false, false, DownMove) },
			{ "move_up",    (new Vector2(-15, 15), 4.7183f, false, false, UpMove) },
			{ "move_left",  (new Vector2(-15, -15), 3.1416f, true, false, LeftMove) },
			{ "move_right", (new Vector2(15, 15), 0f, false, false, RightMove) }
		};
	}

	/// <summary>
	/// Updates the move data for the snake, including its current direction and associated action.
	/// </summary>
	/// <param name="action">The action string representing the snake's movement type or state.</param>
	/// <param name="direction">The new movement direction as a Vector2I.</param>
	public void SetMoveData(string action, Vector2I direction)
	{
		SnakeMoveData[0] = action;
		MoveDirection = direction;
	}

	/// <summary>
	/// Sets the PackedScene to be used for creating snake segments.
	/// </summary>
	/// <param name="snakeSegmentPs">The PackedScene instance representing the snake segment.</param>
	public void SetSnakeSegmentPs(PackedScene snakeSegmentPs)
	{
		SnakeSegmentPs = snakeSegmentPs;
	}

	/// <summary>
	/// Sets the size of a single board cell in pixels.
	/// </summary>
	/// <param name="boardCellPixelSize">The size in pixels for one board cell.</param>
	public void SetCellPixelSizeRef(int boardCellPixelSize)
	{
		CellPixelSize = boardCellPixelSize;
	}
	#endregion
}
