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
using System.Diagnostics;
using Godot;

namespace Snake.scripts;

public partial class Items : Node
{
	private readonly Main _main;
	private readonly Snake _snake;
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
	internal Node2D LargeWallNode;
	private Node2D _alienEggNode;
	private Node2D _iceEggNode;
	private Node2D _pillItemNode;
	private Node2D _discoEggNode;

	public Items(Main main, Snake snake)
	{
		_main = main;
		_snake = snake;
	}

	public Items()
	{
	}

	internal void LoadItems()
	{
		WallNode = _main.GetNode<Node2D>("ItemManager/Wall");
		_freshEggNode = _main.GetNode<Node2D>("ItemManager/FreshEgg");
		_ripeEggNode = _main.GetNode<Node2D>("ItemManager/RipeEgg");
		_rottenEggNode = _main.GetNode<Node2D>("ItemManager/RottenEgg");
		_mushroomNode = _main.GetNode<Node2D>("ItemManager/Mushroom");
		_shinyEggNode = _main.GetNode<Node2D>("ItemManager/ShinyEgg");
		_skullNode = _main.GetNode<Node2D>("ItemManager/Skull");
		_dewDropNode = _main.GetNode<Node2D>("ItemManager/DewDrop");
		_lavaEggNode = _main.GetNode<Node2D>("ItemManager/LavaEgg");
		_frogNode = _main.GetNode<Node2D>("ItemManager/Frog");
		_alienEggNode = _main.GetNode<Node2D>("ItemManager/AlienEgg");
		_iceEggNode = _main.GetNode<Node2D>("ItemManager/IceEgg");
		_pillItemNode = _main.GetNode<Node2D>("ItemManager/Pill");
		_discoEggNode = _main.GetNode<Node2D>("ItemManager/DiscoEgg");
		LargeWallNode = _main.GetNode<Node2D>("ItemManager/LargeWall");
	}
	
	internal void SetItemRates()
	{
		_itemRates = new Dictionary<int, List<Node2D>>
		{
			{ 1, new List<Node2D> { WallNode, _freshEggNode } },
			{ 2, new List<Node2D> { _ripeEggNode } },
			{ 3, new List<Node2D> { _rottenEggNode } },
			{ 5, new List<Node2D> { _mushroomNode } },
			{ 8, new List<Node2D> { _shinyEggNode } },
			{ 13, new List<Node2D> { _skullNode, LargeWallNode } },
			{ 21, new List<Node2D> { _dewDropNode } },
			{ 34, new List<Node2D> { _lavaEggNode } },
			{ 55, new List<Node2D> { _frogNode } },
			{ 89, new List<Node2D> { _alienEggNode } },
			{ 144, new List<Node2D> { _iceEggNode } },
			{ 233, new List<Node2D> { _pillItemNode } },
			{ 377, new List<Node2D> { _discoEggNode } }
		};
	}


