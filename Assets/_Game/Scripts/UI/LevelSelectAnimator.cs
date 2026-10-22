using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DevGames.UI
{
    /// <summary>Animações da tela de seleção de fases: entrada do painel, ilha flutuando e transição com cortina.</summary>
    public sealed class LevelSelectAnimator : MonoBehaviour
    {
        [SerializeField] RectTransform content;
        [SerializeField] CanvasGroup contentGroup;
        [SerializeField] Image curtain;
        [SerializeField] Transform islandPreview;

        Vector2 contentPosition;
        Vector3 islandPosition;
        float elapsed;
        bool transitioning;

        /// <summary>Liga os elementos animados (usado pelo gerador da cena).</summary>
        /// <param name="contentRoot">Painel com os botões.</param>
        /// <param name="group">CanvasGroup para o fade do painel.</param>
        /// <param name="transitionCurtain">Imagem que escurece a tela na troca de cena.</param>
        /// <param name="preview">Ilha 3D exibida ao fundo.</param>
        public void Configure(RectTransform contentRoot, CanvasGroup group, Image transitionCurtain, Transform preview)
        {
            content = contentRoot;
            contentGroup = group;
            curtain = transitionCurtain;
            islandPreview = preview;
        }

        /// <summary>Prepara o estado inicial da animação de entrada.</summary>
        /// <remarks>Guarda as posições originais, desloca o painel para a esquerda, deixa-o transparente e cobre a tela.</remarks>
        void Awake()
        {
            Debug.Assert(content != null && contentGroup != null && curtain != null && islandPreview != null,
                "LevelSelectAnimator precisa das referências geradas pela cena LevelSelect.");
            contentPosition = content.anchoredPosition;
            islandPosition = islandPreview.position;
            content.anchoredPosition = contentPosition + Vector2.left * 48f;
            contentGroup.alpha = 0f;
            SetCurtain(1f, true);
        }

        /// <summary>Anima a entrada e a ilha a cada quadro.</summary>
        /// <remarks>Desliza e revela o painel com ease-out cúbico, abre a cortina e faz a ilha flutuar (seno) e girar.</remarks>
        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (!transitioning)
            {
                var progress = Mathf.Clamp01(elapsed / 0.72f);
                var eased = 1f - Mathf.Pow(1f - progress, 3f);
                content.anchoredPosition = Vector2.LerpUnclamped(contentPosition + Vector2.left * 48f, contentPosition, eased);
                contentGroup.alpha = eased;
                SetCurtain(1f - Mathf.Clamp01(elapsed / 0.42f), false);
            }

            islandPreview.position = islandPosition + Vector3.up * (Mathf.Sin(elapsed * 0.85f) * 0.08f);
            islandPreview.Rotate(0f, 3.5f * Time.unscaledDeltaTime, 0f, Space.World);
        }

        /// <summary>Botão da ilha Fazenda: abre a fase 1.</summary>
        public void LoadFarm() => StartTransition("Farm");

        /// <summary>Botão "Voltar": abre o menu principal.</summary>
        public void LoadMainMenu() => StartTransition("MainMenu");

        /// <summary>Inicia a transição para outra cena, ignorando cliques repetidos.</summary>
        /// <param name="sceneName">Nome da cena de destino.</param>
        void StartTransition(string sceneName)
        {
            if (!transitioning) StartCoroutine(TransitionTo(sceneName));
        }

        /// <summary>Corrotina da transição: esmaece o painel, fecha a cortina e carrega a cena.</summary>
        /// <param name="sceneName">Nome da cena de destino.</param>
        /// <returns>Enumerador da corrotina.</returns>
        /// <remarks>Durante 0,48 s reduz a opacidade/escala do painel e aumenta a cortina; depois chama LoadScene.</remarks>
        IEnumerator TransitionTo(string sceneName)
        {
            transitioning = true;
            var duration = 0.48f;
            var time = 0f;
            SetCurtain(0f, true);
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(time / duration);
                contentGroup.alpha = 1f - progress;
                content.localScale = Vector3.one * Mathf.Lerp(1f, 0.98f, progress);
                SetCurtain(progress, true);
                yield return null;
            }
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>Ajusta a cortina preta.</summary>
        /// <param name="alpha">Opacidade de 0 (invisível) a 1 (tela preta).</param>
        /// <param name="blocksInput">True para bloquear cliques durante a transição.</param>
        void SetCurtain(float alpha, bool blocksInput)
        {
            var color = curtain.color;
            color.a = alpha;
            curtain.color = color;
            curtain.raycastTarget = blocksInput;
        }
    }
}
