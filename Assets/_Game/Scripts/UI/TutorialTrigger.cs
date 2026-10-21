using System.Collections;
using UnityEngine;

namespace DevGames.UI
{
    public sealed class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] GameplayHUD hud;
        [SerializeField, TextArea] string message;
        [SerializeField, Min(0.5f)] float duration = 4f;
        bool shown;

        public void Configure(GameplayHUD gameplayHud, string tutorialMessage)
        {
            hud = gameplayHud;
            message = tutorialMessage;
        }

        void OnTriggerEnter(Collider other)
        {
            if (shown || !other.CompareTag("Player")) return;
            shown = true;
            StartCoroutine(Show());
        }

        IEnumerator Show()
        {
            hud.ShowPrompt(message);
            yield return new WaitForSeconds(duration);
            hud.HidePrompt();
        }
    }
}

