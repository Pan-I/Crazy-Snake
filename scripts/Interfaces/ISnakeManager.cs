using Godot;
using System.Collections.Generic;

namespace Snake.scripts;

public interface ISnakeManager
{
    bool ControlsReversed { get; set; }
    void AddSegment(Vector2I position);
    List<Vector2I> OldData { get; }
    List<Vector2I> SnakeData { get; }
}
