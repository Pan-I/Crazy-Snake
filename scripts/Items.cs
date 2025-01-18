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
	internal List<Node2D> LargeItemNodes;
	internal List<Vector2I> LargeItemsData;
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
		_largeWallNode = _main.GetNode<Node2D>("ItemManager/LargeWall");
	}
	
	internal void SetItemRates()
	{
		_itemRates = new Dictionary<int, List<Node2D>>
		{
			{ 1, new List<Node2D> { WallNode, _freshEggNode } },
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
	}


	internal void MoveEgg()
	{
		EggPosition = RandomPlacement();
		_main.GetNode<Node2D>("Egg").Position = EggPosition * _main.CellPixelSize + new Vector2I(0, _main.CellPixelSize);
	}

	private Vector2I RandomPlacement()
	{
		//TODO: possible parameter for introducing larger objects when being placed to avoid placing on top of other items.
		//Large walls have been spotted spawning on snake segments, and other items (https://github.com/users/Pan-I/projects/6/views/3?pane=issue&itemId=93374486&issue=Pan-I%7CCrazy-Snake%7C9)
		Random rndm = new Random();
		HashSet<Vector2I> occupiedPositions = new HashSet<Vector2I>(_snake.SnakeData) { EggPosition };
		occupiedPositions.UnionWith(ItemsData);
		occupiedPositions.UnionWith(LargeItemsData); //TODO: Needs a way to fill the other cells. Otherwise items may place in non-origin cells of larger items.
		//
		
		Vector2I itemPlacement;
		do
		{
			//generate a new coordinate spot
			itemPlacement = new Vector2I(rndm.Next(0, _main.BoardCellSize - 1), rndm.Next(3, _main.BoardCellSize - 1));
		} while (occupiedPositions. Count < 899 && //TODO: this 899 limit doesn't account for large items either.
				 (occupiedPositions.Contains(itemPlacement) || //Don't place on an occupied position.
				  _main.CheckLargeItemHit(itemPlacement) || //Don't place on large items. //TODO: Doesn't seem to work.
				  IsWithinRadius(itemPlacement, _snake.SnakeData[0], 3) || //Don't place too close to snake head.
				  CheckWithinRadius(itemPlacement, _snake.SnakeData, 1) || //Don't place anywhere near entire body.
				  !IsWithinRadius(itemPlacement, _snake.SnakeData[^1], 20) //Place within 7 cells of the tail. Temporary rule? I kind of like it.
				 ));

		return itemPlacement;

		//TODO: Needs a better implementation.
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
		if (newItem.SceneFilePath == _largeWallNode.SceneFilePath)
		{
			LargeItemNodes.Add(newItem);
			LargeItemsData.Add(_newItemPosition);
		}
		else
		{
			ItemNodes.Add(newItem);
			ItemsData.Add(_newItemPosition);
		}
	}

	internal void ItemResult(Node2D item)
	{
		if (item.SceneFilePath == WallNode.SceneFilePath 
			|| item.SceneFilePath == _largeWallNode.SceneFilePath) 
		{
			_main.EndGame();
		}
		if (item.SceneFilePath == _freshEggNode.SceneFilePath)
			
		{
			_main.Score += 25;
			Debug.Assert(_snake != null, nameof(_snake) + " != null");
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
			_main.Score *= 5;
			_snake.AddSegment(_snake.OldData[^1]);
		}

		if (item.SceneFilePath == _discoEggNode.SceneFilePath)
		{
			_main.Score *= _main.Score;
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if (item.SceneFilePath == _rottenEggNode.SceneFilePath)
		{
			_main.Score -= 75;
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if(item.SceneFilePath == _lavaEggNode.SceneFilePath)
		{
			_main.Score /= 2;
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if (item.SceneFilePath == _iceEggNode.SceneFilePath)
		{
			_main.Score = Math.Sqrt(Math.Abs(_main.Score));
			_snake.AddSegment(_snake.OldData[^1]);
		}
		if (item.SceneFilePath == _mushroomNode.SceneFilePath)
		{
			_main.Score = Math.Pow(Math.Abs(_main.Score), 1.05);
		}
		if (item.SceneFilePath == _dewDropNode.SceneFilePath)
		{
			_main.Score = Math.Abs(_main.Score);
		}
		if (item.SceneFilePath == _pillItemNode.SceneFilePath)
		{
			_main.Score = Math.Pow(Math.Abs(_main.Score), 1.5);
		}
		if (item.SceneFilePath == _skullNode.SceneFilePath)
		{
			Tally = 0;
			
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
}
