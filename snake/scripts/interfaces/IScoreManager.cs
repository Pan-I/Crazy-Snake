namespace Snake.Scripts.Interfaces;

public interface IScoreManager
{
    double Score { get; }
    double ComboPointsX { get; set; }
    double ComboPointsY { get; set; }
    int ComboTally { get; set; }
    bool IsInCombo { get; }

    void Reset();
    void AddScore(double amount);
    void SetScore(double amount);
    void IncrementComboTally();
    void StartCombo();
    void EndCombo();
    void CancelCombo();
    void UpdateComboPointsX(double amount);
    void UpdateComboPointsY(double amount);
}
