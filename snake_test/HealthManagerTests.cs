using NUnit.Framework;
using Godot;
using Snake.Scripts.Interfaces;
using System.Collections.Generic;

namespace SnakeTest;

[TestFixture]
public class HealthManagerTests
{
    private IHealthManager _healthManager;

    [SetUp]
    public void SetUp()
    {
        _healthManager = new MockHealthManager();
    }

    [Test]
    public void TestInitialState()
    {
        Assert.That(_healthManager.Lives, Is.EqualTo(0));
        Assert.That(_healthManager.HealthNodes, Is.Empty);
        Assert.That(_healthManager.HealthData, Is.Empty);
    }

    [Test]
    public void TestInitialize()
    {
        _healthManager.Initialize(new Vector2(0, 0), 16, null);
        Assert.That(_healthManager.Lives, Is.EqualTo(6));
        Assert.That(_healthManager.HealthNodes.Count, Is.EqualTo(6));
        Assert.That(_healthManager.HealthData.Count, Is.EqualTo(6));
        
        // Check first health segment position logic
        // (int)(0 + 8), (int)(0 + 10) = (8, 10)
        Assert.That(_healthManager.HealthData[0], Is.EqualTo(new Vector2I(8, 10)));
    }

    [Test]
    public void TestDeductHealth()
    {
        _healthManager.Initialize(new Vector2(0, 0), 16, null);
        
        _healthManager.DeductHealth();
        Assert.That(_healthManager.Lives, Is.EqualTo(5));
        Assert.That(_healthManager.HealthNodes.Count, Is.EqualTo(5));
        
        // Deduct until game over
        for (int i = 0; i < 5; i++)
        {
            _healthManager.DeductHealth();
        }
        
        Assert.That(_healthManager.Lives, Is.EqualTo(0));
        Assert.That(_healthManager.HealthNodes.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestReset()
    {
        _healthManager.Initialize(new Vector2(0, 0), 16, null);
        _healthManager.Reset();
        
        Assert.That(_healthManager.Lives, Is.EqualTo(0));
        Assert.That(_healthManager.HealthNodes, Is.Empty);
        Assert.That(_healthManager.HealthData, Is.Empty);
    }
}
