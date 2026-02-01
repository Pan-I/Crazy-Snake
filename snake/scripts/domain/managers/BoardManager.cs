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

namespace Snake.Scripts.Domain.Managers;

/// <summary>
/// Manages the board for the game, including its dimensions and functionality for checking board boundaries.
/// </summary>
public partial class BoardManager : GodotObject
{
    #region Properties
    /// <summary>
    /// The size of each cell on the board, in grid units (squares, columns, rows, etc.).
    /// </summary>
    public int BoardCellSize { get; private set; } = 30;
    /// <summary>
    /// The size of each cell on the board, in pixels.
    /// </summary>
    public int CellPixelSize { get; private set; } = 30;
    #endregion

    #region Private Boundaries
    private float BoardLeft { get; set; }
    private float BoardTop { get; set; }
    private float BoardRight { get; set; }
    private float BoardBottom { get; set; }
    #endregion

    #region Initialization
    /// <summary>
    /// Initializes the board manager with the specified board position.
    /// </summary>
    /// <param name="boardPosition"></param>
    public void SetBoardBoundaries(Vector2 boardPosition)
    {
        BoardLeft = boardPosition.X - ((BoardCellSize * CellPixelSize) / 2);
        BoardRight = boardPosition.X + ((BoardCellSize * CellPixelSize) / 2);
        BoardTop = boardPosition.Y - ((BoardCellSize * CellPixelSize) / 2);
        BoardBottom = boardPosition.Y + ((BoardCellSize * CellPixelSize) / 2);
    }
    #endregion

    #region Boundary Checks
    /// <summary>
    /// Checks if the specified position is outside the bounds of the board.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsOutOfBounds(Vector2I position)
    {
        return (position.X * CellPixelSize) < BoardLeft || 
               ((position.X + 1) * CellPixelSize) > BoardRight || 
               ((position.Y + 1) * CellPixelSize) < BoardTop || 
               ((position.Y + 4) * CellPixelSize) > BoardBottom;
    }
    #endregion
}
