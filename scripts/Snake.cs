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

namespace Snake.scripts;

public class Snake
{
	private readonly Main _main;
	internal List<Vector2I> OldData;
	internal List<Vector2I> SnakeData;
	internal List<Node2D> OldSnakeNodes;
	internal List<Node2D> SnakeNodes;
	private Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> _headDirection;
	internal List<string> SnakeMoveData;
	internal List<string> OldSnakeMoveData;
	private readonly Vector2I _startPosition = new (14, 16);
	internal bool CanMove;

	public Snake(Main main)
	{
		_main = main;
	}

	internal void GenerateSnake()
	{
		OldData = new List<Vector2I>();
		SnakeData = new List<Vector2I>();
		OldSnakeNodes = new List<Node2D>();
		SnakeNodes = new List<Node2D>();
		SnakeMoveData = new List<string>();
		OldSnakeMoveData = new List<string>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}

	internal void AddSegment(Vector2I position)
	
	{
		SnakeData.Add(position);
		SnakeMoveData.Add("");
		var snakeSegment = _main.SnakeSegmentPs.Instantiate<AnimatedSprite2D>();
		snakeSegment.Position = position * _main.CellPixelSize + new Vector2I(0, _main.CellPixelSize);
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

		_main.AddChild(snakeSegment);
		SnakeNodes.Add(snakeSegment);
		OldSnakeNodes.Add(snakeSegment);
		
	}

	internal void KeyPressSnakeDirection()
	{
		if (!CanMove) return;
		foreach (var action in _headDirection)
		{
			if (Input.IsActionPressed(action.Key) && _main.MoveDirection != -action.Value.direction)
			{
				_main.MoveDirection = (Vector2I)action.Value.direction;
				SnakeMoveData[0] = action.Key; 
				CanMove = false;

				var headSprite = (AnimatedSprite2D)SnakeNodes[0];
				headSprite.Rotation = action.Value.rotation;
				headSprite.Offset = action.Value.offset;
				headSprite.FlipV = action.Value.flipV;
				headSprite.FlipH = action.Value.flipH;

				_main.StartGame();
				break; // Exit the loop once a movement is processed
			}
		}
	}

	internal void UpdateSnake()
	{
		CanMove = true;
		OldData = SnakeData.ToList();
		OldSnakeMoveData = SnakeMoveData.ToList();
		// Create a deep copy of the snake sprites to keep visual bend.
		for (int i = 0; i < SnakeNodes.Count; i++)
		{
			OldSnakeNodes[i] = _main.CloneAnimatedSprite2D((AnimatedSprite2D)SnakeNodes[i]);
		}
		SnakeData[0] += _main.MoveDirection;// Update snake's head position data
		// Update other body segments data
		for (int i = 0; i < SnakeData.Count; i++)
		{
			// Copy frame data for other body segments
			AnimatedSprite2D currentSegment;
			if (i > 1 && i < SnakeNodes.Count - 1)
			{
				currentSegment = (AnimatedSprite2D)SnakeNodes[i];
				var previousSegment = (AnimatedSprite2D)OldSnakeNodes[i - 1];
				currentSegment.Frame = previousSegment.Frame;
				currentSegment.Offset = new Vector2(15, 15);
				currentSegment.FlipH = false;
				currentSegment.FlipV = false;
				currentSegment.Rotation = 0f;
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
			SnakeNodes[i].Position = SnakeData[i] * _main.CellPixelSize + new Vector2I(0, _main.CellPixelSize);
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

	public void MapDirections()
	{
		// Map input actions to movement directions and settings
		_headDirection = new Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)>
		{
			{ "move_down",  (new Vector2(15, -15), 1.5708f, false, false, _main.DownMove) },
			{ "move_up",    (new Vector2(-15, 15), 4.7183f, false, false, _main.UpMove) },
			{ "move_left",  (new Vector2(-15, -15), 3.1416f, true, false, _main.LeftMove) },
			{ "move_right", (new Vector2(15, 15), 0f, false, false, _main.RightMove) }
		};
	}
}
