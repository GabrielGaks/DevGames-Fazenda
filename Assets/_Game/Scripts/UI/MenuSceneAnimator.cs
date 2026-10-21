using UnityEngine;

namespace DevGames.UI
{
    public sealed class MenuSceneAnimator : MonoBehaviour
    {
        static readonly int RunState = Animator.StringToHash("Run");
        static readonly int JumpStartState = Animator.StringToHash("JumpStart");
        static readonly int JumpLoopState = Animator.StringToHash("JumpLoop");
        static readonly int JumpLandState = Animator.StringToHash("JumpLand");
        static readonly int FallState = Animator.StringToHash("Fall");

        [SerializeField] Transform character;
        [SerializeField] Animator characterAnimator;
        [SerializeField] Transform cameraTransform;
        [SerializeField] RectTransform menuPanel;
        [SerializeField] CanvasGroup menuGroup;
        [SerializeField] Vector3 pathStart;
        [SerializeField] Vector3 pathEnd;
        [SerializeField] float[] jumpPoints = { 0.22f, 0.43f, 0.62f };
        [SerializeField, Range(0.5f, 0.95f)] float edgePoint = 0.76f;
        [SerializeField, Min(1f)] float cycleSeconds = 12f;
        [SerializeField, Min(0f)] float jumpHeight = 1.65f;

        Vector3 cameraPosition;
        Quaternion cameraRotation;
        Vector2 panelPosition;
        float startTime;
        int currentPose = -1;

        public void Configure(Transform animatedCharacter, Animator animator, Transform animatedCamera,
            Vector3 start, Vector3 end)
        {
            character = animatedCharacter;
            characterAnimator = animator;
            cameraTransform = animatedCamera;
            pathStart = start;
            pathEnd = end;
        }

        public void ConfigureCourse(Transform animatedCharacter, Animator animator, Transform animatedCamera,
            RectTransform panel, CanvasGroup group, Vector3 start, Vector3 edge)
        {
            Configure(animatedCharacter, animator, animatedCamera, start, edge);
            menuPanel = panel;
            menuGroup = group;
        }

        void Awake()
        {
            if (cameraTransform != null)
            {
                cameraPosition = cameraTransform.position;
                cameraRotation = cameraTransform.rotation;
            }

            if (menuPanel != null) panelPosition = menuPanel.anchoredPosition;
            Debug.Assert(Mathf.Approximately(JumpArc(0f, 0.2f), 0f));
            Debug.Assert(JumpArc(0.2f, 0.2f) > jumpHeight * 0.99f);
        }

        void OnEnable()
        {
            startTime = Time.unscaledTime;
            currentPose = -1;
            if (character != null) character.gameObject.SetActive(true);
            if (menuGroup != null) menuGroup.alpha = 0f;
            if (menuPanel != null) menuPanel.anchoredPosition = panelPosition + Vector2.left * 44f;
        }

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

        void AnimateCharacter(float progress)
        {
            var forward = (pathEnd - pathStart).normalized;
            if (progress < edgePoint)
            {
                character.gameObject.SetActive(true);
                var runProgress = progress / edgePoint;
                var height = 0f;
                var pose = RunState;
                foreach (var jumpPoint in jumpPoints)
                {
                    var arc = JumpArc(runProgress, jumpPoint);
                    if (arc <= height) continue;
                    height = arc;
                    var jumpProgress = Mathf.InverseLerp(jumpPoint - 0.065f, jumpPoint + 0.065f, runProgress);
                    pose = jumpProgress < 0.22f ? JumpStartState : jumpProgress > 0.78f ? JumpLandState : JumpLoopState;
                }

                var position = Vector3.Lerp(pathStart, pathEnd, runProgress) + Vector3.up * height;
                character.SetPositionAndRotation(position, Quaternion.LookRotation(forward));
                SetPose(pose);
                return;
            }

            var fallProgress = Mathf.InverseLerp(edgePoint, 1f, progress);
            var fallHeight = fallProgress * 6f - fallProgress * fallProgress * 20f;
            var fallPosition = pathEnd + forward * (fallProgress * 3.5f) + Vector3.up * fallHeight;
            var tumble = Mathf.InverseLerp(0.3f, 1f, fallProgress);
            character.SetPositionAndRotation(fallPosition,
                Quaternion.LookRotation(forward) * Quaternion.Euler(tumble * 145f, 0f, 0f));
            SetPose(fallProgress < 0.12f ? JumpStartState : fallProgress < 0.3f ? JumpLoopState : FallState);
            if (fallProgress > 0.94f) character.gameObject.SetActive(false);
        }

        void AnimatePresentation(float elapsed)
        {
            if (menuPanel == null || menuGroup == null) return;
            var entrance = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.75f));
            menuGroup.alpha = entrance;
            menuPanel.anchoredPosition = panelPosition + Vector2.left * (1f - entrance) * 44f;
        }

        void SetPose(int pose)
        {
            if (characterAnimator == null || currentPose == pose) return;
            characterAnimator.CrossFade(pose, currentPose < 0 ? 0f : 0.1f);
            currentPose = pose;
        }

        float JumpArc(float progress, float center)
        {
            const float halfWidth = 0.065f;
            if (progress < center - halfWidth || progress > center + halfWidth) return 0f;
            return Mathf.Sin(Mathf.InverseLerp(center - halfWidth, center + halfWidth, progress) * Mathf.PI) * jumpHeight;
        }
    }
}
