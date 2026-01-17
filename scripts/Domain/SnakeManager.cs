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
using System.Diagnostics;
using System.Linq;
using Godot;

namespace Snake.scripts.Domain;

public struct SnakeVisualState
{
	public int Frame;
	public float Rotation;
	public Vector2 Offset;
	public bool FlipH;
	public bool FlipV;
}

public partial class SnakeManager : GodotObject, ISnakeManager
{
	[Signal] public delegate void RequestStartGameEventHandler();
	[Signal] public delegate void DirectionChangedEventHandler(Vector2I direction, string action);
	[Signal] public delegate void SegmentAddedEventHandler(Node2D node);
	[Signal] public delegate void SegmentRemovedEventHandler(Node2D node);

	// Directions
	public static readonly Vector2I UpMove = new(0, -1);
	public static readonly Vector2I DownMove = new(0, 1);
	public static readonly Vector2I LeftMove = new(-1, 0);
	public static readonly Vector2I RightMove = new(1, 0);

	public PackedScene SnakeSegmentPs { get; set; }
	public int CellPixelSize { get; set; }
	public Vector2I MoveDirection { get; set; }
	public bool ControlsReversed { get; set; }

	public List<Vector2I> OldData { get; set; }
	public List<Vector2I> SnakeData { get; set; }
	internal List<SnakeVisualState> OldSnakeStates; // Changed from List<Node2D>
	internal List<Node2D> SnakeNodes;
	private Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> _headDirection;
	internal List<string> SnakeMoveData;
	internal List<string> OldSnakeMoveData;
	private readonly Vector2I _startPosition = new (14, 16);
	//internal bool CanMove;
	
	public SnakeManager()
	{
	}

	internal void GenerateSnake()
	{
		//Dispose();
		OldData = new List<Vector2I>();
		SnakeData = new List<Vector2I>();
		OldSnakeStates = new List<SnakeVisualState>();
		SnakeNodes = new List<Node2D>();
		SnakeMoveData = new List<string>();
		OldSnakeMoveData = new List<string>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}
	
	public new void Dispose()
	{
		// Clear out all those orphaned clones
		if (SnakeNodes != null)
		{
			foreach (var node in SnakeNodes)
			{
				if (GodotObject.IsInstanceValid(node)) node.Free();
				//node.QueueFree();
			}
			SnakeNodes.Clear();
		}
		
		base.Dispose();
	}


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
		//OldSnakeNodes.Add(snakeSegment);
		
	}

	internal void KeyPressSnakeDirection()
	{
		//if (!CanMove) return;
		foreach (var action in _headDirection)
		{
			if (Input.IsActionPressed(action.Key))
			{
				var effectiveActionKey = InputLogic.GetEffectiveAction(action.Key, ControlsReversed);
				var effectiveDirection = _headDirection[effectiveActionKey];

				if (MoveDirection != -effectiveDirection.direction)
				{
					EmitSignal(SignalName.DirectionChanged, (Vector2I)effectiveDirection.direction, effectiveActionKey);
					//CanMove = false;  //MEMO: I believe setting this to false is what is causing feedback about input responsiveness. But removing this creates the possibility of turning back on yourself.
					//TODO: Leave in for future testing. Implement check to prevent turning back on self (180degree). Can possibly remove property from class completely.


					var headSprite = (AnimatedSprite2D)SnakeNodes[0];
					headSprite.Rotation = effectiveDirection.rotation;
					headSprite.Offset = effectiveDirection.offset;
					headSprite.FlipV = effectiveDirection.flipV;
					headSprite.FlipH = effectiveDirection.flipH;

					EmitSignal(SignalName.RequestStartGame);
					break; // Exit the loop once a movement is processed
				}
			}
		}
	}

	internal void UpdateSnake()
	{
		//CanMove = false;  //MEMO: I believe setting this to false is what is causing feedback about input responsiveness. But removing this creates the possibility of turning back on yourself.
		//TODO: Leave in for future testing. Implement check to prevent turning back on self (180degree). Can possibly remove property from class completely.

		OldData = SnakeData.ToList();
		OldSnakeMoveData = SnakeMoveData.ToList();

		// Save current visual states before moving
		OldSnakeStates = new List<SnakeVisualState>();
		for (int i = 0; i < SnakeNodes.Count; i++)
		{
			var sprite = (AnimatedSprite2D)SnakeNodes[i];
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

	private void HandleTailSegments(int i)
	{
		AnimatedSprite2D currentSegment;
		if (i == SnakeNodes.Count - 3 && SnakeNodes.Count > 5 || (i > 2 && SnakeNodes.Count < 5) || (i == 3 && SnakeNodes.Count == 5))
		{
			currentSegment = (AnimatedSprite2D)SnakeNodes[i];
			currentSegment.Frame = 6;
			var currentDirection = SnakeMoveData[i];
			foreach (var action in _headDirection)
			{
				if (currentDirection == action.Key)
				{
					currentSegment.Rotation = action.Value.rotation;
					currentSegment.Offset = action.Value.offset;
					currentSegment.FlipV = action.Value.flipV;
					currentSegment.FlipH = action.Value.flipH;
				}
			}
		}
		else if (i == SnakeNodes.Count - 2 && SnakeNodes.Count > 5 || (i > 3 && SnakeNodes.Count < 5) || (i == 4 && SnakeNodes.Count == 5))
		{
			currentSegment = (AnimatedSprite2D)SnakeNodes[i];
			currentSegment.Frame = 8;
			var currentDirection = SnakeMoveData[i];
			foreach (var action in _headDirection)
			{
				if (currentDirection == action.Key)
				{
					currentSegment.Rotation = action.Value.rotation;
					currentSegment.Offset = action.Value.offset;
					currentSegment.FlipV = action.Value.flipV;
					currentSegment.FlipH = action.Value.flipH;
				}
			}
		}
		else if (i == SnakeNodes.Count - 1 && SnakeNodes.Count > 5)
		{
			currentSegment = (AnimatedSprite2D)SnakeNodes[i];
			currentSegment.Frame = 10;
			var currentDirection = SnakeMoveData[i];
			foreach (var action in _headDirection)
			{
				if (currentDirection == action.Key)
				{
					currentSegment.Rotation = action.Value.rotation;
					currentSegment.Offset = action.Value.offset;
					currentSegment.FlipV = action.Value.flipV;
					currentSegment.FlipH = action.Value.flipH;
				}
			}
		}
	}

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
			Debug.Assert(tailTip != null, nameof(tailTip) + " != null");
			tailTip.Frame = 10;
		}
	}

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

	public void RemoveHead()
	{
		if (SnakeNodes.Count > 0)
		{
			var node = SnakeNodes[0];
			SnakeData.RemoveAt(0);
			SnakeNodes.RemoveAt(0);
			EmitSignal(SignalName.SegmentRemoved, node);
		}
	}

	public void RemoveTailFrom(int index)
	{
		for (int j = SnakeData.Count - 1; j > index; j--)
		{
			var node = SnakeNodes[j];
			SnakeData.RemoveAt(j);
			OldData.RemoveAt(j);
			SnakeNodes.RemoveAt(j);
			//OldSnakeNodes.RemoveAt(j);
			SnakeMoveData.RemoveAt(j);
			OldSnakeMoveData.RemoveAt(j);
			EmitSignal(SignalName.SegmentRemoved, node);
		}
	}

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
}
