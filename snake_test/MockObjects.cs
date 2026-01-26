using Godot;
using System;
using System.Collections.Generic;
using Snake.Scripts;
using Snake.Scripts.Interfaces;

namespace SnakeTest;

public class MockSnakeManager : ISnakeManager
{
    public bool ControlsReversed { get; set; }
    public List<Vector2I> OldData { get; set; } = new List<Vector2I>();
    public List<Vector2I> SnakeData { get; set; } = new List<Vector2I> { new Vector2I(0,0) };
    public void AddSegment(Vector2I position) { }
}

public class MockScoreManager : IScoreManager
{
    public double Score { get; private set; }
    public double ComboPointsX { get; set; }
    public double ComboPointsY { get; set; }
    public int ComboTally { get; set; }
    public bool IsInCombo { get; private set; }

    public void Reset()
    {
        Score = 0;
        ComboPointsX = 0;
        ComboPointsY = 0;
        ComboTally = 0;
        IsInCombo = false;
    }

    public void AddScore(double amount)
    {
        Score += amount;
        Score = Math.Round(Score, 0, MidpointRounding.AwayFromZero);
    }

    public void SetScore(double amount)
    {
        Score = amount;
        Score = Math.Round(Score, 0, MidpointRounding.AwayFromZero);
    }

    public void IncrementComboTally()
    {
        ComboTally++;
    }

    public void StartCombo()
    {
        if (ComboTally < 7)
        {
            ComboTally = 7;
        }
        IsInCombo = true;
        ComboPointsX = Math.Max(1, ComboTally);
        ComboPointsY = 1;
    }

    public void EndCombo()
    {
        if (IsInCombo)
        {
            double comboPoints = (ComboPointsX * ComboPointsY);
            Score += comboPoints > 0 ? comboPoints : Math.Min(ComboPointsX, ComboPointsY);
            Score = Math.Round(Score, 0, MidpointRounding.AwayFromZero);
        }
        
        IsInCombo = false;
        ComboTally = 0;
        ComboPointsX = 0;
        ComboPointsY = 0;
    }

    public void CancelCombo()
    {
        IsInCombo = false;
        ComboPointsX = 0;
        ComboPointsY = 0;
    }

    public void UpdateComboPointsX(double amount)
    {
        ComboPointsX = amount;
    }

    public void UpdateComboPointsY(double amount)
    {
        ComboPointsY = amount;
    }
}

public class MockHealthManager : IHealthManager
{
    public int Lives { get; private set; }
    public List<Node2D> HealthNodes { get; } = new();
    public List<Vector2I> HealthData { get; } = new();
    private const int HealthCapacity = 6;

    public void Reset()
    {
        HealthNodes.Clear();
        HealthData.Clear();
        Lives = 0;
    }

    public void Initialize(Vector2 startPosition, int cellPixelSize, PackedScene snakeSegmentPs)
    {
        Reset();
        var test = new Vector2I((int)(startPosition.X + 8), (int)(startPosition.Y + 10));
        for (int i = 0; i < HealthCapacity; i++)
        {
            AddHealthSegment(test + new Vector2I((int)(i * cellPixelSize * 1.5), 0), snakeSegmentPs);
        }
        Lives = HealthNodes.Count;
    }

    private void AddHealthSegment(Vector2I position, PackedScene snakeSegmentPs)
    {
        HealthData.Add(position);
        // We can't really instantiate a real PackedScene/Node2D easily without Godot,
        // so we might need to mock Node2D or just use null if we don't access it.
        // For testing logic, we can just add a dummy Node2D if we are in a Godot-linked test,
        // but here we are in a unit test.
        // Let's use a dummy node if we can, or just null and be careful.
        // Actually, Node2D also inherits from GodotObject, so it might crash too.
        HealthNodes.Add(null); 
    }

    public void DeductHealth()
    {
        if (HealthNodes.Count == 0) return;

        HealthData.RemoveAt(HealthData.Count - 1);
        HealthNodes.RemoveAt(HealthNodes.Count - 1);

        if (Lives > 1)
        {
            Lives--;
        }
        else
        {
            Lives = 0;
            // Emit game over
        }
    }
}
