using UnityEngine;

namespace DevGames.Player
{
    /// <summary>Keeps the animated character's feet visually aligned with the floor.</summary>
    public sealed class PlayerVisualGrounding : MonoBehaviour
    {
        [SerializeField] PlayerMotor motor;
        [SerializeField] Animator animator;
        [SerializeField] Transform visualRoot;
        [SerializeField] float soleOffset = -0.038f;
        [SerializeField, Min(0f)] float adjustmentSpeed = 18f;
        [SerializeField, Min(0f)] float maximumCorrection = 0.75f;

        SkinnedMeshRenderer[] bodyRenderers;
        Vector3 originalLocalPosition;

        /// <summary>Stores the visible body meshes and the model's original local position.</summary>
        void Awake()
        {
            if (visualRoot == null) return;
            bodyRenderers = visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            originalLocalPosition = visualRoot.localPosition;
        }

        /// <summary>Runs after animation and keeps the lowest sole visually touching the detected floor.</summary>
        void LateUpdate()
        {
            if (motor == null || visualRoot == null || bodyRenderers == null || bodyRenderers.Length == 0) return;
            if (motor.State == PlayerMotor.MovementState.Hanging || motor.State == PlayerMotor.MovementState.Climbing)
            {
                RestoreOriginalHeight();
                return;
            }

            var playerLayerMask = ~(1 << gameObject.layer);
            var rayOrigin = transform.position + Vector3.up * 1.25f;
            if (!Physics.Raycast(rayOrigin, Vector3.down, out var hit, 2.5f, playerLayerMask, QueryTriggerInteraction.Ignore)) return;

            var visibleSole = float.PositiveInfinity;
            foreach (var bodyRenderer in bodyRenderers)
            {
                if (bodyRenderer != null && bodyRenderer.enabled)
                    visibleSole = Mathf.Min(visibleSole, bodyRenderer.bounds.min.y);
            }
            if (float.IsPositiveInfinity(visibleSole)) return;

            var correction = Mathf.Clamp(hit.point.y + soleOffset - visibleSole, -maximumCorrection, maximumCorrection);
            var smoothing = 1f - Mathf.Exp(-adjustmentSpeed * Time.deltaTime);
            visualRoot.position += Vector3.up * (correction * smoothing);
        }

        /// <summary>Returns the model to its authored height while jumping, falling or climbing.</summary>
        void RestoreOriginalHeight()
        {
            var local = visualRoot.localPosition;
            var smoothing = 1f - Mathf.Exp(-adjustmentSpeed * Time.deltaTime);
            local.y = Mathf.Lerp(local.y, originalLocalPosition.y, smoothing);
            visualRoot.localPosition = local;
        }

        /// <summary>Connects the movement state, humanoid animator and model root.</summary>
        /// <param name="playerMotor">Motor that reports whether the player is grounded.</param>
        /// <param name="characterAnimator">Humanoid animator used to locate both feet.</param>
        /// <param name="characterVisual">Root transform moved only for visual grounding.</param>
        public void Configure(PlayerMotor playerMotor, Animator characterAnimator, Transform characterVisual)
        {
            motor = playerMotor;
            animator = characterAnimator;
            visualRoot = characterVisual;
        }
    }
}
