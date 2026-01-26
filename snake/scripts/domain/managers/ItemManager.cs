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
using Snake.Scripts.Domain.Utilities;
using Snake.Scripts.Interfaces;

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Manages game items, including their spawn rates, placement, interactions, and effects within the game board.
/// Signals out impact of different items to Main.
/// </summary>
public partial class ItemManager : Node
{
	#region Signals
	/// <summary>
	/// Represents the delegate for handling events triggered when a health deduction is requested.
	/// This signal communicates the details of the health deduction process.
	/// </summary>
	[Signal]
	public delegate void HealthDeductedRequestedEventHandler();

	/// <summary>
	/// Represents the delegate for handling events triggered when the score changes.
	/// This signal provides the amount of change and indicates whether the change is incremental.
	/// </summary>
	[Signal]
	public delegate void ScoreChangedEventHandler(double amount, bool isDelta);

	/// <summary>
	/// Represents the delegate for handling events triggered when the X-coordinate of combo points changes.
	/// This signal provides details about the amount of change and indicates if the change is incremental.
	/// </summary>
	[Signal]
	public delegate void ComboPointsXChangedEventHandler(double amount, bool isDelta);

	/// <summary>
	/// Represents the delegate for handling events triggered when the Y-coordinate of combo points changes.
	/// This signal provides details about the amount of change and specifies if the change is incremental.
	/// </summary>
	[Signal]
	public delegate void ComboPointsYChangedEventHandler(double amount, bool isDelta);

	/// <summary>
	/// Represents the delegate for handling events triggered when a combo sequence has started.
	/// This signal informs the system to initiate or respond to the beginning of a combo action.
	/// </summary>
	[Signal]
	public delegate void ComboStartedEventHandler();

	/// <summary>
	/// Represents the delegate for handling events triggered when a combo sequence has successfully ended.
	/// This signal notifies the system to respond to the completion of an ongoing combo action.
	/// </summary>
	[Signal]
	public delegate void ComboEndedEventHandler();

	/// <summary>
	/// Represents the delegate for handling events triggered when a combo sequence is interrupted or canceled.
	/// This signal notifies the system to respond to the cancellation of an ongoing combo action.
	/// </summary>
	[Signal]
	public delegate void ComboCancelledEventHandler();

	/// <summary>
	/// Represents the delegate for handling events that request the termination of the game session.
	/// This signal is used to notify the system when certain conditions are met that warrant ending the game.
	/// </summary>
	[Signal]
	public delegate void GameOverRequestedEventHandler();

	/// <summary>
	/// Represents the delegate for handling events that request a flash effect on the HUD.
	/// This signal is used to instruct the HUD to visually indicate certain actions or events
	/// based on a specified type.
	/// </summary>
	[Signal]
	public delegate void HudFlashRequestedEventHandler(int type);

	/// <summary>
	/// Represents the delegate for handling events triggered when an item is spawned.
	/// This signal is used to notify subscribers about the creation or appearance
	/// of a new item within the game environment.
	/// </summary>
	[Signal]
	public delegate void ItemSpawnedEventHandler(Node2D item);

	/// <summary>
	/// Represents the delegate for handling events triggered when an egg is moved.
	/// This signal is used to notify subscribers about changes in the position
	/// of an egg within the game environment.
	/// </summary>
	[Signal]
	public delegate void EggMovedEventHandler();

	/// <summary>
	/// Represents the delegate for handling events triggered when bad food is eaten.
	/// This signal is used to notify subscribers about the consumption of an undesirable food item
	/// and can be used to implement negative effects or penalties in the game.
	/// </summary>
	[Signal]
	public delegate void BadFoodEatenEventHandler();

	/// <summary>
	/// Represents the delegate for handling events triggered when a special egg is eaten.
	/// This signal is used to notify subscribers about the special egg consumption,
	/// optionally including whether it is part of a combo sequence.
	/// </summary>
	[Signal]
	public delegate void SpecialEggEatenEventHandler(bool isInCombo);

	/// <summary>
	/// Represents the delegate for handling events when a high chime is requested.
	/// This signal is intended to notify listeners to execute actions associated with the request.
	/// </summary>
	[Signal]
	public delegate void HighChimeRequestedEventHandler();
	#endregion
	
	#region Properties
	/// <summary>
	/// Represents the size of a cell in pixels.
	/// This property is used to define the pixel dimensions of an individual cell
	/// within the context of the item's management system.
	/// </summary>
	public int CellPixelSizeRef { get; set; }

