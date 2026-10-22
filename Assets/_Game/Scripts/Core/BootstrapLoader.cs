using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevGames.Core
{
    /// <summary>Primeira cena do jogo: prepara a tela e carrega o menu principal.</summary>
    public sealed class BootstrapLoader : MonoBehaviour
    {
        /// <summary>Cria uma câmera simples para a tela não ficar sem imagem durante o carregamento.</summary>
        /// <remarks>A cena Bootstrap não tem câmera; esta pinta a tela na cor do menu e não desenha objetos (cullingMask = 0).</remarks>
        void Awake()
        {
            var cam = new GameObject("Boot Camera").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.11f, 0.22f, 0.27f);
            cam.cullingMask = 0;
        }

        /// <summary>Carrega a cena do menu principal.</summary>
        /// <remarks>Ao trocar de cena, a câmera temporária é destruída junto com a Bootstrap.</remarks>
        void Start() => SceneManager.LoadScene("MainMenu");
    }
}
