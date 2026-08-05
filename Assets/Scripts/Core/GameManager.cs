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

    private void Awake()
    {
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
        timerUI.UpdateTimer(timer.RemainingTime);
    }

    private void OnFinishGame()
    {
        EndGame?.Invoke();
    }
}