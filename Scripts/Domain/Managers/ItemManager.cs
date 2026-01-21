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
using Snake.Scripts.Domain.Utilities;
using Snake.Scripts.Interfaces;

namespace Snake.Scripts.Domain.Managers;

public partial class ItemManager : Node
{
	[Signal] public delegate void HealthDeductedRequestedEventHandler();
	[Signal] public delegate void ScoreChangedEventHandler(double amount, bool isDelta);
	[Signal] public delegate void ComboPointsXChangedEventHandler(double amount, bool isDelta);
	[Signal] public delegate void ComboPointsYChangedEventHandler(double amount, bool isDelta);
	[Signal] public delegate void ComboStartedEventHandler();
	[Signal] public delegate void ComboEndedEventHandler();
	[Signal] public delegate void ComboCancelledEventHandler();
	[Signal] public delegate void GameOverRequestedEventHandler();
	[Signal] public delegate void HudFlashRequestedEventHandler(int type);
	[Signal] public delegate void ItemSpawnedEventHandler(Node2D item);
	[Signal] public delegate void EggMovedEventHandler();
	[Signal] public delegate void BadFoodEatenEventHandler();
	[Signal] public delegate void SpecialEggEatenEventHandler(bool isInCombo);
	[Signal] public delegate void HighChimeRequestedEventHandler();
	
	private readonly ISnakeManager _snake;
	public int CellPixelSizeRef { get; set; }
	public int BoardCellSizeRef { get; set; }
	public Vector2I PlacementOffsetRef { get; set; }
	public Node2D EggNode { get; set; }
	public double CurrentScoreRef { get; set; }
	public double CurrentComboPointsXRef { get; set; }
	public double CurrentComboPointsYRef { get; set; }

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
	private Vector2I _offset;

	public ItemManager(ISnakeManager snake)
	{
		_snake = snake;
	}

	public ItemManager()
	{
	}

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

	private bool CheckWallTrap()
	{
		//TODO:Doesn't account for large walls, or larger perimeter traps... it should, but I wonder if it's necessary or worth it. A certain planned power-up might make it even less so
		var eggPosNord = new Vector2I(EggPosition.X, EggPosition.Y - 1);
		var eggPosEas = new Vector2I(EggPosition.X + 1, EggPosition.Y);
		var eggPosSud = new Vector2I(EggPosition.X, EggPosition.Y + 1);
		var eggPosWes = new Vector2I(EggPosition.X - 1, EggPosition.Y);
		if (WallsData.Contains(eggPosNord) && WallsData.Contains(eggPosEas) && WallsData.Contains(eggPosSud) &&
			WallsData.Contains(eggPosWes))
		{
			return true;
		}
		return false;
	}

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
			//TODO: more dynamic way to factor for GUI frame in offsets?
			
		} while ((!available.Contains(itemPlacement) || //Don't place on an occupied position.
				  CheckLargeItemHit(itemPlacement) || //Don't place on large 
				  IsWithinRadius(itemPlacement, _snake.SnakeData[0], 3) || //Don't place too close to the snake head.
				  CheckWithinRadius(itemPlacement, _snake.SnakeData, 1) || //Don't place directly next to the entire body.
				  !IsWithinRadius(itemPlacement, _snake.SnakeData[^1], 20) //Place around the tail. Temporary rule? I kind of like it.
				 ));

		return itemPlacement;

		//TODO: Needs a better implementation. (*Why? Don't remember what is wrong with this.*)
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

	public void EggEaten()
	{
		Tally++;
		MoveEgg();
		GenerateFromItemLookup();
	}

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

	public void UpdateItemPulse(double delta, bool isCombo)
	{
		const float cycleTime = (float)3.0;
		if (!isCombo) return;
		
		double variance = 0.0;
		foreach (var item in ItemNodes)
		{
			variance += 0.1;
			float pulseFactor = (float)(1.0 + 0.25 * Math.Sin(2.0 * Math.PI * _eggPulseTime / (cycleTime + variance)));
			item.Scale = new Vector2(pulseFactor, pulseFactor);
		}
	}

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

	public void UpdateScoreReferences(double score, double comboX, double comboY)
	{
		CurrentScoreRef = score;
		CurrentComboPointsXRef = comboX;
		CurrentComboPointsYRef = comboY;
	}

	public void SetCellPixelSizeRef(int boardCellPixelSize)
	{
		CellPixelSizeRef = boardCellPixelSize;
	}

	public void SetBoardCellSizeRef(int boardBoardCellSize)
	{
		BoardCellSizeRef = boardBoardCellSize;
	}

	public void SetPlacementOffset(Vector2I vector2I)
	{
		PlacementOffsetRef = vector2I;
	}

	public void InitializeEggNode(Node2D getNode)
	{
		EggNode	= getNode;
	}
}
