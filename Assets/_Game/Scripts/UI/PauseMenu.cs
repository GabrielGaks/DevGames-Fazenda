using DevGames.Core;
using DevGames.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevGames.UI
{
    /// <summary>Menu de pausa (Esc) com os botões continuar, voltar ao início, reiniciar fase e menu principal.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] GameObject panel;
        [SerializeField] PlayerRespawn respawn;

        /// <summary>Liga as dependências do menu (usado pelo gerador da cena).</summary>
        /// <param name="inputReader">Leitor de teclado/mouse.</param>
        /// <param name="pausePanel">Painel exibido durante a pausa.</param>
        /// <param name="playerRespawn">Componente que devolve o jogador ao início.</param>
        /// <remarks>Guarda as referências e esconde o painel.</remarks>
        public void Configure(InputReader inputReader, GameObject pausePanel, PlayerRespawn playerRespawn)
        {
            input = inputReader;
            panel = pausePanel;
            respawn = playerRespawn;
            panel.SetActive(false);
        }

        /// <summary>Alterna a pausa quando Esc é pressionado.</summary>
        void Update()
        {
            if (input?.PausePressed == true) SetPaused(!panel.activeSelf);
        }

        /// <summary>Botão "Continuar": sai da pausa.</summary>
        public void Resume() => SetPaused(false);

        /// <summary>Botão "Voltar ao início": leva o jogador ao ponto inicial sem zerar o cronômetro.</summary>
        /// <remarks>Chama PlayerRespawn.Respawn e despausa.</remarks>
        public void RestartPosition()
        {
            respawn.Respawn();
            SetPaused(false);
        }

        /// <summary>Botão "Reiniciar fase": recarrega a cena atual do zero.</summary>
        /// <remarks>Restaura a escala de tempo (o pause a deixa em 0) e recarrega a cena ativa.</remarks>
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>Botão "Menu principal".</summary>
        /// <remarks>Restaura a escala de tempo e carrega a cena MainMenu.</remarks>
        public void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>Liga ou desliga a pausa.</summary>
        /// <param name="paused">True para pausar.</param>
        /// <remarks>Mostra/esconde o painel, congela o tempo (timeScale 0) e libera/trava o cursor.</remarks>
        void SetPaused(bool paused)
        {
            panel.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
            input.SetGameplayCursor(!paused);
        }
    }
}