	/// <summary>
	/// Defines the reference size of a board 'cell', meaning a square within the grid.
	/// This property is used to establish the dimensions of a cell within the board's coordinate system.
	/// </summary>
	public int BoardCellSizeRef { get; set; }

	/// <summary>
	/// Defines the offset to be applied to the placement of items on the game board.
	/// This property is used to adjust the position of game elements to ensure
	/// consistent and visually aligned placement relative to the grid and other elements.
	/// </summary>
	public Vector2I PlacementOffsetRef { get; set; }

	/// <summary>
	/// Represents the node associated with the egg in the game.
	/// This property is used to manage the egg's visual and positional state on the board.
	/// </summary>
	public Node2D EggNode { get; set; }

	/// <summary>
	/// Represents a reference to the current score.
	/// This property is used to track and manage the score within the system.
	/// </summary>
	public double CurrentScoreRef { get; set; }

	/// <summary>
	/// Tracks the current combo multiplier reference value.
	/// This property is used to maintain and update the multiplier points
	/// for combinations in the context of the item's management system.
	/// </summary>
	public double CurrentComboPointsXRef { get; set; }

	/// <summary>
	/// Represents the Y-position reference for the current combo points.
	/// This property is used to determine or track the vertical position
	/// of the combo points within the system.
	/// </summary>
	public double CurrentComboPointsYRef { get; set; }
	#endregion

	#region Private Boundaries & Fields
	internal Node2D WallNode;
	internal Vector2I EggPosition;
	internal int Tally;
	private Dictionary<int, List<Node2D>> _itemRates;
	internal List<Node2D> ItemNodes;
	internal List<Vector2I> ItemsData;
	internal List<Node2D> WallNodes;
	internal List<Vector2I> WallsData;
	internal List<Node2D> LargeWallNodes;
	internal List<Vector2I> LargeWallsData;
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

	private string _freshEggPath;
	private string _ripeEggPath;
	private string _rottenEggPath;
	private string _mushroomPath;
	private string _shinyEggPath;
	private string _skullPath;
	private string _dewDropPath;
	private string _lavaEggPath;
	private string _frogPath;
	private string _alienEggPath;
	private string _iceEggPath;
	private string _pillItemPath;
	private string _discoEggPath;
	internal Node2D LargeWallNode;
	private ItemEffectLogic _effectLogic;
	private double _eggPulseTime;
	private double _itemPulseTime;
	private Vector2I _offset;
	private readonly ISnakeManager _snake;
	#endregion

	#region Initialization & Cleanup
	/// <summary>
	/// Manages the operations and logic related to items within the game, including their placement,
	/// effects, and interactions. This class handles item spawning, item effects, and interactions
	/// such as scoring, combo mechanics, wall placements, and egg movements.
	/// It includes event signaling for game updates and interactions.
	/// </summary>
	/// <param name="snake">The snake manager instance responsible for snake-related operations and interactions.</param>
	public ItemManager(ISnakeManager snake)
	{
		_snake = snake;
	}

	/// <summary>
	/// Parameterless initialization of a new instance of the ItemManager class.
	/// </summary>
	public ItemManager()
	{
	}

	/// <summary>
	/// Loads and initializes item nodes and their corresponding file paths for the game.
	/// This method retrieves item-related nodes from the provided item manager and maps them
	/// to internal node fields. It also extracts file paths for each item's corresponding scene
	/// and prepares essential logic for item effects.
	/// </summary>
	/// <param name="itemManager">The node containing item-related child nodes and configurations.</param>
	internal void LoadItems(Node itemManager)
	{
		WallNode = itemManager.GetNode<Node2D>("Wall");
		_freshEggNode = itemManager.GetNode<Node2D>("FreshEgg");
		_ripeEggNode = itemManager.GetNode<Node2D>("RipeEgg");
		_rottenEggNode = itemManager.GetNode<Node2D>("RottenEgg");
		_mushroomNode = itemManager.GetNode<Node2D>("Mushroom");
		_shinyEggNode = itemManager.GetNode<Node2D>("ShinyEgg");
		_skullNode = itemManager.GetNode<Node2D>("Skull");
		_dewDropNode = itemManager.GetNode<Node2D>("DewDrop");
		_lavaEggNode = itemManager.GetNode<Node2D>("LavaEgg");
		_frogNode = itemManager.GetNode<Node2D>("Frog");
		_alienEggNode = itemManager.GetNode<Node2D>("AlienEgg");
		_iceEggNode = itemManager.GetNode<Node2D>("IceEgg");
		_pillItemNode = itemManager.GetNode<Node2D>("Pill");
		_discoEggNode = itemManager.GetNode<Node2D>("DiscoEgg");
		LargeWallNode = itemManager.GetNode<Node2D>("LargeWall");

		_freshEggPath = _freshEggNode.SceneFilePath;
		_ripeEggPath = _ripeEggNode.SceneFilePath;
		_rottenEggPath = _rottenEggNode.SceneFilePath;
		_mushroomPath = _mushroomNode.SceneFilePath;
		_shinyEggPath = _shinyEggNode.SceneFilePath;
		_skullPath = _skullNode.SceneFilePath;
		_dewDropPath = _dewDropNode.SceneFilePath;
		_lavaEggPath = _lavaEggNode.SceneFilePath;
		_frogPath = _frogNode.SceneFilePath;
		_alienEggPath = _alienEggNode.SceneFilePath;
		_iceEggPath = _iceEggNode.SceneFilePath;
		_pillItemPath = _pillItemNode.SceneFilePath;
		_discoEggPath = _discoEggNode.SceneFilePath;

		_effectLogic = new ItemEffectLogic(_snake, _rottenEggPath, _dewDropPath);
	}
	
