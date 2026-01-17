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
using Godot;
using Snake.scripts.Domain;

namespace Snake.scripts;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Category", "WarningCode")]
public partial class Main : Node
{
	#region Exports, Fields, Objects, and Properties
	[Export] public PackedScene SnakeSegmentPs {get; set;}
	[Signal] public delegate void HudFlashRequestedEventHandler(int type);
	private SnakeManager Snake { get; }
	private ItemManager Items { get; }
	private Domain.BoardManager Board { get; }
	private Domain.ScoreManager Score { get; }
	private Domain.HealthManager Health { get; }
	private Domain.TimeManager Time { get; }
	// ReSharper disable once InconsistentNaming
	private Domain.UiManager UI { get; }

	private bool _gameStarted;
	private bool _pause;
	private Vector2I _moveDirection;
	#endregion

	#region GameController Setup
	
	public Main()
	{
		Snake = new SnakeManager();
		Items = new ItemManager(Snake);
		Board = new Domain.BoardManager();
		Score = new Domain.ScoreManager();
		Health = new Domain.HealthManager();
		Time = new Domain.TimeManager();
		UI = new Domain.UiManager();
	}

	public override void _Ready()
	{
		InitializeManagers();
		ConnectSignals();
		NewGame();
	}

	private void InitializeManagers()
	{
		Time.Initialize(
			GetNode<Timer>("MoveTimer"), 
			GetNode<Timer>("HudFlashTimer"), 
			GetNode<Timer>("HealthTimer")
		);
		
		UI.Initialize(
			GetNode<CanvasLayer>("Hud"), 
			GetNode<CanvasLayer>("GameOverMenu"), 
			GetNode<AnimatedSprite2D>("Background")
		);

		Board.Initialize(GetNode<AnimatedSprite2D>("Background").Position);

		Snake.SnakeSegmentPs = SnakeSegmentPs;
		Snake.CellPixelSize = Board.CellPixelSize;
		Snake.MapDirections();

		Items.CellPixelSize = Board.CellPixelSize;
		Items.BoardCellSize = Board.BoardCellSize;
		Items.PlacementOffset = new Vector2I(Board.CellPixelSize/2, Board.CellPixelSize/2);
		Items.EggNode = GetNode<Node2D>("Egg");
		Items.LoadItems(GetNode("ItemManager"));
		Items.SetItemRates();
	}

	private void ConnectSignals()
	{
		// Main
		this.HudFlashRequested += type =>
		{
			UI.HudFlash(type);
			Time.StartHudFlashTimer();
		};
		
		// Snake
		Snake.SegmentAdded += (node) => AddChild(node);
		Snake.SegmentRemoved += (node) => node.QueueFree();
		Snake.RequestStartGame += StartGame;
		Snake.DirectionChanged += (direction, action) =>
		{
			_moveDirection = direction;
			Snake.SnakeMoveData[0] = action;
			Snake.MoveDirection = direction;
		};

		// Items
		Items.ItemSpawned += (node) => AddChild(node);
		Items.HealthDeductedRequested += () => Health.DeductHealth();
		Items.ScoreChanged += (amount, isDelta) =>
		{
			if (isDelta) Score.AddScore(amount);
			else Score.SetScore(amount);
		};
		Items.ComboPointsXChanged += (amount, isDelta) =>
		{
			if (isDelta) Score.ComboPointsX += amount;
			else Score.ComboPointsX = amount;
		};
		Items.ComboPointsYChanged += (amount, isDelta) =>
		{
			if (isDelta) Score.ComboPointsY += amount;
			else Score.ComboPointsY = amount;
		};
		Items.ComboStarted += () => Score.StartCombo();
		Items.ComboEnded += () => Score.EndCombo();
		Items.ComboCancelled += () => Score.CancelCombo();
		Items.GameOverRequested += EndGame;
		Items.HudFlashRequested += type =>
		{
			EmitSignal(SignalName.HudFlashRequested, type);
		};

		// Score
		Score.ScoreChanged += (s, cx, cy, inCombo) => {
			UI.UpdateScore(s, cx, cy, inCombo);
			Items.CurrentScore = s;
			Items.CurrentComboPointsX = cx;
			Items.CurrentComboPointsY = cy;
		};

		// Health
		Health.HealthSegmentAdded += (node) => GetNode<CanvasLayer>("Hud").AddChild(node);
		Health.HealthSegmentRemoved += (node) => node.QueueFree();
		Health.HealthDeducted += () => {
			EmitSignal(SignalName.HudFlashRequested, 1);
			UI.UpdateWindowDressing(Health.Lives <= 2);
			Time.StartHealthTimer();
			Score.EndCombo();
		};
		Health.GameOverRequested += EndGame;
	}

	#endregion
	
	public override void _Process(double delta)
	{
		Items.UpdateItemPulse(delta, Score.IsInCombo);
		Items.UpdateEggPulse(delta, Score.IsInCombo);
		if (Input.IsActionPressed("escape"))
		{
			GetTree().Quit();
		}
		if (Input.IsActionJustPressed("pause"))
		{
			_pause = !_pause;
			if (_pause) Time.StopMoveTimer();
			else Time.StartMoveTimer();
		}
		if (!_pause)
		{
			Snake.KeyPressSnakeDirection();
		} 
	}
	
