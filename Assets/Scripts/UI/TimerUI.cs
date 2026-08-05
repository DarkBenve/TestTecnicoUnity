using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        public void UpdateTimer(float time)
        {
            timerText.text = Mathf.CeilToInt(time).ToString();
        }
    }
}