	/// <summary>
	/// Sets the cell pixel size reference, defining the size of each cell
	/// in pixels for accurate positioning on the game board.
	/// </summary>
	/// <param name="boardCellPixelSize">The size of a single cell in pixels, specified as an integer.</param>
	public void SetCellPixelSizeRef(int boardCellPixelSize)
	{
		CellPixelSizeRef = boardCellPixelSize;
	}

	/// <summary>
	/// Sets the board cell size reference, which determines the size of each cell
	/// on the game board in logically defined units.
	/// </summary>
	/// <param name="boardBoardCellSize">The size of a single logical cell on the board, specified as an integer.</param>
	public void SetBoardCellSizeRef(int boardBoardCellSize)
	{
		BoardCellSizeRef = boardBoardCellSize;
	}

	/// <summary>
	/// Sets the placement offset for item placement operations,
	/// which determines the reference point for positioning items on the game board.
	/// </summary>
	/// <param name="vector2I">The offset in pixels specified as a Vector2I value.</param>
	public void SetPlacementOffset(Vector2I vector2I)
	{
		PlacementOffsetRef = vector2I;
	}

	/// <summary>
	/// Initializes the EggNode with the specified Node2D instance,
	/// allowing it to be used for egg-related operations within the item management system.
	/// </summary>
	/// <param name="getNode">The Node2D instance to assign as the EggNode.</param>
	public void InitializeEggNode(Node2D getNode)
	{
		EggNode = getNode;
	}
	
	/// <summary>
	/// Releases all resources used by the item manager, including clearing all
	/// item and wall-related data, freeing all associated nodes, and resetting
	/// the necessary collections to their initial states. Additionally, updates
	/// the egg's position after resource disposal.
	/// </summary>
	public new void Dispose()
	{
		Tally = 0;
		if (ItemNodes != null)
		{
			foreach (var node in ItemNodes.Where(GodotObject.IsInstanceValid))
			{
				node.Free();
			}
			ItemNodes.Clear();
			ItemsData.Clear();
		}
		if (WallNodes != null)
		{
			foreach (var node in WallNodes.Where(GodotObject.IsInstanceValid))
			{
				node.Free();
			}
			WallNodes.Clear();
			WallsData.Clear();
		}
		if (LargeWallNodes != null)
		{
			foreach (var node in LargeWallNodes.Where(GodotObject.IsInstanceValid))
			{
				node.Free();
			}
			LargeWallNodes.Clear();
			LargeWallsData.Clear();
		}
		//MEMO: avoids unnecessary null checks. Useful since we hash available positions.
		// ReSharper disable once UseCollectionExpression
		ItemNodes = new List<Node2D>();
		// ReSharper disable once UseCollectionExpression
		ItemsData = new List<Vector2I>();
		// ReSharper disable once UseCollectionExpression
		WallNodes = new List<Node2D>();
		// ReSharper disable once UseCollectionExpression
		WallsData = new List<Vector2I>();
		// ReSharper disable once UseCollectionExpression
		LargeWallNodes = new List<Node2D>();
		// ReSharper disable once UseCollectionExpression
		LargeWallsData = new List<Vector2I>();
		
		MoveEgg();
	}
	#endregion
	
