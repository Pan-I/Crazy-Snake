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
	
	//Egg & Item Variables
	private Vector2I _eggPosition;
	private bool _itemRegen = true;
	private List<Node2D> _items;
	private List<Vector2I> _itemsData;
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
	private Node2D _alienEggNode;
	private Node2D _iceEggNode;
	private Node2D _pillItemNode;
	private Node2D _discoEggNode;
	private Node2D _largeWallNode;
	
	//Movement Variables
	private Vector2I _startPosition = new (14, 16);
	private Vector2I _upMove = new (0, -1);
	private Vector2I _downMove = new (0, 1);
	private Vector2I _leftMove = new (-1, 0);
	private Vector2I _rightMove = new (1, 0);
	private Vector2I _moveDirection;
	private bool _canMove;
	private int _tally;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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
		
		NewGame();
	}


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
		GetNode<CanvasLayer>("GameOverMenu").Visible = false;
		UpdateHudScore();
		_moveDirection = _upMove;
		_canMove = true;
		GenerateSnake();
		MoveEgg();
	}

	private void GenerateSnake()
	{
		_oldData = new List<Vector2I>();
		_snakeData = new List<Vector2I>();
		_oldSnake = new List<Node2D>();
		_snake = new List<Node2D>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}
	private void AddSegment(Vector2I position)
	{
		_snakeData.Add(position);
		var snakeSegment = SnakeSegmentPs.Instantiate<AnimatedSprite2D>();
		snakeSegment.Position = (position * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		if (_snake.Count == 0)
		{
			snakeSegment.Frame = 1;
		}
		AddChild(snakeSegment);
		_snake.Add(snakeSegment);
		_oldSnake.Add(snakeSegment);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MoveSnake();
	}
	
	private void MoveSnake()
	{
		if (_canMove)
		{
			if (Input.IsActionPressed("move_down") && _moveDirection != _upMove)
			{
				_moveDirection = _downMove;
				_canMove = false;
				//_snake[0].Rotation = 90;
				if (!_gameStarted)
				{
					StartGame();
				}
			}
			if (Input.IsActionPressed("move_up") && _moveDirection != _downMove)
			{
				_moveDirection = _upMove;
				_canMove = false;
				if (!_gameStarted)
				{
					StartGame();
				}
			}
			if (Input.IsActionPressed("move_left") && _moveDirection != _rightMove)
			{
				_moveDirection = _leftMove;
				_canMove = false;
				if (!_gameStarted)
				{
					StartGame();
				}
			}
			if (Input.IsActionPressed("move_right") && _moveDirection != _leftMove)
			{
				_moveDirection = _rightMove;
				_canMove = false;
				if (!_gameStarted)
				{
					StartGame();
				}
			}
		}
	}

	private void StartGame()
	{
		_gameStarted = true;
		GetNode<Timer>("MoveTimer").Start();
	}

	private void _on_move_timer_timeout()
	{
		_canMove = true;
		_oldData = _snakeData.ToList(); //Shallow copy, so why does this work? 
		for (int i = 0; i < _snake.Count; i++)
		{
			_oldSnake[i] = CloneAnimatedSprite2D((AnimatedSprite2D)_snake[i]);
		}
		_snakeData[0] += _moveDirection;
		for (int i = 0; i < _snakeData.Count; i++)
		{
			if (i > 0)
			{
				_snakeData[i] = _oldData[i - 1];
			}
			_snake[i].Position = (_snakeData[i] * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
			if (_snakeData.Count > 4 && i == 2)
			{
				ChangeSnakeFrames();
			}
			if (_snakeData.Count > 4 && i > 1 && i < _snake.Count - 1)
			{
				var test = (AnimatedSprite2D)_snake[i];
				var oldTest = (AnimatedSprite2D)_oldSnake[i - 1];
				test.Frame = oldTest.Frame;
			}
		}
		CheckOutOfBound();
		CheckSelfEaten();
		CheckEggEaten();
		CheckItemHit();
	}
	
	private void ChangeSnakeFrames()
	{
		AnimatedSprite2D neck = (AnimatedSprite2D)_snake[1];
		Vector2 headPosition = _snake[0].Position;
		Vector2 neckPosition = _snake[1].Position;
		Vector2 tailPosition = _snake[2].Position;

		// Check if the snake is in a straight line
		if ((headPosition.X == neckPosition.X && neckPosition.X == tailPosition.X) ||
		    (headPosition.Y == neckPosition.Y && neckPosition.Y == tailPosition.Y))
		{
			neck.Frame = 0;
			return;
		}

		// Determine relative positions
		bool isHeadAboveTail = headPosition.Y < tailPosition.Y;
		bool isHeadBelowTail = headPosition.Y > tailPosition.Y;
		bool isHeadRightOfTail = headPosition.X > tailPosition.X;
		bool isHeadLeftOfTail = headPosition.X < tailPosition.X;

		if (isHeadAboveTail)
		{
			if (isHeadRightOfTail)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 2 : 5;
			}
			else if (isHeadLeftOfTail)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 3 : 4;
			}
		}
		else if (isHeadBelowTail)
		{
			if (isHeadRightOfTail)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 4 : 3;
			}
			else if (isHeadLeftOfTail)
			{
				neck.Frame = headPosition.X == neckPosition.X ? 5 : 2;
			}
		}
	}
	
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

	private void MoveEgg()
	{
		_eggPosition = RandomPlacement();
		GetNode<Node2D>("Egg").Position = (_eggPosition * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
	}

	private Vector2I RandomPlacement()
	{
		Vector2I itemPlacement;
		Random rndm = new Random();
		do
		{
			_itemRegen = false;
			itemPlacement = new Vector2I(rndm.Next(0, _boardCellSize - 1), rndm.Next(3, _boardCellSize - 1));
			if (itemPlacement == _eggPosition)
			{
				_itemRegen = true;
			}
			for (int i = 0; i < _snakeData.Count; i++)
			{
				if (itemPlacement == _snakeData[i])
				{
					_itemRegen = true;
				}
			}
			for (int i = 0; i < _itemsData.Count; i++)
			{
				if (itemPlacement == _itemsData[i])
				{
					_itemRegen = true;
				}
			}
		} while (_itemRegen);
		
		_itemRegen = true;
		return itemPlacement;
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
			CheckGenerations();
		}
	}

	private void UpdateHudScore()
	{
		GetNode<CanvasLayer>("Hud").GetNode<Label>("ScoreLabel").Text = $"Score: {_score} ";
	}

	#region Items
	
	private void CheckGenerations()
	{
		Node2D newItem;
		if (_tally % 1 == 0)
		{
			newItem = (Node2D)_wallNode.Duplicate();
			GenerateItem(newItem);
			newItem = (Node2D)_freshEggNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 2 == 0)
		{
			newItem = (Node2D)_ripeEggNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 3 == 0)
		{
			newItem = (Node2D)_rottenEggNode.Duplicate();
			GenerateItem(newItem);
		}

		if (_tally % 4 == 0)
		{
			newItem = (Node2D)_mushroomNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 5 == 0)
		{
			newItem = (Node2D)_shinyEggNode.Duplicate();
			GenerateItem(newItem);
		}

		if (_tally % 6 == 0)
		{

			newItem = (Node2D)_skullNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 7 == 0)
		{
			newItem = (Node2D)_dewDropNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 8 == 0)
		{
			newItem = (Node2D)_lavaEggNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 10 == 0)
		{
			newItem = (Node2D)_frogNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 13 == 0)
		{
			newItem = (Node2D)_alienEggNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 21 == 0)
		{
			newItem = (Node2D)_iceEggNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 22 == 0)
		{
			newItem = (Node2D)_pillItemNode.Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 34 == 0)
		{
			newItem = (Node2D)_discoEggNode.Duplicate();
			GenerateItem(newItem);
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
			_score *= 1.1;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == _shinyEggNode.SceneFilePath)
		{
			_score *= 1.25;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == _alienEggNode.SceneFilePath)
		{
			_score *= 2;
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
			_score = Math.Sqrt(_score);
			AddSegment(_oldData[^1]);
		}
		if (item.SceneFilePath == _mushroomNode.SceneFilePath)
		{
			_score = Math.Pow(_score, 1.1);
		}
		if (item.SceneFilePath == _dewDropNode.SceneFilePath)
		{
			_score = Math.Abs(_score);
		}
		if (item.SceneFilePath == _pillItemNode.SceneFilePath)
		{
			_score = Math.Abs(_score) * (Math.Abs(_score) + Math.Abs(_score));
		}
		if (item.SceneFilePath == _skullNode.SceneFilePath)
		{
			_score -= 9999;
		}
			
	}

	private void GenerateItem(Node2D newItem)
	{
		newItem.Visible = true;
		_newItemPosition = RandomPlacement();
		newItem.Position = (_newItemPosition * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		AddChild(newItem);
		_items.Add(newItem);
		_itemsData.Add(_newItemPosition);
	}

	private void CheckItemHit()
	{
		for (int i = 0; i < _itemsData.Count; i++)
		{
			if (_snakeData[0] == _itemsData[i])
			{
				ItemResult(_items[i]);
				UpdateHudScore();
				_items[i].QueueFree();
				_itemsData.RemoveAt(i);
				_items.RemoveAt(i);
			}
		}
	}

	#endregion

	private void CheckSelfEaten()
	{
		for (int i = 1; i < _snakeData.Count; i++)
		{
			if (_snakeData[0] == _snakeData[i])
			{
				EndGame();
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

	private void EndGame()
	{
		GetTree().Paused = true;
		_gameStarted = false;
		GetNode<Timer>("MoveTimer").Stop();
		GetNode<CanvasLayer>("GameOverMenu").Visible = true;
		string message = $"Game Over!\nScore: {_score}";
		GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("GameOverPanel").GetNode<Label>("GameOverLabel").Text = message;
	}
	
	private void _on_game_over_menu_restart()
	{
		NewGame();
	}
}
