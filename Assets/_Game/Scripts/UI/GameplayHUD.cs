using DevGames.Level;
using DevGames.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace DevGames.UI
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField] Text timerText;
        [SerializeField] Text promptText;

        public void Configure(Text timer, Text prompt)
        {
            timerText = timer;
            promptText = prompt;
        }

        void Update()
        {
            if (timerText == null || LevelSession.Current == null) return;
            timerText.text = ProgressStore.FormatMilliseconds(Mathf.RoundToInt(LevelSession.Current.ElapsedSeconds * 1000f));
        }

        public void ShowPrompt(string message) { if (promptText != null) promptText.text = message; }
        public void HidePrompt() { if (promptText != null) promptText.text = string.Empty; }
    }
}

