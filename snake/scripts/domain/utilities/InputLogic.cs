namespace Snake.Scripts.Domain.Utilities;

public static class InputLogic
{
    public static string GetEffectiveAction(string pressedAction, bool reversed)
    {
        if (!reversed) return pressedAction;
        
        return pressedAction switch
        {
            "move_up" => "move_down",
            "move_down" => "move_up",
            "move_left" => "move_right",
            "move_right" => "move_left",
            _ => pressedAction
        };
    }
}
