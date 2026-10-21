using DevGames.Level;
using DevGames.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevGames.UI
{
    public sealed class CompletionPanel : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Text resultText;

        public void Configure(GameObject completionPanel, Text result)
        {
            panel = completionPanel;
            resultText = result;
            panel.SetActive(false);
        }

        void Start()
        {
            if (LevelSession.Current != null) LevelSession.Current.Completed += OnCompleted;
        }

        void OnDestroy()
        {
            if (LevelSession.Current != null) LevelSession.Current.Completed -= OnCompleted;
        }

        void OnCompleted(int milliseconds, bool isRecord)
        {
            Time.timeScale = 0f;
            panel.SetActive(true);
            resultText.text = $"FASE CONCLUÍDA\n\nDedo recuperado\n{ProgressStore.FormatMilliseconds(milliseconds)}"
                + (isRecord ? "\n\nNOVO RECORDE" : string.Empty);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Replay()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LevelSelect()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("LevelSelect");
        }
    }
}

