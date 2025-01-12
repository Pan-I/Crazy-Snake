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
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Snake.scripts;

public partial class Main : Node
{
	[Export] public PackedScene SnakeSegmentPs {get; set;}
	private Node2D _wallNode;
	
	//Game Variables
	private double _score;
	private bool _gameStarted;
	
	//Grid Variables
	private int _boardCellSize = 30;
	private int _cellPixelSize = 30;
	
	//Snake Variables
	private List<Vector2I> _oldData;
	private List<Vector2I> _snakeData;
	private List<Node2D> _oldSnake;
	private List<Node2D> _snake;
	private Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)> _headDirection;
	private List<string> _snakeMoveData;
	private List<string> _oldSnakeMoveData;
	
	//Movement Variables
	private Vector2I _startPosition = new (14, 16);
	private bool _canMove;
	private Vector2I _upMove = new (0, -1);
	private Vector2I _downMove = new (0, 1);
	private Vector2I _leftMove = new (-1, 0);
	private Vector2I _rightMove = new (1, 0);
	private Vector2I _moveDirection;
	
	//Egg & Item Variables
	private Vector2I _eggPosition;
	private bool _itemRegen = true;
	private int _tally;
	private Dictionary<int, List<Node2D>> _itemRates;
	private List<Node2D> _items;
	private List<Vector2I> _itemsData;
	private List<Vector2I> _largeItemsData;
	private Vector2I _newItemPosition;
	private Node2D _freshEggNode;
	private Node2D _ripeEggNode;
	private Node2D _rottenEggNode;
	private Node2D _mushroomNode;
	private Node2D _shinyEggNode;
	private Node2D _skullNode;
	private Node2D _dewDropNode;
	private Node2D _lavaEggNode;
	private Node2D _frogNode;
	private Node2D _largeWallNode;
	private Node2D _alienEggNode;
	private Node2D _iceEggNode;
	private Node2D _pillItemNode;
	private Node2D _discoEggNode;

	private bool _pause;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// The WaitTime is the amount of seconds between each snake movement. .1-.2 is a good regular gameplay speed; .75 is a good debug speed for animations etc.
		GetNode<Timer>("MoveTimer").WaitTime = 0.75;
		
		_wallNode = GetNode<Node2D>("ItemManager/Wall");
		_freshEggNode = GetNode<Node2D>("ItemManager/FreshEgg");
		_ripeEggNode = GetNode<Node2D>("ItemManager/RipeEgg");
		_rottenEggNode = GetNode<Node2D>("ItemManager/RottenEgg");
		_mushroomNode = GetNode<Node2D>("ItemManager/Mushroom");
		_shinyEggNode = GetNode<Node2D>("ItemManager/ShinyEgg");
		_skullNode = GetNode<Node2D>("ItemManager/Skull");
		_dewDropNode = GetNode<Node2D>("ItemManager/DewDrop");
		_lavaEggNode = GetNode<Node2D>("ItemManager/LavaEgg");
		_frogNode = GetNode<Node2D>("ItemManager/Frog");
		_alienEggNode = GetNode<Node2D>("ItemManager/AlienEgg");
		_iceEggNode = GetNode<Node2D>("ItemManager/IceEgg");
		_pillItemNode = GetNode<Node2D>("ItemManager/Pill");
		_discoEggNode = GetNode<Node2D>("ItemManager/DiscoEgg");
		_largeWallNode = GetNode<Node2D>("ItemManager/LargeWall");
		
		// Map input actions to movement directions and settings
		_headDirection = new Dictionary<string, (Vector2 offset, float rotation, bool flipV, bool flipH, Vector2 direction)>
		{
			{ "move_down",  (new Vector2(15, -15), 1.5708f, false, false, _downMove) },
			{ "move_up",    (new Vector2(-15, 15), 4.7183f, false, false, _upMove) },
			{ "move_left",  (new Vector2(-16, -15), 3.1416f, true, false, _leftMove) },
			{ "move_right", (new Vector2(15, 15), 0f, false, false, _rightMove) }
		};
		
		_itemRates = new Dictionary<int, List<Node2D>>
		{
			{ 1, new List<Node2D> { _wallNode, _freshEggNode } },
			{ 2, new List<Node2D> { _ripeEggNode } },
			{ 3, new List<Node2D> { _rottenEggNode } },
			{ 4, new List<Node2D> { _mushroomNode } },
			{ 5, new List<Node2D> { _shinyEggNode } },
			{ 6, new List<Node2D> { _skullNode } },
			{ 7, new List<Node2D> { _dewDropNode } },
			{ 8, new List<Node2D> { _lavaEggNode } },
			{ 10, new List<Node2D> { _frogNode } },
			{ 12, new List<Node2D> { _largeWallNode } },
			{ 13, new List<Node2D> { _alienEggNode } },
			{ 21, new List<Node2D> { _iceEggNode } },
			{ 22, new List<Node2D> { _pillItemNode } },
			{ 34, new List<Node2D> { _discoEggNode } }
		};
		_pause = false;
		NewGame();
	}

	#region Game Handling
	
	public void NewGame()
	{
		GetTree().Paused = false;
		GetTree().CallGroup("snake", "queue_free");
		_score = 0;
		_tally = 0;
		if (_items != null)
		{
			foreach (Node2D node in _items)
			{
				node.QueueFree();
			}
		}
		_items = new List<Node2D>();
		_itemsData = new List<Vector2I>();
		_largeItemsData = new List<Vector2I>();
		GetNode<CanvasLayer>("GameOverMenu").Visible = false;
		UpdateHudScore();
		_moveDirection = _upMove;
		_canMove = true;
		GenerateSnake();
		MoveEgg();
	}
	
	private void StartGame()
	{
		_gameStarted = true;
		GetNode<Timer>("MoveTimer").Start();
	}
	
	private void EndGame()
	{
		GetTree().Paused = true;
		_gameStarted = false;
		GetNode<Timer>("MoveTimer").Stop();
		GetNode<CanvasLayer>("GameOverMenu").Visible = true;
		string message = $"Game Over!\nScore: {_score}";
		GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("GameOverPanel").GetNode<Label>("GameOverLabel").Text = message;
		//var test = GetNode<AnimatedSprite2D>("Background");
		//test.Frame = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionPressed("pause"))
		{
			if (!_pause)
			{
				GetNode<Timer>("MoveTimer").Stop();
				_canMove = false;
				_pause = true;
			}
			else
			{
				GetNode<Timer>("MoveTimer").Start();
				_canMove = true;
				_pause = false;
			}

		}
		MoveSnake();
	}
	
		
	private void _on_move_timer_timeout()
	{
		_canMove = true;
		_oldData = _snakeData.ToList();
		_oldSnakeMoveData = _snakeMoveData.ToList();
		// Create a deep copy of the snake sprites to keep visual bend.
		for (int i = 0; i < _snake.Count; i++)
		{
			_oldSnake[i] = CloneAnimatedSprite2D((AnimatedSprite2D)_snake[i]);
		}
		_snakeData[0] += _moveDirection;// Update snake's head position data
		// Update other body segments data
		AnimatedSprite2D currentSegment;
		for (int i = 0; i < _snakeData.Count; i++)
		{
			// Copy frame data for other body segments
			currentSegment = null;
			if (i > 1 && i < _snake.Count - 1)
			{
				currentSegment = (AnimatedSprite2D)_snake[i];
				var previousSegment = (AnimatedSprite2D)_oldSnake[i - 1];
				currentSegment.Frame = previousSegment.Frame;
				currentSegment.Offset = new Vector2(15, 15);
				currentSegment.FlipH = false;
				currentSegment.FlipV = false;
				currentSegment.Rotation = 0f;
			}
			if (i > 0)
			{
				currentSegment = (AnimatedSprite2D)_snake[i];
				currentSegment.Visible = true;
				_snakeData[i] = _oldData[i - 1];
				_snakeMoveData[i] = _oldSnakeMoveData[i - 1];
			}
			HandleTailSegments(currentSegment, i);
			if ((_snake.Count > 4 && i > 4))
			{
				BendTail(i);
			}
			// Update the position of the segment sprite
			_snake[i].Position = (_snakeData[i] * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
			// Handle new neck bending for the second segment
			if (i == 2)
			{
				BendNeckSegment();
			}
		}
		
		CheckOutOfBound();
		CheckSelfEaten();
		CheckEggEaten();
		CheckItemHit();
		CheckLargeItemHit();
	}
	
	#endregion Game Handling

	
	#region Snake Methods
	
	private void GenerateSnake()
	{
		_oldData = new List<Vector2I>();
		_snakeData = new List<Vector2I>();
		_oldSnake = new List<Node2D>();
		_snake = new List<Node2D>();
		_snakeMoveData = new List<string>();
		_oldSnakeMoveData = new List<string>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}
	private void AddSegment(Vector2I position)
	
	{
		_snakeData.Add(position);
		_snakeMoveData.Add("");
		var snakeSegment = SnakeSegmentPs.Instantiate<AnimatedSprite2D>();
		snakeSegment.Position = (position * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		if (_snake.Count == 0)
		{
			snakeSegment.Frame = 1;
			snakeSegment.Offset = new Vector2(-15, 15);
			snakeSegment.Rotation = 4.7183f;
		}
		if (_snake.Count > 3)
		{
			snakeSegment.Visible = false;
		}
		AddChild(snakeSegment);
		_snake.Add(snakeSegment);
		_oldSnake.Add(snakeSegment);
	}
	private void MoveSnake()
	{
		if (!_canMove) return;
		foreach (var action in _headDirection)
		{
			if (Input.IsActionPressed(action.Key) && _moveDirection != -action.Value.direction)
			{
				_moveDirection = (Vector2I)action.Value.direction;
				_snakeMoveData[0] = action.Key; 
				_canMove = false;

				var headSprite = (AnimatedSprite2D)_snake[0];
				headSprite.Rotation = action.Value.rotation;
				headSprite.Offset = action.Value.offset;
				headSprite.FlipV = action.Value.flipV;
				headSprite.FlipH = action.Value.flipH;

				if (!_gameStarted)
				{
					StartGame();
				}
				break; // Exit the loop once a movement is processed
			}
		}
	}
	private void HandleTailSegments(AnimatedSprite2D currentSegment, int i)
	{
		if (i == _snake.Count - 3 && _snake.Count > 5 || (i > 2 && _snake.Count < 5) || (i == 3 && _snake.Count == 5))
		{
			currentSegment = (AnimatedSprite2D)_snake[i];
			currentSegment.Frame = 6;
			var currentDirection = _snakeMoveData[i];
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
		else if (i == _snake.Count - 2 && _snake.Count > 5 || (i > 3 && _snake.Count < 5) || (i == 4 && _snake.Count == 5))
		{
			currentSegment = (AnimatedSprite2D)_snake[i];
			currentSegment.Frame = 7;
			var currentDirection = _snakeMoveData[i];
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
		else if (i == _snake.Count - 1 && _snake.Count > 5)
		{
			currentSegment = (AnimatedSprite2D)_snake[i];
			currentSegment.Frame = 9;
			var currentDirection = _snakeMoveData[i];
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
	
	private void BendTail(int i)
	{
		var valid = false;
		AnimatedSprite2D tailBase = (AnimatedSprite2D)_snake[^3];
		AnimatedSprite2D tailShaft = (AnimatedSprite2D)_snake[^2];
		AnimatedSprite2D tailTip = (AnimatedSprite2D)_snake[^1];
		Vector2 tailBasePosition = _snake[^3].Position;
		Vector2 tailShaftPosition = _snake[^2].Position;
		Vector2 tailTipPosition = _snake[^1].Position;

		// Check if the tail is in a straight line
		if ((tailBasePosition.X == tailShaftPosition.X && tailShaftPosition.X == tailTipPosition.X) ||
			(tailBasePosition.Y == tailShaftPosition.Y && tailShaftPosition.Y == tailBasePosition.Y))
		{
			tailBase.Frame = 6;
			tailShaft.Frame = 7;
			tailTip.Frame = 9;
		}
		// Determine relative positions
		bool isBaseAboveTip = tailBasePosition.Y < tailTipPosition.Y;
		bool isBaseBelowTip = tailBasePosition.Y > tailTipPosition.Y;
		bool isBaseRightOfTip = tailBasePosition.X > tailTipPosition.X;
		bool isBaseLeftOfTip = tailBasePosition.X < tailTipPosition.X;

		if (isBaseAboveTip)
		{
			if (isBaseRightOfTip)
			{
				tailShaft.Frame = 8;
				tailTip.Frame = 10;
			}
			else if (isBaseLeftOfTip)
			{
				tailShaft.Frame = 8;
				tailTip.Frame = 10;
			}
		}
		else if (isBaseBelowTip)
		{
			if (isBaseRightOfTip)
			{
				tailShaft.Frame = 8;
				tailTip.Frame = 10;
			}
			else if (isBaseLeftOfTip)
			{
				tailShaft.Frame = 8;
				tailTip.Frame = 10;
			}
		}
	}
	
	private void BendNeckSegment()
	{
		AnimatedSprite2D neck = (AnimatedSprite2D)_snake[1];
		Vector2 headPosition = _snake[0].Position;
		Vector2 neckPosition = _snake[1].Position;
		Vector2 bodyPosition = _snake[2].Position;

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
				neck.Frame = headPosition.X == neckPosition.X ? 2 : 5;
			}
			else if (isHeadLeftOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 3 : 4;
			}
		}
		else if (isHeadBelowBody)
		{
			if (isHeadRightOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 4 : 3;
			}
			else if (isHeadLeftOfBody)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 5 : 2;
			}
		}
	}
	
	#endregion Snake Methods

	#region Items
	
	private void MoveEgg()
	{
		_eggPosition = RandomPlacement();
		GetNode<Node2D>("Egg").Position = (_eggPosition * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
	}

	private Vector2I RandomPlacement()
	{
		//TODO: possible parameter for introducing larger objects when being placed to avoid placing on top of other items.
		//Large walls have been spotted spawning on snake segments, and other items (https://github.com/users/Pan-I/projects/6/views/3?pane=issue&itemId=93374486&issue=Pan-I%7CCrazy-Snake%7C9)
		Random rndm = new Random();
		HashSet<Vector2I> occupiedPositions = new HashSet<Vector2I>(_snakeData);
		occupiedPositions.Add(_eggPosition);
		occupiedPositions.UnionWith(_itemsData);
		occupiedPositions.UnionWith(_largeItemsData); //TODO: Needs a way to fill the other cells. Otherwise items may place in non-origin cells of larger items.
		//
		
		Vector2I itemPlacement;
		do
		{
			itemPlacement = new Vector2I(rndm.Next(0, _boardCellSize - 1), rndm.Next(3, _boardCellSize - 1));
		} while (occupiedPositions. Count < 899 && //TODO: this 899 limit doesn't account for large items either.
				 (occupiedPositions.Contains(itemPlacement) || //Don't place on an occupied position.
				  IsWithinRadius(itemPlacement, _snakeData[0], 3) || //Don't place too close to snake head.
				  !IsWithinRadius(itemPlacement, _snakeData[^1], 7) //Place within 7 cells of the tail. Temporary rule? I kind of like it.
				 ));
		// Helper function to check if a position is within a given radius
		bool IsWithinRadius(Vector2I position, Vector2I center, int radius)
		{
			int dx = Math.Abs(position.X - center.X);
			int dy = Math.Abs(position.Y - center.Y);
			return dx <= radius && dy <= radius;
		}
		
		return itemPlacement;
	}
	
	private void GenerateFromItemLookup()
	{
		foreach (var rule in _itemRates)
		{
			if (_tally % rule.Key == 0)
			{
				foreach (var itemNode in rule.Value)
				{
					var newItem = (Node2D)itemNode.Duplicate();
					SpawnItem(newItem);
				}
			}
		}
	}
	
	private void SpawnItem(Node2D newItem)
	{
		newItem.Visible = true;
		_newItemPosition = RandomPlacement();
		newItem.Position = (_newItemPosition * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		AddChild(newItem);
		_items.Add(newItem);
		if (newItem.SceneFilePath == _largeWallNode.SceneFilePath)
		{
			_largeItemsData.Add(_newItemPosition);
		}
		else
		{
			_itemsData.Add(_newItemPosition);
		}
	}
	
	private void ItemResult(Node2D item)
	{
		if (item.SceneFilePath == _wallNode.SceneFilePath 
			|| item.SceneFilePath == _largeWallNode.SceneFilePath) 
		{
			EndGame();
		}
		if (item.SceneFilePath == _freshEggNode.SceneFilePath)
			
		{
			_score += 25;
			AddSegment(_oldData[^1]);
		}
		if (item.SceneFilePath == _ripeEggNode.SceneFilePath)
		{
			_score *= 1.5;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == _shinyEggNode.SceneFilePath)
		{
			_score *= 2;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == _alienEggNode.SceneFilePath)
		{
			_score *= 5;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == _discoEggNode.SceneFilePath)
		{
			_score *= _score;
			AddSegment(_oldData[^1]);
		}
		if (item.SceneFilePath == _rottenEggNode.SceneFilePath)
		{
			_score -= 75;
			AddSegment(_oldData[^1]);
		}
		if(item.SceneFilePath == _lavaEggNode.SceneFilePath)
		{			
			_score /= 2;
			AddSegment(_oldData[^1]);
		}
		if (item.SceneFilePath == _iceEggNode.SceneFilePath)
		{
			_score = Math.Sqrt(Math.Abs(_score));
			AddSegment(_oldData[^1]);
		}
		if (item.SceneFilePath == _mushroomNode.SceneFilePath)
		{
			_score = Math.Pow(Math.Abs(_score), 1.05);
		}
		if (item.SceneFilePath == _dewDropNode.SceneFilePath)
		{
			_score = Math.Abs(_score);
		}
		if (item.SceneFilePath == _pillItemNode.SceneFilePath)
		{
			_score = Math.Pow(Math.Abs(_score), 1.5);
		}
		if (item.SceneFilePath == _skullNode.SceneFilePath)
		{
			Random rnd = new Random();
			int result;
			do
			{
				result = rnd.Next(-1, 2);
			} while (result == 0 || result== 2); //results can only be 1 or -1
			if (result == 1)
			{
				_score = -9999;
			}
			else if (result == -1)
			{
				_score -= 9999;
			}
			else //shouldn't hit
			{
				_score = 0;
			}
		}
			
	}
	
	#endregion Items

	#region GUI_Checks_Events
	
	private void UpdateHudScore()
	{
		_score = Math.Round(_score, 0);
		GetNode<CanvasLayer>("Hud").GetNode<Label>("ScoreLabel").Text = $"Score: {_score} ";
	}
	
	private void CheckEggEaten()
	{
		if (_eggPosition == _snakeData[0])
		{
			_score += 5;
			_tally++;
			UpdateHudScore();
			AddSegment(_oldData[^1]);
			MoveEgg();
			GenerateFromItemLookup();
		}
	}

	private void CheckItemHit()
	{
		for (int i = 0; i < _itemsData.Count; i++)
		{
			if (_snakeData[0] == _itemsData[i])
			{
				ItemResult(_items[i]);
				UpdateHudScore();
				if (_items[i].SceneFilePath != _wallNode.SceneFilePath)
				{
					_items[i].QueueFree();
					_itemsData.RemoveAt(i);
					_items.RemoveAt(i);
				}
			}
		}
	}
	
	private void CheckLargeItemHit()
	{
		for (int i = 0; i < _largeItemsData.Count; i++)
		{
			Vector2I q2 = new Vector2I(x: _largeItemsData[i].X, y: _largeItemsData[i].Y + 1);
			Vector2I q3 = new Vector2I(x: _largeItemsData[i].X + 1, y: _largeItemsData[i].Y + 1);
			Vector2I q4 = new Vector2I(x: _largeItemsData[i].X + 1, y: _largeItemsData[i].Y);
			
			if (_snakeData[0] == _largeItemsData[i] ||
				_snakeData[0] == q2 ||
				_snakeData[0] == q3 ||
				_snakeData[0] == q4 )
			{
				EndGame();
			}
		}
	}

	private void CheckSelfEaten()
	{
		for (int i = 1; i < _snakeData.Count; i++)
		{
			if (_snakeData[0] == _snakeData[i])
			{
				if (_snake.Count > 5 && i > _snake.Count - 3) //Snake can pass over last two segments of tail just fine (if tail is fully developed).
				{
					return;
				}
				else
				{
					EndGame();
				}
			}
		}
	}

	private void CheckOutOfBound()
	{
		if (_snakeData[0].X < 0 || _snakeData[0].X > _boardCellSize - 1 || _snakeData[0].Y < 1 ||
			_snakeData[0].Y > _boardCellSize)
		{
			EndGame();
		}
	}
	
	private void _on_game_over_menu_restart()
	{
		NewGame();
	}
	
	#endregion GUI_Checks_Events
	
	#region Helper Methods
	private AnimatedSprite2D CloneAnimatedSprite2D(AnimatedSprite2D original)
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
	#endregion Helper Methods
}
