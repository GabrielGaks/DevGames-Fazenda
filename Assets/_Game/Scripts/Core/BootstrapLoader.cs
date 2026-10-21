using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevGames.Core
{
    public sealed class BootstrapLoader : MonoBehaviour
    {
        void Start() => SceneManager.LoadScene("MainMenu");
    }
}
