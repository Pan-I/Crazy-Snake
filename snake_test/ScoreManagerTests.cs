using NUnit.Framework;
using Snake.Scripts.Interfaces;
using System;

namespace SnakeTest;

[TestFixture]
public class ScoreManagerTests
{
    private IScoreManager _scoreManager;

    [SetUp]
    public void SetUp()
    {
        _scoreManager = new MockScoreManager();
    }

    [Test]
    public void TestInitialState()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.Score, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsY, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboTally, Is.EqualTo(0));
            Assert.That(_scoreManager.IsInCombo, Is.False);
        }
    }

    [Test]
    public void TestAddScore()
    {
        _scoreManager.AddScore(10.5);
        Assert.That(_scoreManager.Score, Is.EqualTo(11));
        
        _scoreManager.AddScore(5.4);
        Assert.That(_scoreManager.Score, Is.EqualTo(16));
    }

    [Test]
    public void TestSetScore()
    {
        _scoreManager.SetScore(100.7);
        Assert.That(_scoreManager.Score, Is.EqualTo(101));
    }

    [Test]
    public void TestReset()
    {
        _scoreManager.AddScore(100);
        _scoreManager.IncrementComboTally();
        _scoreManager.StartCombo();
        
        _scoreManager.Reset();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.Score, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsY, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboTally, Is.EqualTo(0));
            Assert.That(_scoreManager.IsInCombo, Is.False);
        }
    }

    [Test]
    public void TestComboLogic()
    {
        // Increment tally
        _scoreManager.IncrementComboTally();
        Assert.That(_scoreManager.ComboTally, Is.EqualTo(1));

        // Start combo (tally < 7 should be bumped to 7)
        _scoreManager.StartCombo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.IsInCombo, Is.True);
            Assert.That(_scoreManager.ComboTally, Is.EqualTo(7));
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(7));
            Assert.That(_scoreManager.ComboPointsY, Is.EqualTo(1));
        }

        // Update combo points
        _scoreManager.UpdateComboPointsX(10);
        _scoreManager.UpdateComboPointsY(2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(10));
            Assert.That(_scoreManager.ComboPointsY, Is.EqualTo(2));
        }

        // End combo
        _scoreManager.EndCombo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.IsInCombo, Is.False);
            Assert.That(_scoreManager.Score, Is.EqualTo(20)); // 10 * 2
            Assert.That(_scoreManager.ComboTally, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsY, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestStartComboWithHighTally()
    {
        for (int i = 0; i < 10; i++) _scoreManager.IncrementComboTally();
        _scoreManager.StartCombo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.ComboTally, Is.EqualTo(10));
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(10));
        }
    }

    [Test]
    public void TestCancelCombo()
    {
        _scoreManager.StartCombo();
        _scoreManager.UpdateComboPointsX(10);
        _scoreManager.UpdateComboPointsY(5);
        
        _scoreManager.CancelCombo();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_scoreManager.IsInCombo, Is.False);
            Assert.That(_scoreManager.Score, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsX, Is.EqualTo(0));
            Assert.That(_scoreManager.ComboPointsY, Is.EqualTo(0));
        }
    }
}
