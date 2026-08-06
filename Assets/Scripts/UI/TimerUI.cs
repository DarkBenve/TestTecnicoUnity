using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        private GameSessionData sessionData;
        private float remainingTime;
        private int gemScore;

        private void OnEnable()
        {
            sessionData = GameSessionData.GetOrCreate();
            gemScore = sessionData.CurrentScore;
            sessionData.ScoreChanged += UpdateGemScore;
            RefreshText();
        }

        private void OnDisable()
        {
            if (sessionData != null)
            {
                sessionData.ScoreChanged -= UpdateGemScore;
            }
        }

        public void UpdateTimer(float time)
        {
            remainingTime = time;
            RefreshText();
        }

        private void UpdateGemScore(int score)
        {
            gemScore = score;
            RefreshText();
        }

        private void RefreshText()
        {
            if (timerText != null)
            {
                timerText.text = $"Tempo: {Mathf.CeilToInt(remainingTime)}\nGemme: {gemScore}";
            }
        }
    }
}
