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

namespace Snake.scripts;

public class Snake
{
	private readonly Main _main;
	internal List<Vector2I> OldData;
	internal List<Vector2I> SnakeData;
	private List<Node2D> _oldSnakeNodes;
	internal List<Node2D> SnakeNodes;
	internal Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> HeadDirection;
	private List<string> _snakeMoveData;
	private List<string> _oldSnakeMoveData;
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
		_oldSnakeNodes = new List<Node2D>();
		SnakeNodes = new List<Node2D>();
		_snakeMoveData = new List<string>();
		_oldSnakeMoveData = new List<string>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}

	internal void AddSegment(Vector2I position)
	
	{
		SnakeData.Add(position);
		_snakeMoveData.Add("");
		var snakeSegment = _main.SnakeSegmentPs.Instantiate<AnimatedSprite2D>();
		snakeSegment.Position = (position * _main.CellPixelSize) + new Vector2I(0, _main.CellPixelSize);
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
		_oldSnakeNodes.Add(snakeSegment);
	}

	internal void KeyPressSnakeDirection()
	{
		if (!CanMove) return;
		foreach (var action in HeadDirection)
		{
			if (Input.IsActionPressed(action.Key) && _main.MoveDirection != -action.Value.direction)
			{
				_main.MoveDirection = (Vector2I)action.Value.direction;
				_snakeMoveData[0] = action.Key; 
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
		_oldSnakeMoveData = _snakeMoveData.ToList();
		// Create a deep copy of the snake sprites to keep visual bend.
		for (int i = 0; i < SnakeNodes.Count; i++)
		{
			_oldSnakeNodes[i] = _main.CloneAnimatedSprite2D((AnimatedSprite2D)SnakeNodes[i]);
		}
		SnakeData[0] += _main.MoveDirection;// Update snake's head position data
		// Update other body segments data
		for (int i = 0; i < SnakeData.Count; i++)
		{
			// Copy frame data for other body segments
			AnimatedSprite2D currentSegment = null;
			if (i > 1 && i < SnakeNodes.Count - 1)
			{
				currentSegment = (AnimatedSprite2D)SnakeNodes[i];
				var previousSegment = (AnimatedSprite2D)_oldSnakeNodes[i - 1];
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
				_snakeMoveData[i] = _oldSnakeMoveData[i - 1];
			}
			HandleTailSegments(i);
			if ((SnakeNodes.Count > 4 && i > 4))
			{
				BendTail(i);
			}
			if ((SnakeNodes.Count == 5 && i > 3))
			{
				BendTail(i, false);
			}
			// Update the position of the segment sprite
			SnakeNodes[i].Position = (SnakeData[i] * _main.CellPixelSize) + new Vector2I(0, _main.CellPixelSize);
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
			var currentDirection = _snakeMoveData[i];
			foreach (var action in HeadDirection)
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
			var currentDirection = _snakeMoveData[i];
			foreach (var action in HeadDirection)
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
			var currentDirection = _snakeMoveData[i];
			foreach (var action in HeadDirection)
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

	private void BendTail(int i, bool fullTail = true)
	{
		
		AnimatedSprite2D tailBase = fullTail ? (AnimatedSprite2D)SnakeNodes[^3] : (AnimatedSprite2D)SnakeNodes[^2];
		AnimatedSprite2D tailShaft = fullTail ? (AnimatedSprite2D)SnakeNodes[^2] : (AnimatedSprite2D)SnakeNodes[^1];
		AnimatedSprite2D tailTip = fullTail ? (AnimatedSprite2D)SnakeNodes[^1] : null;
		string tailBaseMovement = fullTail ? _snakeMoveData[^3] : _snakeMoveData[^2];
		string tailShaftMovement = fullTail ? _snakeMoveData[^2] : _snakeMoveData[^1];
		string tailTipMovement = fullTail ? _snakeMoveData[^1] : null;
		
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
}