	#region Item Rates & Rules
	/// <summary>
	/// Configures item spawn rates by mapping item categories to their respective node representations.
	/// This method initializes a dictionary where each key represents a spawn frequency, and its corresponding value
	/// is a list of items to be spawned at that frequency. Higher keys denote rarer items.
	/// The mapping ensures that different nodes, such as eggs, walls, and power-ups, are assigned varying likelihoods
	/// of appearing in the game. This configuration influences item distribution dynamics during gameplay.
	/// </summary>
	internal void SetItemRates()
	{
		_itemRates = new Dictionary<int, List<Node2D>>
		{
			{ 1, [WallNode, _freshEggNode] },
			{ 2, [_ripeEggNode] },
			{ 3, [_rottenEggNode] },
			{ 5, [_mushroomNode] },
			{ 8, [_shinyEggNode] },
			{ 13, [_skullNode, LargeWallNode] },
			{ 21, [_dewDropNode] },
			{ 34, [_lavaEggNode] },
			{ 55, [_frogNode] },
			{ 89, [_alienEggNode] },
			{ 144, [_iceEggNode] },
			{ 233, [_pillItemNode] },
			{ 377, [_discoEggNode] }
		};
	}
	
	/// <summary>
	/// Processes the result of an item interaction, handling its removal, triggering signals,
	/// and applying its effects based on the item's type and whether it is part of a combo.
	/// </summary>
	/// <param name="item">The item being processed, represented as a Node2D instance.</param>
	/// <param name="i">The index of the item in the corresponding item data list.</param>
	/// <param name="isInCombo">A boolean indicating if the item interaction is part of a combo sequence.</param>
	internal void ItemResult(Node2D item, int i, bool isInCombo)
	{
		if (item.SceneFilePath != LargeWallNode.SceneFilePath)
		{
			if (item.SceneFilePath == WallNode.SceneFilePath)
			{				
				item.QueueFree();
				WallsData.RemoveAt(i);
				WallNodes.RemoveAt(i);}
			else
			{
				item.QueueFree();
				ItemsData.RemoveAt(i);
				ItemNodes.RemoveAt(i);
				EmitSignal(SignalName.HudFlashRequested, 2);
			}
		}

		if (item.SceneFilePath == WallNode.SceneFilePath 
		    || item.SceneFilePath == LargeWallNode.SceneFilePath)
		{
			EmitSignal(SignalName.HealthDeductedRequested);
		}

		ApplyItemEffect(item.SceneFilePath, isInCombo);
	}
	
