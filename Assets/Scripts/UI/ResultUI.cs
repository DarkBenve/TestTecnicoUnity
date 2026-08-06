using TMPro;
using UnityEngine;

namespace UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultText;

        private void Start()
        {
            GameSessionData data = GameSessionData.GetOrCreate();

            resultText.text =
                $"RISULTATI\n" +
                $"Punteggio: {data.CurrentScore}\n" +
                $"Swipe: {data.CurrentSwipeCount}\n" +
                $"Best score: {data.BestScore}\n\n" +
                "Tocca per tornare al menu";
        }
    }
}
