using System;
using UI;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float gameTimerDuration;
    
    [Header("References")]
    [SerializeField] private TimerUI timerUI;
    
    public event Action StartGame; 
    public event Action EndGame;
    
    private GameTimer timer;
    private GameSessionData sessionData;

    private void Awake()
    {
        sessionData = GameSessionData.GetOrCreate();
        if (!sessionData.IsGameInProgress)
        {
            sessionData.BeginNewGame();
        }

        timer = new GameTimer(gameTimerDuration);
        timer.OnFinish += OnFinishGame;
        EndGame += timer.Pause;
        StartGame += timer.Start;
    }
    
    private void Start()
    {
        StartGame?.Invoke();
    }

    private void Update()
    {
        timer.Update();

        if (timerUI != null)
        {
            timerUI.UpdateTimer(timer.RemainingTime);
        }
    }

    private void OnFinishGame()
    {
        EndGame?.Invoke();
        sessionData.CompleteGame();
        SceneNavigator.LoadResultScene();
    }

    public void RegisterSwipe() => sessionData.RegisterSwipe();

    public void AddScore(int amount) => sessionData.AddScore(amount);

    public void SetScore(int score) => sessionData.SetScore(score);
}
