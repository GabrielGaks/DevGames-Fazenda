using DevGames.Level;
using DevGames.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevGames.UI
{
    /// <summary>Painel exibido ao concluir a fase, com o tempo e botões de jogar novamente/seleção de fases.</summary>
    public sealed class CompletionPanel : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Text resultText;

        /// <summary>Liga o painel e o texto de resultado (usado pelo gerador da cena).</summary>
        /// <param name="completionPanel">Objeto do painel.</param>
        /// <param name="result">Texto onde o tempo é escrito.</param>
        /// <remarks>Guarda as referências e esconde o painel até a fase acabar.</remarks>
        public void Configure(GameObject completionPanel, Text result)
        {
            panel = completionPanel;
            resultText = result;
            panel.SetActive(false);
        }

        /// <summary>Passa a ouvir o evento de conclusão da sessão.</summary>
        void Start()
        {
            if (LevelSession.Current != null) LevelSession.Current.Completed += OnCompleted;
        }

        /// <summary>Para de ouvir o evento ao sair da cena.</summary>
        void OnDestroy()
        {
            if (LevelSession.Current != null) LevelSession.Current.Completed -= OnCompleted;
        }

        /// <summary>Mostra o resultado da fase.</summary>
        /// <param name="milliseconds">Tempo final.</param>
        /// <param name="isRecord">Se o tempo foi recorde.</param>
        /// <remarks>Pausa o jogo, exibe o painel com o tempo formatado e libera o cursor para clicar nos botões.</remarks>
        void OnCompleted(int milliseconds, bool isRecord)
        {
            Time.timeScale = 0f;
            panel.SetActive(true);
            resultText.text = $"FASE CONCLUÍDA\n\nDedo recuperado\n{ProgressStore.FormatMilliseconds(milliseconds)}"
                + (isRecord ? "\n\nNOVO RECORDE" : string.Empty);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>Botão "Jogar novamente": reinicia a fase.</summary>
        /// <remarks>Despausa o tempo e recarrega a cena ativa.</remarks>
        public void Replay()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>Botão "Seleção de fases".</summary>
        /// <remarks>Despausa o tempo e carrega a cena LevelSelect.</remarks>
        public void LevelSelect()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
