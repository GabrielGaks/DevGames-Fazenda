using UnityEngine;

namespace DevGames.UI
{
    /// <summary>Cena animada do menu principal: personagem fazendo parkour em loop, câmera e entrada do painel.</summary>
    public sealed class MenuSceneAnimator : MonoBehaviour
    {
        static readonly int RunState = Animator.StringToHash("Base Layer.Run");
        static readonly int JumpStartState = Animator.StringToHash("Base Layer.JumpStart");
        static readonly int JumpLoopState = Animator.StringToHash("Base Layer.JumpLoop");
        static readonly int JumpLandState = Animator.StringToHash("Base Layer.JumpLand");
        static readonly int FallState = Animator.StringToHash("Base Layer.Fall");

        [SerializeField] Transform character;
        [SerializeField] Animator characterAnimator;
        [SerializeField] Transform cameraTransform;
        [SerializeField] RectTransform menuPanel;
        [SerializeField] CanvasGroup menuGroup;
        [SerializeField] Vector3 pathStart;
        [SerializeField] Vector3 pathEnd;
        [SerializeField] Vector3[] coursePoints;
        [SerializeField, Range(0.5f, 0.95f)] float edgePoint = 0.72f;
        [SerializeField, Range(0.5f, 0.95f)] float fallPoint = 0.72f;
        [SerializeField, Min(1f)] float cycleSeconds = 22f;
        [SerializeField, Min(0f)] float jumpHeight = 1.2f;

        Vector3 cameraPosition;
        Quaternion cameraRotation;
        Vector2 panelPosition;
        float startTime;
        int currentPose = -1;

        /// <summary>Define personagem, animator, câmera e começo/fim do percurso.</summary>
        /// <param name="animatedCharacter">Transform do personagem do menu.</param>
        /// <param name="animator">Animator que troca as poses.</param>
        /// <param name="animatedCamera">Câmera que balança levemente.</param>
        /// <param name="start">Início do percurso.</param>
        /// <param name="end">Fim do percurso.</param>
        public void Configure(Transform animatedCharacter, Animator animator, Transform animatedCamera,
            Vector3 start, Vector3 end)
        {
            character = animatedCharacter;
            characterAnimator = animator;
            cameraTransform = animatedCamera;
            pathStart = start;
            pathEnd = end;
        }

        /// <summary>Configura o percurso de parkour completo e o painel do menu (usado pelo gerador do menu).</summary>
        /// <param name="animatedCharacter">Transform do personagem.</param>
        /// <param name="animator">Animator do personagem.</param>
        /// <param name="animatedCamera">Câmera do menu.</param>
        /// <param name="panel">Painel de botões que entra deslizando.</param>
        /// <param name="group">CanvasGroup para o fade do painel.</param>
        /// <param name="points">Pontos de pouso do parkour, do primeiro ao último (mínimo 3).</param>
        public void ConfigureCourse(Transform animatedCharacter, Animator animator, Transform animatedCamera,
            RectTransform panel, CanvasGroup group, Vector3[] points)
        {
            Debug.Assert(points != null && points.Length >= 3);
            coursePoints = points;
            Configure(animatedCharacter, animator, animatedCamera, points[0], points[points.Length - 1]);
            menuPanel = panel;
            menuGroup = group;
        }

        /// <summary>Guarda posições iniciais e confere a configuração.</summary>
        /// <remarks>Salva câmera e painel; Debug.Assert valida pontos crescentes e estados do Animator.</remarks>
        void Awake()
        {
            if (cameraTransform != null)
            {
                cameraPosition = cameraTransform.position;
                cameraRotation = cameraTransform.rotation;
            }

            if (menuPanel != null) panelPosition = menuPanel.anchoredPosition;
            Debug.Assert(coursePoints != null && coursePoints.Length >= 3);
            for (var i = 1; coursePoints != null && i < coursePoints.Length - 1; i++)
                Debug.Assert(coursePoints[i].y >= coursePoints[i - 1].y);
            Debug.Assert(edgePoint <= fallPoint && fallPoint < 1f);
            if (characterAnimator != null)
            {
                Debug.Assert(characterAnimator.HasState(0, JumpStartState));
                Debug.Assert(characterAnimator.HasState(0, JumpLoopState));
                Debug.Assert(characterAnimator.HasState(0, JumpLandState));
                Debug.Assert(characterAnimator.HasState(0, FallState));
            }
        }

        /// <summary>Reinicia o loop quando o menu aparece.</summary>
        /// <remarks>Zera o relógio e a pose, mostra o personagem e esconde/desloca o painel para a entrada.</remarks>
        void OnEnable()
        {
            startTime = Time.unscaledTime;
            currentPose = -1;
            if (character != null) character.gameObject.SetActive(true);
            if (menuGroup != null) menuGroup.alpha = 0f;
            if (menuPanel != null) menuPanel.anchoredPosition = panelPosition + Vector2.left * 44f;
        }

        /// <summary>Avança a animação a cada quadro.</summary>
        /// <remarks>Calcula o progresso do ciclo, anima personagem e painel e faz a câmera oscilar levemente.</remarks>
        void Update()
        {
            if (character == null) return;
            var elapsed = Time.unscaledTime - startTime;
            var progress = Mathf.Repeat(elapsed / cycleSeconds, 1f);
            AnimateCharacter(progress);
            AnimatePresentation(elapsed);

            if (cameraTransform == null) return;
            var drift = Mathf.Sin(Time.unscaledTime * 0.22f);
            cameraTransform.SetPositionAndRotation(cameraPosition + Vector3.right * drift * 0.12f,
                cameraRotation * Quaternion.Euler(0f, drift * 0.35f, 0f));
        }

