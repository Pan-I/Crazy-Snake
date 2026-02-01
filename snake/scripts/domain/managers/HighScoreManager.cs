using Godot;
using NUnit.Framework.Constraints;
using Snake.Scripts.Utilities;

namespace Snake.Scripts.Domain.Managers;

public partial class HighScoreManager : GodotObject
{
    [Signal] public delegate void HighScoreChangedEventHandler(double score);
    
    private readonly ConfigUtility _config;
    //private JsonSerializer _serializer; //MEMO: Possible future implementation.

    private double HighScore { get; set; }
    public double ComboPointsX { get; set; }
    public double ComboPointsY { get; set; }
    public int ComboTally { get; set; }
    
    public HighScoreManager(ConfigUtility config)
    {
        _config = config;
        HighScore = _config.LoadLeaderboard();
    }

    public HighScoreManager()
    {
        HighScore = 0;
    }

    public void CheckHighScore(double score)
    {
        if (!(score > HighScore)) return;
        HighScore = score;
        SaveHighScore();
        EmitSignal(nameof(HighScoreChanged), HighScore);
    }
    
    public void SaveHighScore()
    {
        _config.SaveLeaderboard(HighScore);
    }
}
