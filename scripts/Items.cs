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

public partial class Items : Node
{
	private Main _main;
	internal Node2D WallNode;
	internal Vector2I EggPosition;
	internal int Tally;
	internal Dictionary<int, List<Node2D>> ItemRates;
	internal List<Node2D> ItemNodes;
	internal List<Vector2I> ItemsData;
	internal List<Vector2I> LargeItemsData;
	private Vector2I _newItemPosition;
	internal Node2D FreshEggNode;
	internal Node2D RipeEggNode;
	internal Node2D RottenEggNode;
	internal Node2D MushroomNode;
	internal Node2D ShinyEggNode;
	internal Node2D SkullNode;
	internal Node2D DewDropNode;
	internal Node2D LavaEggNode;
	internal Node2D FrogNode;
	internal Node2D LargeWallNode;
	internal Node2D AlienEggNode;
	internal Node2D IceEggNode;
	internal Node2D PillItemNode;
	internal Node2D DiscoEggNode;

	public Items(Main main)
	{
		_main = main;
	}

	internal void LoadItems()
	{
	}

	internal void SetItemRates()
	{
	}


	internal void MoveEgg()
	{
		EggPosition = RandomPlacement();
		_main.GetNode<Node2D>("Egg").Position = (EggPosition * _main.CellPixelSize) + new Vector2I(0, _main.CellPixelSize);
	}

	private Vector2I RandomPlacement()
	{
		//TODO: possible parameter for introducing larger objects when being placed to avoid placing on top of other items.
		//Large walls have been spotted spawning on snake segments, and other items (https://github.com/users/Pan-I/projects/6/views/3?pane=issue&itemId=93374486&issue=Pan-I%7CCrazy-Snake%7C9)
		Random rndm = new Random();
		HashSet<Vector2I> occupiedPositions = new HashSet<Vector2I>(_main.Snake.SnakeData);
		occupiedPositions.Add(EggPosition);
		occupiedPositions.UnionWith(ItemsData);
		occupiedPositions.UnionWith(LargeItemsData); //TODO: Needs a way to fill the other cells. Otherwise items may place in non-origin cells of larger items.
		//
		
		Vector2I itemPlacement;
		do
		{
			itemPlacement = new Vector2I(rndm.Next(0, _main.BoardCellSize - 1), rndm.Next(3, _main.BoardCellSize - 1));
		} while (occupiedPositions. Count < 899 && //TODO: this 899 limit doesn't account for large items either.
				 (occupiedPositions.Contains(itemPlacement) || //Don't place on an occupied position.
				  IsWithinRadius(itemPlacement, _main.Snake.SnakeData[0], 3) || //Don't place too close to snake head.
				  !IsWithinRadius(itemPlacement, _main.Snake.SnakeData[^1], 20) //Place within 7 cells of the tail. Temporary rule? I kind of like it.
				 ));
		// Helper function to check if a position is within a given radius
		bool IsWithinRadius(Vector2I position, Vector2I center, int radius)
		{
			int dx = Math.Abs(position.X - center.X);
			int dy = Math.Abs(position.Y - center.Y);
			return dx <= radius && dy <= radius;
		}
		
		return itemPlacement;
	}

	internal void GenerateFromItemLookup()
	{
		foreach (var rule in ItemRates)
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
		newItem.Position = (_newItemPosition * _main.CellPixelSize) + new Vector2I(0, _main.CellPixelSize);
		_main.AddChild(newItem);
		ItemNodes.Add(newItem);
		if (newItem.SceneFilePath == LargeWallNode.SceneFilePath)
		{
			LargeItemsData.Add(_newItemPosition);
		}
		else
		{
			ItemsData.Add(_newItemPosition);
		}
	}

	internal void ItemResult(Node2D item)
	{
		if (item.SceneFilePath == WallNode.SceneFilePath 
			|| item.SceneFilePath == LargeWallNode.SceneFilePath) 
		{
			_main.EndGame();
		}
		if (item.SceneFilePath == FreshEggNode.SceneFilePath)
			
		{
			_main.Score += 25;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}
		if (item.SceneFilePath == RipeEggNode.SceneFilePath)
		{
			_main.Score *= 1.5;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}

		if (item.SceneFilePath == ShinyEggNode.SceneFilePath)
		{
			_main.Score *= 2;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}

		if (item.SceneFilePath == AlienEggNode.SceneFilePath)
		{
			_main.Score *= 5;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}

		if (item.SceneFilePath == DiscoEggNode.SceneFilePath)
		{
			_main.Score *= _main.Score;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}
		if (item.SceneFilePath == RottenEggNode.SceneFilePath)
		{
			_main.Score -= 75;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}
		if(item.SceneFilePath == LavaEggNode.SceneFilePath)
		{
			_main.Score /= 2;
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}
		if (item.SceneFilePath == IceEggNode.SceneFilePath)
		{
			_main.Score = Math.Sqrt(Math.Abs(_main.Score));
			_main.Snake.AddSegment(_main.Snake.OldData[^1]);
		}
		if (item.SceneFilePath == MushroomNode.SceneFilePath)
		{
			_main.Score = Math.Pow(Math.Abs(_main.Score), 1.05);
		}
		if (item.SceneFilePath == DewDropNode.SceneFilePath)
		{
			_main.Score = Math.Abs(_main.Score);
		}
		if (item.SceneFilePath == PillItemNode.SceneFilePath)
		{
			_main.Score = Math.Pow(Math.Abs(_main.Score), 1.5);
		}
		if (item.SceneFilePath == SkullNode.SceneFilePath)
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