	/// <summary>
	/// Applies the effect of the specified item based on its type and whether it is part of a combo.
	/// Depending on the item's properties, this may modify the snake's attributes, update the score,
	/// emit signals, or trigger special game events.
	/// </summary>
	/// <param name="sceneFilePath">The file path representing the item's scene, used to identify the type of item.</param>
	/// <param name="isInCombo">A boolean indicating if the item is part of a combo sequence, which may alter the effect logic.</param>
	internal void ApplyItemEffect(string sceneFilePath, bool isInCombo)
	{
		_effectLogic?.Apply(sceneFilePath);

		//Good eggs
		if (sceneFilePath == _freshEggPath)
		{
			//Modify X during Combo
			EmitSignal(isInCombo ? SignalName.ComboPointsYChanged : SignalName.ScoreChanged, 2, true);
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.SpecialEggEaten, isInCombo);
		}
		if (sceneFilePath == _ripeEggPath)
		{
			//Modify Y during Combo
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * 1.5, false);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, 3, true);	
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.SpecialEggEaten, isInCombo);
		}
		if (sceneFilePath == _shinyEggPath)
		{
			//Modify Y during Combo
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * 2, false);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, 5, true);
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.SpecialEggEaten, isInCombo);
		}
		if (sceneFilePath == _alienEggPath)
		{
			//Modify Y during Combo
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * 5, false);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, 8, true);
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.SpecialEggEaten, isInCombo);
		}
		if (sceneFilePath == _discoEggPath)
		{
			//Modify Y during Combo
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * Math.Sqrt(Math.Abs(CurrentScoreRef)), false);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, 13, true);
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.SpecialEggEaten, isInCombo);
		}
		//Bad eggs
		if (sceneFilePath == _rottenEggPath)
		{
			//End Combo, Modify X
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsXChanged, -Math.Max(10, CurrentScoreRef * 0.25), true);
				EmitSignal(SignalName.ComboEnded);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, -Math.Max(10, CurrentScoreRef * 0.10), true);
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.BadFoodEaten);
		}
		if(sceneFilePath == _lavaEggPath)
		{
			//End Combo, Modify Y
			if (isInCombo)
			{			
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * 0.25, false);
				EmitSignal(SignalName.ComboEnded);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, -Math.Max(75, CurrentScoreRef * 0.75), true);
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.BadFoodEaten);
		}
		if (sceneFilePath == _iceEggPath)
		{
			//Cancel Combo
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboCancelled);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, Math.Min(CurrentScoreRef - 10000, Math.Sqrt(Math.Abs(CurrentScoreRef))), false);
			}
			_snake.AddSegment(_snake.OldData[^1]);
			EmitSignal(SignalName.BadFoodEaten);
		}
		//Complex Scorers
		if (sceneFilePath == _mushroomPath)
		{
			//Start Combo, Modify X
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsXChanged, CurrentComboPointsXRef * 1.25, false);
			}
			else
			{
				// If the score is negative, set it to a fixed value of -1.
				// Otherwise, apply a slight exponential increase (power of 1.05) to the absolute value of the score.
				EmitSignal(SignalName.ScoreChanged, (CurrentScoreRef < 0) ? (-1) : Math.Pow(Math.Abs(CurrentScoreRef), 1.05), false);
				EmitSignal(SignalName.ComboStarted);	
			}
			EmitSignal(SignalName.HighChimeRequested);
		}
		if (sceneFilePath == _dewDropPath)
		{
			//End Combo, No Combo Score Change
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsXChanged, Math.Abs(CurrentComboPointsXRef), false);
				EmitSignal(SignalName.ComboPointsYChanged, Math.Abs(CurrentComboPointsYRef), false);
				EmitSignal(SignalName.ComboEnded);
			}
			else
			{
				EmitSignal(SignalName.ScoreChanged, Math.Abs(CurrentScoreRef), false);
			}
			// Set the score to its absolute value, effectively removing any negative sign.
		}
		if (sceneFilePath == _frogPath)
		{
			//Start Combo, Modify Y
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * 3, false);
			}
			else
			{
				// If the score is negative, reduce it further by subtracting an exponential value (power of 1.15).
				// If the score is positive, apply an exponential increase (power of 1.15).
				EmitSignal(SignalName.ScoreChanged, (CurrentScoreRef < 0) ? (Math.Abs(CurrentScoreRef) - Math.Pow(Math.Abs(CurrentScoreRef), 1.15)) 
					: Math.Pow(Math.Abs(CurrentScoreRef), 1.15), false);
				EmitSignal(SignalName.ComboStarted);
			}
			EmitSignal(SignalName.HighChimeRequested);
		}
		if (sceneFilePath == _pillItemPath)
		{
			//Start Combo, Modify X & Y
			if (isInCombo)
			{
				EmitSignal(SignalName.ComboPointsXChanged, CurrentComboPointsXRef * 8, false);
				EmitSignal(SignalName.ComboPointsYChanged, CurrentComboPointsYRef * 8, false);
			}
			else
			{
				// If the score is negative, apply a larger exponential reduction (power of 1.5) to the absolute value.
				// If the score is positive, apply an exponential increase (power of 1.5).
				EmitSignal(SignalName.ScoreChanged, (CurrentScoreRef < 0) ? (0 - Math.Pow(Math.Abs(CurrentScoreRef), 1.5)) 
					: Math.Pow(Math.Abs(CurrentScoreRef), 1.5), false);
				EmitSignal(SignalName.ComboStarted);	
			}
			EmitSignal(SignalName.HighChimeRequested);
		}
		// ReSharper disable once InvertIf
		if (sceneFilePath == _skullPath)
		{
			//Tally = 0;
			
			EmitSignal(SignalName.ComboCancelled);
			
			Random rnd = new Random();
			int result;
			do
			{
				result = rnd.Next(-1, 7);
			} while (result % 2 == 0); //the results can only be odd

			switch (result)
			{
				case -1:
					EmitSignal(SignalName.ScoreChanged, -9999, false);
					break;
				case 1:
					EmitSignal(SignalName.ScoreChanged, CurrentScoreRef + 9999, false); // Wait, score -= -9999 is score += 9999
					break;
				case 3:
					EmitSignal(SignalName.ScoreChanged, 0, false);
					break;
				case 5:
					EmitSignal(SignalName.ScoreChanged, CurrentScoreRef - 9999, false);
					break;
				default:
					EmitSignal(SignalName.ScoreChanged, 9999, false);
					break;
			}
		}
	}
	
	#endregion

	#region Item Management
	/// <summary>
	/// Handles the logic executed when the egg is eaten by the snake. This includes updating the tally of eaten eggs,
	/// repositioning the egg on the game board, and generating a new set of items at randomized locations.
	/// </summary>
	public void EggEaten()
	{
		Tally++;
		MoveEgg();
		GenerateFromItemLookup();
	}
	
	/// <summary>
	/// Randomly repositions the egg to a new valid spot on the game board, ensuring it is not trapped by walls.
	/// This method identifies all available positions on the board and selects one at random.
	/// It verifies that the chosen position does not result in a wall trap. If a trap is detected,
	/// it continues to select new positions until a valid one is found. Once the egg's new position
	/// is determined, its visual representation is updated accordingly.
	/// </summary>
	internal void MoveEgg()
	{
		EmitSignal(SignalName.EggMoved);
		List<Vector2I> available = FindAvailableSpots(Vector2I.Zero);
		
		do { EggPosition = RandomPlacement(available); }
		while ( CheckWallTrap() );
		if (EggNode != null)
		{
			EggNode.Position = EggPosition * CellPixelSizeRef + 
			                   new Vector2I(0, CellPixelSizeRef) + 
			                   PlacementOffsetRef;
			EggNode.ZIndex = 999;
			_eggPulseTime = 0;
			EggNode.Scale = Vector2.One;
		}
	}

	/// <summary>
	/// Randomly selects a valid position for item placement on the board.
	/// The method ensures that the selected position complies with several constraints, such as avoiding
	/// collisions with existing items, large items, or the snake's body, and maintaining a specified minimum
	/// distance from the snake's head and a certain proximity to the snake's tail.
	/// </summary>
	/// <param name="available">A list of available positions on the board where items can potentially be placed.</param>
	/// <returns>Returns a <c>Vector2I</c> object representing the valid randomly-selected position for placement.</returns>
	private Vector2I RandomPlacement(List<Vector2I> available)
	{
		Random rndm = new Random();

		int tryCounter = 0;
		
		Vector2I itemPlacement;
		do
		{
			//generate a new coordinate spot
			tryCounter++;
			if (tryCounter == 90000) //if there have been this many attempts to place the item, something is wrong.
			{
				EmitSignal(SignalName.GameOverRequested);
			}
			itemPlacement = new Vector2I(rndm.Next(1, BoardCellSizeRef + 1), rndm.Next(3, BoardCellSizeRef - 4)); 
			
		} while ((!available.Contains(itemPlacement) || //Don't place on an occupied position.
				  CheckLargeItemHit(itemPlacement) || //Don't place on large 
				  IsWithinRadius(itemPlacement, _snake.SnakeData[0], 3) || //Don't place too close to the snake head.
				  CheckWithinRadius(itemPlacement, _snake.SnakeData, 1) || //Don't place directly next to the entire body.
				  !IsWithinRadius(itemPlacement, _snake.SnakeData[^1], 20) //Place around the tail. Temporary rule? I kind of like it.
				 ));

		return itemPlacement;
		
		bool CheckWithinRadius(Vector2I newItemPlacement, List<Vector2I> listCoords, int radius)
		{
			return listCoords.Any(spot => IsWithinRadius(newItemPlacement, spot, radius));
		}

		// Helper function to check if a position is within a given radius
		bool IsWithinRadius(Vector2I position, Vector2I center, int radius)
		{
			int dx = Math.Abs(position.X - center.X);
			int dy = Math.Abs(position.Y - center.Y);
			return dx <= radius && dy <= radius;
		}
	}
	
	/// <summary>
	/// Generates new items based on the current tally and predefined item rate rules.
	/// For each rate rule, items are spawned if the tally satisfies the rule's condition.
	/// New items are created by duplicating existing item templates and are placed
	/// on the game board in valid positions.
	/// </summary>
	internal void GenerateFromItemLookup()
	{
		// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		foreach (KeyValuePair<int, List<Node2D>> rule in _itemRates)
		{
			if (Tally % rule.Key != 0) continue;
			foreach (var newItem in rule.Value.Select(itemNode => (Node2D)itemNode.Duplicate()))
			{
				SpawnItem(newItem);
			}
		}
	}

	/// <summary>
	/// Places the specified item on the game board in a valid position based
	/// on its type, ensuring it does not overlap with existing items, walls, or other occupied areas.
	/// </summary>
	/// <param name="newItem">The item to be placed on the game board, represented as a Node2D object.</param>
	private void SpawnItem(Node2D newItem)
	{
		newItem.Visible = true;
		
		Vector2I smallArea = new Vector2I(0, 0); //a single spot on the grid
		Vector2I largeArea = new Vector2I(1, 1); //a 2X2 area on the grid
		//Vector2I veryLargeArea = new Vector2I(2, 2); //a 3X3 area on the grid; nothing takes up that size yet.
		
		if (newItem.SceneFilePath == WallNode.SceneFilePath)
		{
			List<Vector2I> available = FindAvailableSpots(smallArea);
			_newItemPosition = RandomPlacement(available);
			newItem.Position = _newItemPosition * CellPixelSizeRef + 
			                   new Vector2I(0, CellPixelSizeRef) + 
			                   PlacementOffsetRef;
			
			WallNodes.Add(newItem);
			WallsData.Add(_newItemPosition);
		}
		else if (newItem.SceneFilePath == LargeWallNode.SceneFilePath)
		{
		 List<Vector2I> available = FindAvailableSpots(largeArea);
		_newItemPosition = RandomPlacement(available);
		newItem.Position = _newItemPosition * CellPixelSizeRef + 
		                   new Vector2I(0, CellPixelSizeRef) + 
		                   PlacementOffsetRef;

		newItem.Position += PlacementOffsetRef; //offset a second time to account for the large wall's largeness'
		LargeWallNodes.Add(newItem);
		LargeWallsData.Add(_newItemPosition);
		}
		else
		{
			List<Vector2I> available = FindAvailableSpots(smallArea);
			_newItemPosition = RandomPlacement(available);
			newItem.Position = _newItemPosition * CellPixelSizeRef + 
			                   new Vector2I(0, CellPixelSizeRef) + 
			                   PlacementOffsetRef;
			
			ItemNodes.Add(newItem);
			ItemsData.Add(_newItemPosition);
		}
		EmitSignal(SignalName.ItemSpawned, newItem);
	}
	
	#endregion
	
	#region Item Interaction Checks
	/// <summary>
	/// Checks if the current egg position is completely surrounded by walls, forming a trap.
	/// Specifically, this method evaluates whether the North, East, South, and West neighboring cells
	/// relative to the egg's position are all occupied by walls. If all these neighboring cells contain walls,
	/// the position is considered trapped and unsuitable for the egg's placement.
	/// </summary>
	/// <returns>
	/// Returns <c>true</c> if the egg's position is fully enclosed by walls in all four cardinal directions,
	/// indicating a wall trap. Otherwise, returns <c>false</c>.
	/// </returns>
	private bool CheckWallTrap()
	{
		var eggPosNord = new Vector2I(EggPosition.X, EggPosition.Y - 1);
		var eggPosEas = new Vector2I(EggPosition.X + 1, EggPosition.Y);
		var eggPosSud = new Vector2I(EggPosition.X, EggPosition.Y + 1);
		var eggPosWes = new Vector2I(EggPosition.X - 1, EggPosition.Y);
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (WallsData.Contains(eggPosNord) && WallsData.Contains(eggPosEas) && WallsData.Contains(eggPosSud) &&
		    WallsData.Contains(eggPosWes))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Checks if the given position intersects with any part of a large item on the board.
	/// A large item is represented as a cluster of cells forming a square.
	/// The method verifies if the position matches the top-left corner of the cluster or
	/// any of its adjacent cells (right, bottom, or bottom-right).
	/// </summary>
	/// <param name="position">The position on the board to check for a collision with a large item.</param>
	/// <returns>Returns true if the position overlaps any part of a large item; otherwise, false.</returns>
	internal bool CheckLargeItemHit(Vector2I position)
	{
		bool hit = false;
		for (int i = 0; i < LargeWallsData.Count; i++)
		{
			Vector2I q2 = new Vector2I(x: LargeWallsData[i].X, y: LargeWallsData[i].Y + 1);
			Vector2I q3 = new Vector2I(x: LargeWallsData[i].X + 1, y: LargeWallsData[i].Y + 1);
			Vector2I q4 = new Vector2I(x: LargeWallsData[i].X + 1, y: LargeWallsData[i].Y);

			if (position == LargeWallsData[i] || position == q2 || position == q3 || position == q4 )
			{
				hit = true;
			}
		}

		return hit;
	}
	
	/// <summary>
	/// Identifies and returns all available positions on the game board where items can be placed,
	/// ensuring that the specified area does not overlap with existing snake segments, items, walls,
	/// or other occupied positions.
	/// </summary>
	/// <param name="area">The size of the area to check for availability, represented as a Vector2I
	/// where X and Y define the width and height of the area respectively.</param>
	/// <returns>A list of Vector2I positions representing the top-left corners of all available
	/// spots on the game board that can accommodate the specified area.</returns>
	private List<Vector2I> FindAvailableSpots(Vector2I area)
	{
		List<Vector2I> available = [];
		
		HashSet<Vector2I> occupiedPositions = [.._snake.SnakeData, EggPosition];
		occupiedPositions.UnionWith(ItemsData);
		occupiedPositions.UnionWith(WallsData);
		occupiedPositions.UnionWith(LargeWallsData);
		
		// Calculate the actual size needed (area + 1 since Vector2I(0,0) = 1x1)
		int width = area.X + 1;
		int height = area.Y + 1;
	
		// Iterate through all possible starting positions
		for (int x = 0; x <= BoardCellSizeRef - width; x++)
		{
			for (int y = 0; y <= BoardCellSizeRef - height; y++)
			{
				Vector2I position = new Vector2I(x, y);
			
				// Check if all positions in the area are free
				bool areaIsFree = true;
				for (int dx = 0; dx < width; dx++)
				{
					for (int dy = 0; dy < height; dy++)
					{
						if (occupiedPositions.Contains(new Vector2I(x + dx, y + dy)))
						{
							areaIsFree = false;
							break;
						}
					}
					if (!areaIsFree) break;
				}
			
				if (areaIsFree)
				{
					available.Add(position);
				}
			}
		}
		
		return available;
	}
	#endregion

	#region Update Helper Methods
	/// <summary>
	/// Updates the pulse animation and rotation for the egg. The pulse effect modifies the egg's
	/// scale based on a sinusoidal pattern, creating a growing and shrinking effect. Additionally,
	/// when the combo mode is active, the egg rotates to enhance the animation.
	/// </summary>
	/// <param name="delta">The time elapsed since the last frame, in seconds.</param>
	/// <param name="isCombo">Indicates whether the combo mode is currently active.</param>
	public void UpdateEggPulse(double delta, bool isCombo)
	{
		if (EggNode == null) return;

		_eggPulseTime += delta;
		const float cycleTime = (float)2.0;
		float pulseFactor = (float)(1.0 + 0.25 * Math.Sin(2.0 * Math.PI * _eggPulseTime / cycleTime));
		EggNode.Scale = new Vector2(pulseFactor, pulseFactor);
		if (isCombo)
		{
			EggNode.Rotation = (float)(Math.PI * _eggPulseTime / cycleTime);
		}
		else
		{
			EggNode.Rotation = 0;
		}
	}

	/// <summary>
	/// Updates the pulse animation for all items. This method adjusts the scaling effect
	/// based on a sinusoidal animation pattern. The pulse effect is only applied when
	/// the combo mode is active.
	/// </summary>
	/// <param name="delta">The elapsed time interval since the last frame, in seconds.</param>
	/// <param name="isCombo">A boolean value indicating whether the combo mode is currently active.</param>
	public void UpdateItemPulse(double delta, bool isCombo)
	{
		_itemPulseTime += delta;
		const float cycleTime = (float)3.0;
		if (!isCombo) return;
		
		double variance = 0.0;
		foreach (var item in ItemNodes)
		{
			variance += 0.1;
			float pulseFactor = (float)(1.0 + 0.25 * Math.Sin(2.0 * Math.PI * _itemPulseTime / (cycleTime + variance)));
			item.Scale = new Vector2(pulseFactor, pulseFactor);
		}
	}

	/// <summary>
	/// Updates the score and combo point references used to track the player's progress
	/// in the game. These values are updated based on the current state of the gameplay.
	/// </summary>
	/// <param name="score">The updated score to be assigned to the CurrentScoreRef property.</param>
	/// <param name="comboX">The updated X component of combo points to be assigned to the CurrentComboPointsXRef property.</param>
	/// <param name="comboY">The updated Y component of combo points to be assigned to the CurrentComboPointsYRef property.</param>
	public void UpdateScoreReferences(double score, double comboX, double comboY)
	{
		CurrentScoreRef = score;
		CurrentComboPointsXRef = comboX;
		CurrentComboPointsYRef = comboY;
	}
	#endregion
}