        /// <summary>Escolhe a fase do loop do personagem.</summary>
        /// <param name="progress">Progresso do ciclo de 0 a 1.</param>
        /// <remarks>Parkour até edgePoint; some fora da tela; cai de volta na ilha; aterrissa; corre até o início.</remarks>
        void AnimateCharacter(float progress)
        {
            if (progress < edgePoint)
            {
                character.gameObject.SetActive(true);
                AnimateParkour(progress / edgePoint);
                return;
            }

            if (progress < fallPoint)
            {
                character.gameObject.SetActive(false);
                return;
            }

            character.gameObject.SetActive(true);
            var islandLanding = pathStart + new Vector3(4f, 0f, 2.5f);
            var runDirection = Vector3.ProjectOnPlane(pathStart - islandLanding, Vector3.up).normalized;
            if (progress < 0.85f)
            {
                var fallProgress = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(fallPoint, 0.85f, progress));
                var fallOrigin = new Vector3(islandLanding.x + 1.5f,
                    coursePoints[coursePoints.Length - 2].y, islandLanding.z);
                character.SetPositionAndRotation(Vector3.Lerp(fallOrigin, islandLanding, fallProgress * fallProgress),
                    FacingRotation(runDirection) * Quaternion.Euler(15f, 0f, 0f));
                SetPose(FallState);
                return;
            }

            if (progress < 0.87f)
            {
                character.SetPositionAndRotation(islandLanding, FacingRotation(runDirection));
                SetPose(JumpLandState);
                return;
            }

            var runProgress = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.87f, 1f, progress));
            character.SetPositionAndRotation(Vector3.Lerp(islandLanding, pathStart, runProgress), FacingRotation(runDirection));
            SetPose(RunState);
        }

        /// <summary>Move o personagem de pouso em pouso com saltos em arco.</summary>
        /// <param name="progress">Progresso do trecho de parkour de 0 a 1.</param>
        /// <remarks>
        /// Converte o progresso em segmento entre dois pontos; no ar usa Lerp + seno para o arco
        /// e troca as poses de pulo; no fim do segmento aterrissa e gira para o próximo salto.
        /// </remarks>
        void AnimateParkour(float progress)
        {
            const float airborneEnd = 0.78f;
            const int offscreenStartPoint = 14;
            var offscreenShare = 1.5f / (cycleSeconds * edgePoint);
            var visibleShare = 1f - offscreenShare;
            var segmentProgress = progress < visibleShare
                ? progress / visibleShare * offscreenStartPoint
                : offscreenStartPoint + Mathf.InverseLerp(visibleShare, 1f, progress) *
                  (coursePoints.Length - 1 - offscreenStartPoint);
            var segment = Mathf.Min(Mathf.FloorToInt(segmentProgress), coursePoints.Length - 2);
            var localProgress = segmentProgress - segment;
            var start = coursePoints[segment];
            var end = coursePoints[segment + 1];
            var direction = Vector3.ProjectOnPlane(end - start, Vector3.up).normalized;

            if (localProgress >= airborneEnd)
            {
                var nextDirection = segment < coursePoints.Length - 2
                    ? Vector3.ProjectOnPlane(coursePoints[segment + 2] - end, Vector3.up).normalized
                    : direction;
                var turnProgress = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(airborneEnd, 1f, localProgress));
                character.SetPositionAndRotation(end,
                    Quaternion.Slerp(FacingRotation(direction), FacingRotation(nextDirection), turnProgress));
                SetPose(JumpLandState);
                return;
            }

            var jumpProgress = Mathf.SmoothStep(0f, 1f, localProgress / airborneEnd);
            var position = Vector3.Lerp(start, end, jumpProgress) +
                           Vector3.up * (Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight);
            character.SetPositionAndRotation(position, FacingRotation(direction));
            SetPose(jumpProgress < 0.2f ? JumpStartState : jumpProgress > 0.82f ? JumpLandState : JumpLoopState);
        }

        /// <summary>Rotação que faz o modelo olhar na direção dada.</summary>
        /// <param name="direction">Direção horizontal desejada.</param>
        /// <returns>Rotação corrigida em 90° (o modelo do menu aponta para o lado).</returns>
        static Quaternion FacingRotation(Vector3 direction) =>
            Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f);

        /// <summary>Entrada do painel do menu (fade + deslize).</summary>
        /// <param name="elapsed">Segundos desde que o menu abriu.</param>
        void AnimatePresentation(float elapsed)
        {
            if (menuPanel == null || menuGroup == null) return;
            var entrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.75f));
            menuGroup.alpha = entrance;
            menuPanel.anchoredPosition = panelPosition + Vector2.left * (1f - entrance) * 44f;
        }

        /// <summary>Troca a animação do personagem com CrossFade, só quando ela muda.</summary>
        /// <param name="pose">Hash do estado do Animator (Run, JumpStart, JumpLoop, JumpLand, Fall).</param>
        void SetPose(int pose)
        {
            if (characterAnimator == null || currentPose == pose) return;
            characterAnimator.CrossFade(pose, currentPose < 0 ? 0f : 0.1f);
            currentPose = pose;
        }

    }
}
