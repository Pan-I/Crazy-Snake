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
using Godot;

namespace Snake.scripts;

public partial class Main : Node
{
	[Export] public PackedScene SnakeSegmentPs {get; set;}
	private Snake Snake { get; }
	private Items Items { get; }

	//Game Variables
	internal double Score;
	private bool _gameStarted;
	private bool _pause;
	
	//Grid Variables
	internal int BoardCellSize = 30;
	internal int CellPixelSize = 30;
	
	//Movement Variables
	internal Vector2I UpMove = new (0, -1);
	internal Vector2I DownMove = new (0, 1);
	internal Vector2I LeftMove = new (-1, 0);
	internal Vector2I RightMove = new (1, 0);
	internal Vector2I MoveDirection;

	public Main()
	{
		Snake = new Snake(this);
		Items = new Items(this, Snake);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// The WaitTime is the amount of seconds between each snake movement. .1-.2 is a good regular gameplay speed; .75 is a good debug speed for animations etc.
		GetNode<Timer>("MoveTimer").WaitTime = 0.333333333;
		Items.LoadItems();
		Items.SetItemRates();
		Snake.MapDirections();
		NewGame();
	}

	#region Game Handling

	private void NewGame()
	{
		GetTree().Paused = false;
		GetTree().CallGroup("snake", "queue_free");
		Score = 0;
		Items.Tally = 0;
		if (Items.ItemNodes != null)
		{
			foreach (Node2D node in Items.ItemNodes)
			{
				node.QueueFree();
			}
		}
		if (Items.LargeItemNodes != null)
		{
			foreach (Node2D node in Items.LargeItemNodes)
			{
				node.QueueFree();
			}
		}

		Items.ItemNodes = new List<Node2D>();
		Items.ItemsData = new List<Vector2I>();
		Items.LargeItemNodes = new List<Node2D>();
		Items.LargeItemsData = new List<Vector2I>();
		GetNode<CanvasLayer>("GameOverMenu").Visible = false;
		UpdateHudScore();
		MoveDirection = UpMove;
		Snake.CanMove = true;
		Snake.GenerateSnake();
		Items.MoveEgg();
	}

	internal void StartGame()
	{
		if (!_gameStarted)
		{
			_gameStarted = true;
			GetNode<Timer>("MoveTimer").Start();
		}
	}

	internal void EndGame()
	{
		Snake.DeadSnake();
		GetTree().Paused = true;
		_gameStarted = false;
		GetNode<Timer>("MoveTimer").Stop();
		GetNode<CanvasLayer>("GameOverMenu").Visible = true;
		string message = $"Game Over!\nScore: {Score}";
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
				_pause = true;
			}
			else
			{
				GetNode<Timer>("MoveTimer").Start();
				_pause = false;
			}
		}
		if (!_pause)
		{
			Snake.KeyPressSnakeDirection();
		}
	}
	
		
	private void _on_move_timer_timeout()
	{
		Snake.UpdateSnake();
		CheckOutOfBound();
		CheckSelfEaten();
		bool eggEaten =  CheckEggEaten();
		if (eggEaten)
		{
			if (1 + Snake.SnakeData.Count + Items.ItemsData.Count + (Items.LargeItemsData.Count*4) == BoardCellSize)
			{
				EndGame(); // TODO: this doesn't account for if there are edible items left. Small walls might need separating into their own group.
			}
		}
		CheckItemHit();
		CheckLargeItemHit(Snake.SnakeData[0]);
	}

	#endregion Game Handling
	
	#region GUI_Checks_Events
	
	private void UpdateHudScore()
	{
		Score = Math.Round(Score, 0);
		GetNode<CanvasLayer>("Hud").GetNode<Label>("ScoreLabel").Text = $"Score: {Score} ";
	}
	
	private bool CheckEggEaten()
	{
		if (Items.EggPosition != Snake.SnakeData[0]) return false;
		Score += 5;
		Items.Tally++;
		UpdateHudScore();
		Snake.AddSegment(Snake.OldData[^1]);
		Items.MoveEgg();
		Items.GenerateFromItemLookup();
		return true;
	}

	private void CheckItemHit()
	{
		for (int i = 0; i < Items.ItemsData.Count; i++)
		{
			if (Snake.SnakeData[0] == Items.ItemsData[i])
			{
				Items.ItemResult(Items.ItemNodes[i]);
				UpdateHudScore();
				if (Items.ItemNodes[i].SceneFilePath != Items.WallNode.SceneFilePath)
				{
					Items.ItemNodes[i].QueueFree();
					Items.ItemsData.RemoveAt(i);
					Items.ItemNodes.RemoveAt(i);
				}
			}
		}
	}
	
	internal bool CheckLargeItemHit(Vector2I position)
	
	{
		bool hit = false;
		for (int i = 0; i < Items.LargeItemsData.Count; i++)
		{
			Vector2I q2 = new Vector2I(x: Items.LargeItemsData[i].X, y: Items.LargeItemsData[i].Y + 1);
			Vector2I q3 = new Vector2I(x: Items.LargeItemsData[i].X + 1, y: Items.LargeItemsData[i].Y + 1);
			Vector2I q4 = new Vector2I(x: Items.LargeItemsData[i].X + 1, y: Items.LargeItemsData[i].Y);
			
			if (position == Items.LargeItemsData[i] || position == q2 || position == q3 || position == q4 )
			{
				hit = true;
			}
		}

		return hit;
	}

	private void CheckSelfEaten()
	{
		for (int i = 1; i < Snake.SnakeData.Count; i++)
		{
			if (Snake.SnakeData[0] == Snake.SnakeData[i])
			{
				if (Snake.SnakeNodes.Count > 5 && i > Snake.SnakeNodes.Count - 3) //Snake can pass over last two segments of tail just fine (if tail is fully developed).
				{
					return;
				}
				
				EndGame();
			}
		}
	}

	private void CheckOutOfBound()
	{
		if (Snake.SnakeData[0].X < 0 || Snake.SnakeData[0].X > BoardCellSize - 1 || Snake.SnakeData[0].Y < 1 || Snake.SnakeData[0].Y > BoardCellSize)
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
	#endregion Helper Methods
}
