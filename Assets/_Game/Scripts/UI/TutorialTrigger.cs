using System.Collections;
using UnityEngine;

namespace DevGames.UI
{
    /// <summary>Área invisível que mostra uma dica na tela quando o jogador entra nela pela primeira vez.</summary>
    public sealed class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] GameplayHUD hud;
        [SerializeField, TextArea] string message;
        [SerializeField, Min(0.5f)] float duration = 4f;
        bool shown;

        /// <summary>Define o HUD e o texto da dica (usado pelo gerador da cena).</summary>
        /// <param name="gameplayHud">HUD que exibe a mensagem.</param>
        /// <param name="tutorialMessage">Texto da dica.</param>
        public void Configure(GameplayHUD gameplayHud, string tutorialMessage)
        {
            hud = gameplayHud;
            message = tutorialMessage;
        }

        /// <summary>Chamado quando algo entra na área.</summary>
        /// <param name="other">Collider que entrou.</param>
        /// <remarks>Só reage uma vez e só à tag "Player"; inicia a corrotina que mostra a dica.</remarks>
        void OnTriggerEnter(Collider other)
        {
            if (shown || !other.CompareTag("Player")) return;
            shown = true;
            StartCoroutine(Show());
        }

        /// <summary>Mostra a dica por alguns segundos e depois a esconde.</summary>
        /// <returns>Enumerador da corrotina.</returns>
        /// <remarks>Exibe o texto, espera <c>duration</c> segundos e oculta.</remarks>
        IEnumerator Show()
        {
            hud.ShowPrompt(message);
            yield return new WaitForSeconds(duration);
            hud.HidePrompt();
        }
    }
}
