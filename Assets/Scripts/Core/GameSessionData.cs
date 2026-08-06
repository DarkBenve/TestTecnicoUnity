using System;
using UnityEngine;

public class GameSessionData : SingletonBehaviour<GameSessionData>
{
    private const string BestScoreKey = "BestScore";

    public int CurrentScore { get; private set; }
    public int CurrentSwipeCount { get; private set; }
    public int BestScore { get; private set; }
    public bool IsGameInProgress { get; private set; }

    public event Action<int> ScoreChanged;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    public static GameSessionData GetOrCreate()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject sessionObject = new GameObject(nameof(GameSessionData));
        return sessionObject.AddComponent<GameSessionData>();
    }

    public void BeginNewGame()
    {
        CurrentScore = 0;
        CurrentSwipeCount = 0;
        IsGameInProgress = true;
        ScoreChanged?.Invoke(CurrentScore);
    }

    public void RegisterSwipe()
    {
        if (IsGameInProgress)
        {
            CurrentSwipeCount++;
        }
    }

    public void AddScore(int amount)
    {
        if (IsGameInProgress)
        {
            CurrentScore = Mathf.Max(0, CurrentScore + amount);
            ScoreChanged?.Invoke(CurrentScore);
        }
    }

    public void SetScore(int score)
    {
        if (IsGameInProgress)
        {
            CurrentScore = Mathf.Max(0, score);
            ScoreChanged?.Invoke(CurrentScore);
        }
    }

    public void CompleteGame()
    {
        if (!IsGameInProgress)
        {
            return;
        }

        IsGameInProgress = false;

        if (CurrentScore <= BestScore)
        {
            return;
        }

        BestScore = CurrentScore;
        PlayerPrefs.SetInt(BestScoreKey, BestScore);
        PlayerPrefs.Save();
    }

    public void CancelGame()
    {
        IsGameInProgress = false;
    }

    public void ResetBestScore()
    {
        BestScore = 0;
        PlayerPrefs.DeleteKey(BestScoreKey);
        PlayerPrefs.Save();
    }
}
