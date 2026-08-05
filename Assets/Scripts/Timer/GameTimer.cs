
using System;
using UnityEngine;

public class GameTimer
{
    public float Duration { get; private set; }
    public float RemainingTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    public event Action<float> OnTick;
    public event Action OnFinish;

    public GameTimer(float duration)
    {
        Duration = duration;
        RemainingTime = duration;
    }

    public void Start()
    {
        RemainingTime = Duration;
        IsRunning = true;
        IsPaused = false;
    }

    public void Update()
    {
        if (!IsRunning || IsPaused) return;

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
        }
        
        OnTick?.Invoke(RemainingTime);
        
        if (RemainingTime <= 0)
        {
            IsRunning = false;
            OnFinish?.Invoke();
        }
    }
    
    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;

    public void Reset(float newDuration)
    {
        Duration = newDuration;
        RemainingTime = newDuration;
        IsRunning = false;
        IsPaused = false;
    }
}