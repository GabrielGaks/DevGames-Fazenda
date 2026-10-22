using DevGames.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevGames.UI
{
    /// <summary>Botões do menu principal: jogar, escolher fase e sair.</summary>
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] Button farmButton;
        [SerializeField] Text farmBestText;

        /// <summary>Prepara o menu ao abrir.</summary>
        /// <remarks>Libera o cursor, habilita o botão da fazenda se liberada e mostra o melhor tempo salvo.</remarks>
        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (farmButton != null) farmButton.interactable = ProgressStore.HighestUnlockedLevel >= 1;
            if (farmBestText != null) farmBestText.text = $"Melhor tempo: {ProgressStore.FormatMilliseconds(ProgressStore.BestMilliseconds(1))}";
        }

        /// <summary>Botão "Jogar": abre a seleção de fases.</summary>
        public void LoadLevelSelect() => SceneManager.LoadScene("LevelSelect");
        /// <summary>Abre direto a fase 1 (Fazenda).</summary>
        public void LoadFarm() => SceneManager.LoadScene("Farm");
        /// <summary>Botão "Sair": fecha o jogo (não tem efeito dentro do editor).</summary>
        public void Quit() => Application.Quit();
    }
}
