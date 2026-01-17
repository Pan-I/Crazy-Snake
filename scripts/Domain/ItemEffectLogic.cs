namespace Snake.scripts.Domain;

public class ItemEffectLogic
{
    private readonly ISnakeManager _snake;
    private readonly string _rottenEggPath;
    private readonly string _dewDropPath;

    public ItemEffectLogic(ISnakeManager snake, string rottenEggPath, string dewDropPath)
    {
        _snake = snake;
        _rottenEggPath = rottenEggPath;
        _dewDropPath = dewDropPath;
    }

    public void Apply(string sceneFilePath)
    {
        if (sceneFilePath == _rottenEggPath)
        {
            _snake.ControlsReversed = true;
        }
        else if (sceneFilePath == _dewDropPath)
        {
            _snake.ControlsReversed = false;
        }
    }
}
