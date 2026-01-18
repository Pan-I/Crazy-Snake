using Godot;
using System.Collections.Generic;
using Snake.Scripts;
using Snake.Scripts.Interfaces;

namespace Snake.Tests;

public class MockSnakeManager : ISnakeManager
{
    public bool ControlsReversed { get; set; }
    public List<Vector2I> OldData { get; set; } = new List<Vector2I>();
    public List<Vector2I> SnakeData { get; set; } = new List<Vector2I> { new Vector2I(0,0) };
    public void AddSegment(Vector2I position) { }
}
