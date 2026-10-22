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
        GameObject promptBackground;

        /// <summary>Connects the timer and tutorial caption created by the scene builder.</summary>
        /// <param name="timer">Text updated with the elapsed level time.</param>
        /// <param name="prompt">Text used to display contextual tutorial captions.</param>
        public void Configure(Text timer, Text prompt)
        {
            timerText = timer;
            promptText = prompt;
            promptBackground = promptText != null && promptText.transform.parent != null
                ? promptText.transform.parent.gameObject
                : null;
            if (promptBackground != null) promptBackground.SetActive(false);
        }

        /// <summary>Refreshes the timer every frame while a level session is active.</summary>
        void Update()
        {
            if (timerText == null || LevelSession.Current == null) return;
            timerText.text = ProgressStore.FormatMilliseconds(Mathf.RoundToInt(LevelSession.Current.ElapsedSeconds * 1000f));
        }

        /// <summary>Displays a tutorial message together with its high-contrast background.</summary>
        /// <param name="message">Instruction presented to the player.</param>
        public void ShowPrompt(string message)
        {
            if (promptText != null) promptText.text = message;
            if (promptBackground != null) promptBackground.SetActive(true);
        }

        /// <summary>Clears the tutorial message and hides its background.</summary>
        public void HidePrompt()
        {
            if (promptText != null) promptText.text = string.Empty;
            if (promptBackground != null) promptBackground.SetActive(false);
        }
    }
}

