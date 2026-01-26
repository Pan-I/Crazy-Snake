using System.Collections.Generic;
using Godot;

namespace Snake.Scripts.Interfaces;

public interface IHealthManager
{
    int Lives { get; }
    List<Node2D> HealthNodes { get; }
    List<Vector2I> HealthData { get; }

    void Reset();
    void Initialize(Vector2 startPosition, int cellPixelSize, PackedScene snakeSegmentPs);
    void DeductHealth();
}
