using DevGames.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevGames.UI
{
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] Button farmButton;
        [SerializeField] Text farmBestText;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (farmButton != null) farmButton.interactable = ProgressStore.HighestUnlockedLevel >= 1;
            if (farmBestText != null) farmBestText.text = $"Melhor tempo: {ProgressStore.FormatMilliseconds(ProgressStore.BestMilliseconds(1))}";
        }

        public void LoadLevelSelect() => SceneManager.LoadScene("LevelSelect");
        public void LoadFarm() => SceneManager.LoadScene("Farm");
        public void Quit() => Application.Quit();
    }
}
