
using Godot;
using System.Collections.Generic;

namespace Snake.Scripts.Utilities;
public partial class ConfigUtility : Node
{
    private const string SavePath = "user://game_settings.cfg";
    private const string LeaderBoardPath = "user://leaderboard.cfg";

    public void SaveSettings()
    {
        var config = new ConfigFile();
        
        // Store values under a section and a key
        config.SetValue("Audio", "volume", 0.8f);
        config.SetValue("Graphics", "fullscreen", true);
        config.SetValue("Player", "name", "Steve");

        // Save the ConfigFile to the specified path
        Error error = config.Save(SavePath);
        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to save configuration file: {error}");
        }
    }

    public void LoadSettings()
    {
        var config = new ConfigFile();
        // Load the ConfigFile from the specified path
        Error error = config.Load(SavePath);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to load configuration file: {error}");
            return;
        }

        // Retrieve values, providing a default value if the key doesn't exist
        float volume = (float)config.GetValue("Audio", "volume", 0.5f);
        bool fullscreen = (bool)config.GetValue("Graphics", "fullscreen", false);
        string playerName = (string)config.GetValue("Player", "name", "Guest");
        
        GD.Print($"Loaded settings: Volume={volume}, Fullscreen={fullscreen}, Name={playerName}");
    }
    
    public void SaveLeaderboard(double highScore)
    {
        
    }
    
    public double LoadLeaderboard()
    {
        return 10000;
    }
}