	internal void MoveEgg()
	{
		do { EggPosition = RandomPlacement(); }
		while ( CheckWallTrap() );
		_main.GetNode<Node2D>("Egg").Position = EggPosition * _main.CellPixelSize + new Vector2I(0, _main.CellPixelSize);
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

	private Vector2I RandomPlacement()
	{
		//TODO: possible parameter for introducing larger objects when being placed to avoid placing on top of other  Or does the large item hit in main cover this?
		Random rndm = new Random();
		HashSet<Vector2I> occupiedPositions = new HashSet<Vector2I>(_snake.SnakeData) { EggPosition };
		occupiedPositions.UnionWith(ItemsData);
		occupiedPositions.UnionWith(WallsData);
		occupiedPositions.UnionWith(LargeWallsData);

		int tryCounter = 0;
		
		Vector2I itemPlacement;
		do
		{
			//generate a new coordinate spot
			tryCounter++;
			if (tryCounter == 90000) //if there have been this many attempts to place the item, something is wrong.
			{
				_main.EndGame(); //TODO: for REVIEW place a stopping here and see if this can be easily triggered on a long-lasting game. 
			}
			itemPlacement = new Vector2I(rndm.Next(1, _main.BoardCellSize + 1), rndm.Next(3, _main.BoardCellSize - 4)); 
			//TODO: more dynamic way to factor for GUI frame in offsets?
			
		} while ((occupiedPositions.Contains(itemPlacement) || //Don't place on an occupied position.
				  _main.CheckLargeItemHit(itemPlacement) || //Don't place on large 
				  IsWithinRadius(itemPlacement, _snake.SnakeData[0], 3) || //Don't place too close to snake head.
				  CheckWithinRadius(itemPlacement, _snake.SnakeData, 1) || //Don't place directly next to entire body.
				  !IsWithinRadius(itemPlacement, _snake.SnakeData[^1], 20) //Place around the tail. Temporary rule? I kind of like it.
				 ));

		return itemPlacement;

		//TODO: Needs a better implementation. (*Why? Don't remember what is wrong with this.*)
		bool CheckWithinRadius(Vector2I itemPlacement, List<Vector2I> listCoords, int radius)
		{
			bool withinRadius = false;
			foreach (var spot in listCoords)
			{
				if (IsWithinRadius(itemPlacement, spot, radius))
				{
					withinRadius = true;
					return withinRadius;
				}
			}
			return withinRadius;
		}

		// Helper function to check if a position is within a given radius
		bool IsWithinRadius(Vector2I position, Vector2I center, int radius)
		{
			int dx = Math.Abs(position.X - center.X);
			int dy = Math.Abs(position.Y - center.Y);
			return dx <= radius && dy <= radius;
		}
	}

	private void ReplaceItem()
	{
		//do nothing;
	}

	internal void GenerateFromItemLookup()
	{
		foreach (var rule in _itemRates)
		{
			if (Tally % rule.Key == 0)
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
		newItem.Position = _newItemPosition * _main.CellPixelSize + new Vector2I(0, _main.CellPixelSize);
		_main.AddChild(newItem);
		if (newItem.SceneFilePath == WallNode.SceneFilePath)
		{
			WallNodes.Add(newItem);
			WallsData.Add(_newItemPosition);
		}
		else if (newItem.SceneFilePath == LargeWallNode.SceneFilePath)
		{
			LargeWallNodes.Add(newItem);
			LargeWallsData.Add(_newItemPosition);
		}
		else
		{
			ItemNodes.Add(newItem);
			ItemsData.Add(_newItemPosition);
		}
	}

	internal void ItemResult(Node2D item, int i)
	//TODO: refactor, out Score, instead of manipulating from here.
	{
				
		if (item.SceneFilePath != WallNode.SceneFilePath && item.SceneFilePath != LargeWallNode.SceneFilePath)
		{
			item.QueueFree();
			ItemsData.RemoveAt(i);
			ItemNodes.RemoveAt(i);
		}

		if (item.SceneFilePath == WallNode.SceneFilePath 
			|| item.SceneFilePath == LargeWallNode.SceneFilePath) 
		{
			_main.EndGame();
		}
		//Good eggs
		if (item.SceneFilePath == _freshEggNode.SceneFilePath)
		{
			_main.Score += 25;
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if (item.SceneFilePath == _ripeEggNode.SceneFilePath)
		{
			_main.Score *= 1.5;
			_snake.AddSegment(_snake.OldData[^1]);
		}

		if (item.SceneFilePath == _shinyEggNode.SceneFilePath)
		{
			_main.Score *= 2;
			_snake.AddSegment(_snake.OldData[^1]);
		}

		if (item.SceneFilePath == _alienEggNode.SceneFilePath)
		{
			_main.Score *= 10;
			_snake.AddSegment(_snake.OldData[^1]);
		}

		if (item.SceneFilePath == _discoEggNode.SceneFilePath)
		{
			_main.Score *= _main.Score;
			_snake.AddSegment(_snake.OldData[^1]);
		}
		//Bad eggs
		if (item.SceneFilePath == _rottenEggNode.SceneFilePath)
		{
			_main.Score -= Math.Max(150, _main.Score * 0.5);
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if(item.SceneFilePath == _lavaEggNode.SceneFilePath)
		{
			_main.Score  -= Math.Max(375, _main.Score * 0.75);
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if (item.SceneFilePath == _iceEggNode.SceneFilePath)
		{
			_main.Score = Math.Min(_main.Score - 10000, Math.Sqrt(Math.Abs(_main.Score)));
			_snake.AddSegment(_snake.OldData[^1]);
		}
		//Complex Scorers
		if (item.SceneFilePath == _mushroomNode.SceneFilePath)
		{
			// If the score is negative, set it to a fixed value of -1.
			// Otherwise, apply a slight exponential increase (power of 1.05) to the absolute value of the score.
			_main.Score = (_main.Score < 0) ? (-1) : Math.Pow(Math.Abs(_main.Score), 1.05);
		}
		if (item.SceneFilePath == _dewDropNode.SceneFilePath)
		{
			// Set the score to its absolute value, effectively removing any negative sign.
			_main.Score = Math.Abs(_main.Score);
		}
		if (item.SceneFilePath == _frogNode.SceneFilePath)
		{
			// If the score is negative, reduce it further by subtracting an exponential value (power of 1.15).
			// If the score is positive, apply an exponential increase (power of 1.15).
			_main.Score = (_main.Score < 0) ? (Math.Abs(_main.Score) - Math.Pow(Math.Abs(_main.Score), 1.15)) 
				: Math.Pow(Math.Abs(_main.Score), 1.15);
		}
		if (item.SceneFilePath == _pillItemNode.SceneFilePath)
		{
			// If the score is negative, apply a larger exponential reduction (power of 1.5) to the absolute value.
			// If the score is positive, apply an exponential increase (power of 1.5).
			_main.Score = (_main.Score < 0) ? (0 - Math.Pow(Math.Abs(_main.Score), 1.5)) 
				: Math.Pow(Math.Abs(_main.Score), 1.5);
		}
		if (item.SceneFilePath == _skullNode.SceneFilePath)
		{
			//Tally = 0;
			
			Random rnd = new Random();
			int result;
			do { result = rnd.Next(-1, 7); } while (result % 2 == 0); //results can only be odd
			
			switch (result)
			{
				case -1:
					_main.Score = -9999;
					break;
				case 1:
					_main.Score -= -9999;
					break;
				case 3:
					_main.Score = 0;
					break;
				case 5:
					_main.Score += 9999;
					break;
				default:
					_main.Score = 9999;
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

	public void Reset()
	{
		Tally = 0;
		if (ItemNodes != null)
		{
			foreach (Node2D node in ItemNodes)
			{
				node.QueueFree();
			}
		}
		if (WallNodes != null)
		{
			foreach (Node2D node in WallNodes)
			{
				node.QueueFree();
			}
		}
		if (LargeWallNodes != null)
		{
			foreach (Node2D node in LargeWallNodes)
			{
				node.QueueFree();
			}
		}

		ItemNodes = new List<Node2D>();
		ItemsData = new List<Vector2I>();
		WallNodes = new List<Node2D>();
		WallsData = new List<Vector2I>();
		LargeWallNodes = new List<Node2D>();
		LargeWallsData = new List<Vector2I>();
		MoveEgg();
	}
}
