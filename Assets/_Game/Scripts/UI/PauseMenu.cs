using DevGames.Core;
using DevGames.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevGames.UI
{
    public sealed class PauseMenu : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] GameObject panel;
        [SerializeField] PlayerRespawn respawn;

        public void Configure(InputReader inputReader, GameObject pausePanel, PlayerRespawn playerRespawn)
        {
            input = inputReader;
            panel = pausePanel;
            respawn = playerRespawn;
            panel.SetActive(false);
        }

        void Update()
        {
            if (input?.PausePressed == true) SetPaused(!panel.activeSelf);
        }

        public void Resume() => SetPaused(false);

        public void RestartPosition()
        {
            respawn.Respawn();
            SetPaused(false);
        }

        public void MainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        void SetPaused(bool paused)
        {
            panel.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
            input.SetGameplayCursor(!paused);
        }
    }
}

