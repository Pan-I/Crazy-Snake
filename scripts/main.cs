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

	//Game Variables
	private double _score;
	private bool _gameStarted;
	
	//Grid Variables
	private int _boardCellSize = 30;
	private int _cellPixelSize = 30;
	
	//Snake Variables
	private List<Vector2I> _oldData;
	private List<Vector2I> _snakeData;
	private List<Node2D> _snake;
	
	//Egg & Item Variables
	private Vector2I _eggPosition;
	private bool _itemRegen = true;
	private List<Node2D> _items;
	private List<Vector2I> _itemsData;
	private Vector2I _newItemPosition;
	
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
		_snake = new List<Node2D>();

		for (int i = 0; i < 3; i++)
		{
			AddSegment(_startPosition + new Vector2I(0, i));
		}
	}
	private void AddSegment(Vector2I position)
	{
		_snakeData.Add(position);
		var snakeSegment = SnakeSegmentPs.Instantiate<Node2D>();
		snakeSegment.Position = (position * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		AddChild(snakeSegment);
		_snake.Add(snakeSegment);
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
		_oldData = _snakeData.ToList();
		_snakeData[0] += _moveDirection;
		for (int i = 0; i < _snakeData.Count; i++)
		{
			if (i > 0)
			{
				_snakeData[i] = _oldData[i - 1];
			}
			_snake[i].Position = (_snakeData[i] * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		}
		CheckOutOfBound();
		CheckSelfEaten();
		CheckEggEaten();
		CheckItemHit();
	}
	
	private void MoveEgg()
	{
		Random rndm = new Random();
		do
		{
			_itemRegen = false;
			_eggPosition = new Vector2I(rndm.Next(0, _boardCellSize - 1), rndm.Next(3, _boardCellSize - 1));
			for (int i = 0; i < _snakeData.Count; i++)
			{
				if (_eggPosition == _snakeData[i])
				{
					_itemRegen = true;
				}
			}
			for (int i = 0; i < _itemsData.Count; i++)
			{
				if (_eggPosition == _itemsData[i])
				{
					_itemRegen = true;
				}
			}
		} while (_itemRegen);
		
		GetNode<Node2D>("Egg").Position = (_eggPosition * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		_itemRegen = true;
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
			newItem = (Node2D)GetNode<Node2D>("ItemManager/Wall").Duplicate();
			GenerateItem(newItem);
			newItem = (Node2D)GetNode<Node2D>("ItemManager/FreshEgg").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 2 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/RipeEgg").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 3 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/RottenEgg").Duplicate();
			GenerateItem(newItem);
		}

		if (_tally % 4 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/Mushroom").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 5 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/ShinyEgg").Duplicate();
			GenerateItem(newItem);
		}

		if (_tally % 6 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/Skull").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 7 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/DewDrop").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 8 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/LavaEgg").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 10 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/Frog").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 13 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/AlienEgg").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 21 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/IceEgg").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 22 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/Pill").Duplicate();
			GenerateItem(newItem);
		}
		if (_tally % 34 == 0)
		{
			newItem = (Node2D)GetNode<Node2D>("ItemManager/DiscoEgg").Duplicate();
			GenerateItem(newItem);
		}
	}
	
	private void ItemResult(Node2D item)
	{
		if (item.SceneFilePath == GetNode("ItemManager/Wall").SceneFilePath 
			|| item.SceneFilePath == GetNode("ItemManager/LargeWall").SceneFilePath) 
		{
			EndGame();
		}
		if (item.SceneFilePath == GetNode("ItemManager/FreshEgg").SceneFilePath)
		{
			_score += 25;
			AddSegment(_oldData[^1]);
			//_tally++;
			//CheckGenerations();
		}
		if (item.SceneFilePath == GetNode("ItemManager/RipeEgg").SceneFilePath)
		{
			_score *= 1.1;
			AddSegment(_oldData[^1]);
			//_tally++;
			//CheckGenerations();
		}

		if (item.SceneFilePath == GetNode("ItemManager/ShinyEgg").SceneFilePath)
		{
			_score *= 1.25;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == GetNode("ItemManager/AlienEgg").SceneFilePath)
		{
			_score *= 2;
			AddSegment(_oldData[^1]);
		}

		if (item.SceneFilePath == GetNode("ItemManager/DiscoEgg").SceneFilePath)
		{
			_score *= _score;
			AddSegment(_oldData[^1]);
		}
		if (item.SceneFilePath == GetNode("ItemManager/RottenEgg").SceneFilePath)
		{
			_score -= 25;
			AddSegment(_oldData[^1]);
			//_tally++;
			//CheckGenerations();
		}
		if(item.SceneFilePath == GetNode("ItemManager/LavaEgg").SceneFilePath)
		{			
			_score /= 2;
			AddSegment(_oldData[^1]);
			//_tally++;
			//CheckGenerations();
		}
		if (item.SceneFilePath == GetNode("ItemManager/IceEgg").SceneFilePath)
		{
			_score = Math.Sqrt(_score);
			AddSegment(_oldData[^1]);
			//_tally++;
			//CheckGenerations();
		}
		if (item.SceneFilePath == GetNode("ItemManager/Mushroom").SceneFilePath)
		{
			_score += 75;
		}
		if (item.SceneFilePath == GetNode("ItemManager/DewDrop").SceneFilePath)
		{
			_score = Math.Abs(_score);
		}
		if (item.SceneFilePath == GetNode("ItemManager/Pill").SceneFilePath)
		{
			_score = Math.Abs(_score) * (Math.Abs(_score) + Math.Abs(_score));
		}
		if (item.SceneFilePath == GetNode("ItemManager/Skull").SceneFilePath)
		{
			_score -= 9999;
		}
			
	}

	private void GenerateItem(Node2D newItem)
	{
		newItem.Visible = true;
		Random rndm = new Random();
		do
		{
			_itemRegen = false;
			_newItemPosition = new Vector2I(rndm.Next(0, _boardCellSize - 1), rndm.Next(3, _boardCellSize - 1));
			if (_newItemPosition == _eggPosition)
			{
				_itemRegen = true;
			}
			for (int i = 0; i < _snakeData.Count; i++)
			{
				if (_newItemPosition == _snakeData[i])
				{
					_itemRegen = true;
				}
			}
			for (int i = 0; i < _itemsData.Count; i++)
			{
				if (_newItemPosition == _itemsData[i])
				{
					_itemRegen = true;
				}
			}
		} while (_itemRegen);
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
