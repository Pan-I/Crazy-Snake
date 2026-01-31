using Snake.Scripts.Domain.Utilities;

namespace SnakeTest;

[TestFixture]
public class SnakeControlTests
{
	[Test]
	public void TestRottenEggReversesControls()
	{
		var mockSnake = new MockSnakeManager();
		var logic = new ItemEffectLogic(mockSnake, "res://scenes/RottenEgg.tscn", "res://scenes/DewDrop.tscn");
		
		logic.Apply("res://scenes/RottenEgg.tscn");
		
		Assert.That(mockSnake.ControlsReversed, Is.True);
	}

	[Test]
	public void TestDewDropResetsControls()
	{
		var mockSnake = new MockSnakeManager { ControlsReversed = true };
		var logic = new ItemEffectLogic(mockSnake, "res://scenes/RottenEgg.tscn", "res://scenes/DewDrop.tscn");
		
		logic.Apply("res://scenes/DewDrop.tscn");
		
		Assert.That(mockSnake.ControlsReversed, Is.False);
	}

	[Test]
	public void TestNormalInput()
	{
		Assert.That(InputLogic.GetEffectiveAction("move_up", false), Is.EqualTo("move_up"));
		Assert.That(InputLogic.GetEffectiveAction("move_down", false), Is.EqualTo("move_down"));
	}

	[Test]
	public void TestReversedInput()
	{
		Assert.That(InputLogic.GetEffectiveAction("move_up", true), Is.EqualTo("move_down"));
		Assert.That(InputLogic.GetEffectiveAction("move_down", true), Is.EqualTo("move_up"));
		Assert.That(InputLogic.GetEffectiveAction("move_left", true), Is.EqualTo("move_right"));
		Assert.That(InputLogic.GetEffectiveAction("move_right", true), Is.EqualTo("move_left"));
	}
}
