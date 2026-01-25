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
using BoardManager = Snake.Scripts.Domain.Managers.BoardManager;
using HealthManager = Snake.Scripts.Domain.Managers.HealthManager;
using ScoreManager = Snake.Scripts.Domain.Managers.ScoreManager;
using SnakeManager = Snake.Scripts.Domain.Managers.SnakeManager;
using TimeManager = Snake.Scripts.Domain.Managers.TimeManager;
using ItemManager = Snake.Scripts.Domain.Managers.ItemManager;
using AudioManager = Snake.Scripts.Domain.Managers.AudioManager;
using UiManager = Snake.Scripts.Domain.Managers.UiManager;
using Timer = Godot.Timer;

namespace Snake.Scripts;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Category", "WarningCode")]

/// <summary>
/// Represents the main game controller and entry point for the game.
/// Receives and processes user input, and signals from managers to control the game state.
/// </summary>
public partial class Main : Node
{
	#region Exports, Fields, Objects, and Properties

	/// <summary>
	/// Represents the reference to the packed scene used for creating individual snake segments in the game.
	/// </summary>
	/// <remarks>
	/// This property is essential for dynamically spawning snake segments during gameplay. It is used
	/// as a reference by the snake system to instantiate new segments, ensuring cohesive behavior and
	/// consistent visuals throughout the game.
	/// </remarks>
	[Export]
	public PackedScene SnakeSegmentPs {get; set;}

	/// <summary>
	/// Represents the root node for organizing audio-related functionality in the game.
	/// </summary>
	/// <remarks>
	/// This property is used to manage and initialize the audio system of the game. It serves
	/// as a reference point for the <c>Audio.Initialize</c> method, allowing the system to access
	/// and control various audio resources and groups.
	/// </remarks>
	[Export]
	public Node AudioGroupRoot {get; set;}

	/// <summary>
	/// Delegate for the HudFlashRequested signal, typically used to request
	/// a flash or highlight action on the HUD in response to a game event.
	/// </summary>
	/// <param name="type">An integer parameter indicating the type or category
	/// of the flash request. Typically used to differentiate various HUD flash actions.</param>
	[Signal]
	public delegate void HudFlashRequestedEventHandler(int type);
	private SnakeManager Snake { get; }
	private ItemManager Items { get; }
	private BoardManager Board { get; }
	private ScoreManager Score { get; }
	private HealthManager Health { get; }
	private TimeManager Time { get; }
	private AudioManager Audio { get; }
	// ReSharper disable once InconsistentNaming
	private UiManager UI { get; }

	private bool _gameStarted;
	private bool _pause;
	private Vector2I _moveDirection;
	#endregion

	#region GameController Setup

	/// <summary>
	/// Represents the entry point and primary controller for the game.
	/// This class initializes and manages the core components of the game,
	/// including the snake, board, items, and score systems, ensuring
	/// proper interaction and operation between them. It acts as the
	/// centralized hub for game logic and state management.
	/// </summary>
	public Main()
	{
		Snake = new SnakeManager();
		Items = new ItemManager(Snake);
		Board = new BoardManager();
		Score = new ScoreManager();
		Health = new HealthManager();
		Time = new TimeManager();
		Audio = new AudioManager();
		UI = new UiManager();
	}

	/// <summary>
	/// Called when the node is added to the scene tree and fully initialized.
	/// This method sets up the initial game state by performing necessary initialization tasks,
	/// such as configuring game managers, connecting signals, and starting a new game session.
	/// It ensures that all dependencies and references are properly set and that the game is ready to run.
	/// </summary>
	public override void _Ready()
	{
		InitializeManagers();
		ConnectSignals();
		NewGame();
	}

	/// <summary>
	/// Initializes and configures all game managers required for core gameplay functionality.
	/// This includes setting up the TimeManager, UiManager, BoardManager, SnakeManager,
	/// ItemManager, and AudioManager. Each manager is provided with the necessary references
	/// to nodes, resources, and configuration details to ensure proper operation and
	/// seamless coordination between game systems.
	/// </summary>
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
		
		Snake.SetSnakeSegmentPs(SnakeSegmentPs);
		Snake.SetCellPixelSizeRef(Board.CellPixelSize);
		Snake.MapDirections();

