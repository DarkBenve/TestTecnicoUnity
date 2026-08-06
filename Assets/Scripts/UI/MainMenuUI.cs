using TMPro;
using UnityEngine;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text bestScoreText;

        private void Start()
        {
            int bestScore = GameSessionData.GetOrCreate().BestScore;
            bestScoreText.text = $"Best score: {bestScore}";
        }
    }
}