	#region Gameplay Logic
	private void NewGame()
	{
		Time.SetMoveTimerWaitTime(0.5);
		GetTree().Paused = false;
		GetTree().CallGroup("snake", "queue_free");
		
		Score.Reset();
		UI.HideGameOver();
		UI.SetBackgroundVisible(false);
		UI.UpdateWindowDressing(false);
		
		_moveDirection = SnakeManager.UpMove;
		Snake.MoveDirection = _moveDirection;
		Snake.GenerateSnake();
		
		Items.Dispose();
		Health.Initialize(
			GetNode<CanvasLayer>("Hud").GetNode<Panel>("ScorePanel").GetNode<Panel>("HealthPanel").Position,
			Board.CellPixelSize,
			SnakeSegmentPs
		);
	}

	private void StartGame()
	{
		if (_gameStarted) return;
		
		_gameStarted = true;
		Time.StartMoveTimer();
	}

	private void EndGame()
	{
		UI.UpdateWindowDressing(false, true);
		Time.StartHealthTimer();
		
		// Remove health segments
		foreach (var node in Health.HealthNodes) {
			node.QueueFree();
		}
		Health.Reset();

		UI.HudFlash(1);
		Score.EndCombo();
		Snake.DeadSnake();
		Snake.RemoveHead();

		GetTree().Paused = true;
		_gameStarted = false;
		Time.StopMoveTimer();
		UI.ShowGameOver(Score.Score);
	}

	private void CheckSelfEaten()
	{
		for (int i = 1; i < Snake.SnakeData.Count; i++)
		{
			if (Snake.SnakeData[0] != Snake.SnakeData[i]) continue;
			if (Snake.SnakeNodes.Count > 5 && i > Snake.SnakeNodes.Count - 3) return;

			Snake.RemoveTailFrom(i);
			Health.DeductHealth();
			return;
		}
	}

	private bool CheckEggEaten()
	{
		if (Items.EggPosition != Snake.SnakeData[0]) return false;
		
		Items.EggEaten();
		Snake.AddSegment(Snake.OldData[^1]);
		UI.SetBackgroundVisible(true);
		Score.ComboTally++;

		UI.UpdateComboMeter(Score.ComboTally, Score.IsInCombo);
		EmitSignal(SignalName.HudFlashRequested, 0);

		if (!Score.IsInCombo)
		{
			Score.AddScore(1);
			UpdateBackgroundOnEggEaten();
			Time.SetMoveTimerWaitTime(Snake.SnakeNodes.Count < 6 ? 0.25 : 0.125);
			if (Score.ComboTally >= 7) Score.StartCombo();
		}
		else
		{
			UpdateBackgroundInCombo();
			Score.ComboPointsX += 2;
		}

		return true;
	}

	private void UpdateBackgroundOnEggEaten()
	{
		if (Score.ComboTally > 3)
		{
			UI.SetBackgroundFrame(Score.ComboTally % 2 == 0 ? 1 : 2);
		}
	}

	private void UpdateBackgroundInCombo()
	{
		if (Score.ComboTally % 2 == 0) UI.SetBackgroundFrame(4);
		else if (Score.ComboTally % 3 == 0) UI.SetBackgroundFrame(5);
		else UI.SetBackgroundFrame(3);
	}

	private void CheckItemHit()
	{
		for (int i = 0; i < Items.ItemsData.Count; i++)
		{
			if (Snake.SnakeData[0] == Items.ItemsData[i])
			{
				Items.ItemResult(Items.ItemNodes[i], i, Score.IsInCombo);
			}
		}
		for (int i = 0; i < Items.WallsData.Count; i++)
		{
			if (Snake.SnakeData[0] == Items.WallsData[i])
			{
				Items.ItemResult(Items.WallNodes[i], i, Score.IsInCombo);
			}
		}
	}

	private void CheckFullBoard(bool eggEaten)
	{
		if (!eggEaten) return;
		int occupied = 1 + Snake.SnakeData.Count + Items.WallsData.Count + (Items.LargeWallsData.Count * 4);
		if (occupied >= Board.BoardCellSize * Board.BoardCellSize)
		{
			EndGame();
		}
	}
	
	#endregion
	
	#region Input Handling & Signals

	private void _on_hud_flash_timer_timeout()
	{
		UI.ResetHudPanels();
		Time.StopHudFlashTimer();
	}

	private void _on_health_timer_timeout()
	{
		UI.UpdateWindowDressing(Health.Lives <= 2);
		Time.StopHealthTimer();
	}

	private void _on_game_over_menu_restart()
	{
		NewGame();
	}
	
	private void _on_move_timer_timeout()
	{
		Snake.UpdateSnake();
		
		if (Board.IsOutOfBounds(Snake.SnakeData[0]))
		{
			EndGame();
			return;
		}

		CheckSelfEaten();
		bool eggEaten = CheckEggEaten();
		CheckFullBoard(eggEaten);
		CheckItemHit();
		
		if (Items.CheckLargeItemHit(Snake.SnakeData[0]))
		{
			EndGame();
		}
	}
	
	#endregion
}