		Items.SetCellPixelSizeRef(Board.CellPixelSize);
		Items.SetBoardCellSizeRef(Board.BoardCellSize);
		Items.SetPlacementOffset(new Vector2I(Board.CellPixelSize/2, Board.CellPixelSize/2));
		Items.InitializeEggNode(GetNode<Node2D>("Egg"));
		Items.LoadItems(GetNode("ItemManager"));
		Items.SetItemRates();
		
		Audio.Initialize(AudioGroupRoot);
	}

	/// <summary>
	/// Establishes connections between various signals and their corresponding event handlers
	/// within the game. Manages interactions across different game systems including HUD updates,
	/// snake behaviors, item interactions, score changes, health events, and audio feedback. This
	/// method ensures proper communication between systems to support seamless gameplay functionality.
	/// </summary>
	private void ConnectSignals()
	{
		// Main
		HudFlashRequested += type =>
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
			Snake.SetMoveData(action, direction);
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
			double newValue = Score.ComboPointsX;
			
			if (isDelta) newValue += amount;
			else newValue = amount;
			Score.UpdateComboPointsX(newValue);
		};
		Items.ComboPointsYChanged += (amount, isDelta) =>
		{
			double newValue = Score.ComboPointsY;
			
			if (isDelta) newValue += amount;
			else newValue = amount;
			Score.UpdateComboPointsY(newValue);
		};
		Items.ComboStarted += () =>
		{
			Time.SetMoveTimerWaitTime(.075);
			Score.StartCombo();
		};
		Items.ComboEnded += () =>
		{
			Time.SetMoveTimerWaitTime(0.1);
			Score.EndCombo();
		};
		Items.ComboCancelled += () => Score.CancelCombo();
		Items.GameOverRequested += EndGame;
		Items.HudFlashRequested += type =>
		{
			EmitSignal(SignalName.HudFlashRequested, type);
		};
		Items.EggMoved += () => Audio.PlayProgressSfx();
		Items.BadFoodEaten += () => Audio.PlayBadFoodSfx();
		Items.SpecialEggEaten += (inCombo) => Audio.PlaySpecialEggSfx(inCombo);
		Items.HighChimeRequested += () => Audio.PlayHighChimeSfx();

		// Score
		Score.ScoreChanged += (s, cx, cy, inCombo) => {
			UI.UpdateScore(s, cx, cy, inCombo);
			Items.UpdateScoreReferences(s, cx, cy);
		};
		Score.ComboStarted += () =>
		{
			Audio.PlayPowerUpSfx();
			UpdateMusic();
		};
		Score.ComboEnded += () =>
		{
			Audio.PlayPowerDownSfx();
			UpdateMusic();
		};
		Score.ComboCanceled += () =>
		{
			Audio.PlayPowerDownSfx();
			UpdateMusic();
		};

		// Health
		Health.HealthSegmentAdded += (node) => GetNode<CanvasLayer>("Hud").AddChild(node);
		Health.HealthSegmentRemoved += (node) => node.QueueFree();
		Health.HealthDeducted += () => {
			EmitSignal(SignalName.HudFlashRequested, 1);
			//TODO: Should be included in change for low health flashing.
			UI.UpdateWindowDressing(Health.Lives <= 2);
			Time.StartHealthTimer();
			Score.EndCombo();
			Audio.PlayHurtSfx();
		};
		Health.GameOverRequested += EndGame;
	}

	#endregion

	/// <summary>
	/// Called every frame to manage logic updates within the game. Handles item pulse
	/// animations, egg pulse effects, and game state changes such as pause and quit.
	/// Processes player input for direction changes and controls the flow of the game
	/// based on the current pause state.
	/// </summary>
	/// <param name="delta">The amount of time elapsed since the last frame, used for
	/// ensuring time-based updates are frame-rate independent.</param>
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

	/// <summary>
	/// Initializes a new gameplay session by resetting the game state, clearing the game board,
	/// and reinitializing critical managers and user interface components. This method ensures
	/// the environment is prepared for a fresh start, including setting the initial move direction,
	/// generating a new snake, and resetting gameplay variables such as score and health.
	/// </summary>
	private void NewGame()
	{
		_gameStarted = false;
		Time.SetMoveTimerWaitTime(0.5);
		GetTree().Paused = false;
		GetTree().CallGroup("snake", "queue_free");
		
		Score.Reset();
		UI.HideGameOver();
		UI.SetBackgroundVisible(false);
		UI.UpdateWindowDressing(false);
		UI.ResetHudPanels();
		
		_moveDirection = SnakeManager.UpMove;
		Snake.MoveDirection = _moveDirection;
		Snake.GenerateSnake();
		
		Items.Dispose();
		Health.Initialize(
			GetNode<CanvasLayer>("Hud").GetNode<Panel>("ScorePanel").GetNode<Panel>("HealthPanel").Position,
			Board.CellPixelSize,
			SnakeSegmentPs
		);
		Audio.PlayLobbyMusic();
	}

	/// <summary>
	/// Initializes and starts a new game session. This includes setting the game state to active,
	/// starting any necessary timers, and updating the game music to reflect the active state.
	/// If a game session is already ongoing, this method will not reinitialize.
	/// </summary>
	private void StartGame()
	{
		if (_gameStarted) return;
		
		_gameStarted = true;
		Time.StartMoveTimer();
		UpdateMusic();
	}

	/// <summary>
	/// Ends the current game session and performs all necessary cleanup and state updates.
	/// This includes resetting the player's health, stopping timers, pausing the game,
	/// finalizing the score, and updating the UI to display the game over state.
	/// </summary>
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
		
		Audio.GameOver();
	}

	/// <summary>
	/// Verifies if the snake has collided with itself by checking if the head's position
	/// overlaps with any of the other snake segments. If self-collision is detected, performs
	/// the necessary actions to handle the scenario, such as removing the snake's tail
	/// from the point of collision and deducting health.
	/// </summary>
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

	/// <summary>
	/// Determines if the player-controlled snake has consumed an egg in the current frame.
	/// If the snake's head position matches the egg's position, triggers mechanics such as
	/// updating the game state, extending the snake's length, updating the score,
	/// manipulating the combo system, and adjusting gameplay elements like background visuals and timers.
	/// </summary>
	/// <returns>True if the snake has eaten an egg; otherwise, false.</returns>
	private bool CheckEggEaten()
	{
		if (Items.EggPosition != Snake.SnakeData[0]) return false;
		
		Items.EggEaten();
		Snake.AddSegment(Snake.OldData[^1]);
		UI.SetBackgroundVisible(true);
		Score.IncrementComboTally();

		UI.UpdateComboMeter(Score.ComboTally, Score.IsInCombo);
		EmitSignal(SignalName.HudFlashRequested, 0);

		if (!Score.IsInCombo)
		{
			Score.AddScore(1);
			UpdateBackgroundOnEggEaten();
			Time.SetMoveTimerWaitTime(Snake.SnakeNodes.Count < 6 ? 0.25 : 0.1);
			if (Score.ComboTally >= 7) Score.StartCombo();
		}
		else
		{
			Time.SetMoveTimerWaitTime(.05);
			UpdateBackgroundInCombo();
			Score.ComboPointsX += 2;
		}
		UpdateMusic();
		return true;
	}

	/// <summary>
	/// Adjusts the game board's background display following the consumption of an egg.
	/// When the player consumes an egg and their combo tally exceeds a specified threshold,
	/// the background alternates between predefined visual frames to reflect ongoing achievements
	/// and enhance gameplay responsiveness.
	/// </summary>
	private void UpdateBackgroundOnEggEaten()
	{
		if (Score.ComboTally > 3)
		{
			UI.SetBackgroundFrame(Score.ComboTally % 2 == 0 ? 1 : 2);
		}
	}

	/// <summary>
	/// Updates the game board's background appearance during an active combo streak.
	/// Depending on the current combo tally, the background frame is dynamically set
	/// to create a visually distinct effect, enhancing gameplay feedback by reflecting
	/// the player's progress and maintaining engagement.
	/// </summary>
	private void UpdateBackgroundInCombo()
	{
		if (Score.ComboTally % 2 == 0) UI.SetBackgroundFrame(4);
		else if (Score.ComboTally % 3 == 0) UI.SetBackgroundFrame(5);
		else UI.SetBackgroundFrame(3);
	}

	/// <summary>
	/// Checks if the snake's current head position intersects with any item or wall on the game board.
	/// If a collision is detected, the corresponding item's effect is processed, potentially impacting
	/// game state such as score or combo status.
	/// </summary>
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

	/// <summary>
	/// Checks if the game board is fully occupied and triggers the end of the game if so.
	/// This method calculates the total number of occupied cells, including the snake segments,
	/// walls, large walls, and the egg itself.
	/// If the total occupied cells equal or exceed the board's capacity, the game concludes.
	/// </summary>
	/// <param name="eggEaten">Indicates whether the egg has been eaten during the current update cycle.
	/// This parameter determines whether the board occupancy check should proceed.</param>
	private void CheckFullBoard(bool eggEaten)
	{
		if (!eggEaten) return;
		int occupied = 1 + Snake.SnakeData.Count + Items.WallsData.Count + (Items.LargeWallsData.Count * 4);
		if (occupied >= Board.BoardCellSize * Board.BoardCellSize)
		{
			EndGame();
		}
	}
	
	/// <summary>
	/// Manages the game's background music by determining the appropriate track
	/// to play based on the current game state. When the game is not started,
	/// it plays the lobby music. If the player is in a combo, it switches to the
	/// combo music. Otherwise, it selects different tracks based on the current
	/// move timer's wait time, either playing faster-paced or space-themed music
	/// to match the gameplay intensity.
	/// </summary>
	private void UpdateMusic()
	{
		if (!_gameStarted)
		{
			Audio.PlayLobbyMusic();
			return;
		}

		if (Score.IsInCombo)
		{
			Audio.PlayComboMusic();
			return;
		}
		double time = Time.GetMoveTimerWaitTime();
		if (time < 0.5)
		{
			Audio.PlayBaseMusic();
		}
		else
		{
			Audio.PlaySpaceMusic();
		}
	}
	
	#endregion
	
	#region Input Handling & Signals

	/// <summary>
	/// Handles the timeout event triggered by the HUD flash timer.
	/// This method is responsible for the following operations:
	/// 1. Resets the HUD panels to their default state to ensure visual consistency in the user interface.
	/// 2. Stops the HUD flash timer to prevent further unnecessary executions of the flash logic.
	/// </summary>
	private void _on_hud_flash_timer_timeout()
	{
		UI.ResetHudPanels();
		Time.StopHudFlashTimer();
	}

	/// <summary>
	/// Handles the timeout event triggered by the health timer.
	/// This method performs the following operations to manage gameplay state:
	/// 1. Updates the user interface to reflect a visual warning if the player's health is critically low.
	/// 2. Stops the health timer to prevent further automatic health-related events until explicitly restarted.
	/// </summary>
	private void _on_health_timer_timeout()
	{
		UI.UpdateWindowDressing(Health.Lives <= 2);
		Time.StopHealthTimer();
	}

	/// <summary>
	/// Handles the restart action triggered from the game over menu.
	/// This method initiates a new game by resetting the game state,
	/// reinitializing all critical managers, and ensuring the game board
	/// and user interface are ready for a fresh start.
	/// The following operations are performed:
	/// 1. Resets the gameplay state and move timer using <see cref="NewGame"/>.
	/// 2. Fully reinitialized the score, health, and other gameplay components.
	/// 3. Clears the existing game entities and creates a new snake.
	/// 4. Ensures the UI hides the game over screen and displays a new game setup.
	/// </summary>
	private void _on_game_over_menu_restart()
	{
		NewGame();
	}

	/// <summary>
	/// Handles the actions to be performed when the snake's movement timer times out.
	/// This method updates the snake's position, checks for collisions, and determines
	/// conditions like game over or progression.
	/// The following checks and operations are performed in sequence:
	/// 1. Updates the snake's position using <see cref="SnakeManager.UpdateSnake"/>.
	/// 2. Checks if the snake's head position is out of board boundaries using <see cref="BoardManager.IsOutOfBounds"/>.
	/// If true, ends the game.
	/// 3. Verifies if the snake has collided with itself.
	/// 4. Checks if an egg has been eaten and handles full-board logic if necessary.
	/// 5. Determines if the snake has hit any collectible items.
	/// 6. Checks if the snake has collided with a large item using <see cref="ItemManager.CheckLargeItemHit"/>.
	/// If true, ends the game.
	/// </summary>
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
