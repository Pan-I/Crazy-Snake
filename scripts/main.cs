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
using System;
using System.Collections.Generic;
using System.Linq;

public partial class main : Node
{
	[Export] public PackedScene SnakeSegmentScene {get; set;}

	//Game Variables
	private int _score;
	private bool _gameStarted;
	
	//Grid Variables
	private int _boardCellSize = 30;
	private int _cellPixelSize = 30;
	
	//Snake Variables
	private List<Vector2I> _oldData;
	private List<Vector2I> _snakeData;
	private List<Node2D> _snake;
	
	//Food Variables
	private Vector2I _foodPosition;
	private bool _foodRegen = true;
	
	//Movement Variables
	private Vector2I _startPosition = new (14, 16);
	private Vector2I _upMove = new (0, -1);
	private Vector2I _downMove = new (0, 1);
	private Vector2I _leftMove = new (-1, 0);
	private Vector2I _rightMove = new (1, 0);
	private Vector2I _moveDirection;
	private bool _canMove;
	
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
		GetNode<CanvasLayer>("GameOverMenu").Visible = false;
		GetNode<CanvasLayer>("Hud").GetNode<Label>("ScoreLabel").Text = $"Score: {_score.ToString()} ";
		_moveDirection = _upMove;
		_canMove = true;
		GenerateSnake();
		MoveFood();
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
		var snakeSegment = SnakeSegmentScene.Instantiate<Node2D>();
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
		//var test = Array.Empty<Vector2I>();
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
		CheckFoodEaten();
	}

	private void MoveFood()
	{
		Random rndm = new Random();
		do
		{
			_foodRegen = false;
			_foodPosition = new Vector2I(rndm.Next(0, _boardCellSize - 1), rndm.Next(3, _boardCellSize - 1));
			for (int i = 0; i < _snakeData.Count; i++)
			{
				if (_foodPosition == _snakeData[i])
				{
					_foodRegen = true;
				}
			}
		} while (_foodRegen);
		
		GetNode<Node2D>("Food").Position = (_foodPosition * _cellPixelSize) + new Vector2I(0, _cellPixelSize);
		_foodRegen = true;
	}

	private void CheckFoodEaten()
	{
		if (_foodPosition == _snakeData[0])
		{
			_score += 5;
			GetNode<CanvasLayer>("Hud").GetNode<Label>("ScoreLabel").Text = $"Score: {_score.ToString()} ";
			AddSegment(_oldData[^1]);
			MoveFood();
		}
	}

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
			_snakeData[0].Y > _boardCellSize - 1)
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




