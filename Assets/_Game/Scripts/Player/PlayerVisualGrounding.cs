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
        Transform[] feet;
        float ankleHeight;

        /// <summary>Stores the visible body meshes and the model's original local position.</summary>
        void Awake()
        {
            if (visualRoot == null) return;
            bodyRenderers = visualRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            originalLocalPosition = visualRoot.localPosition;

            // A skinned mesh's bounds follow the hips, not the feet, so on a humanoid they bob with every
            // stride and the correction fought the walk. The foot bones give the real sole instead.
            if (animator != null && animator.isHuman && bodyRenderers.Length > 0)
            {
                feet = new[] { animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot) };
                if (feet[0] == null || feet[1] == null) feet = null;
                else ankleHeight = Mathf.Min(feet[0].position.y, feet[1].position.y) - LowestBoundsY();
            }
        }

        /// <summary>Menor altura (Y) entre os renderers visíveis do corpo.</summary>
        /// <returns>Y mais baixo do modelo, ou +infinito se não houver renderer ativo.</returns>
        float LowestBoundsY()
        {
            var lowest = float.PositiveInfinity;
            foreach (var bodyRenderer in bodyRenderers)
                if (bodyRenderer != null && bodyRenderer.enabled) lowest = Mathf.Min(lowest, bodyRenderer.bounds.min.y);
            return lowest;
        }

        /// <summary>Runs after animation and keeps the lowest sole visually touching the detected floor.</summary>
        void LateUpdate()
        {
            if (motor == null || visualRoot == null || bodyRenderers == null || bodyRenderers.Length == 0) return;
            // During a ledge pull-up LedgeClimber holds the body where the clip puts it.
            if (motor.State == PlayerMotor.MovementState.Climbing) return;
            // Only on the floor: mid-jump the ray still finds the ground and would drag the body down.
            if (motor.State != PlayerMotor.MovementState.Grounded)
            {
                RestoreOriginalHeight();
                return;
            }

            var playerLayerMask = ~(1 << gameObject.layer);
            var rayOrigin = transform.position + Vector3.up * 1.25f;
            if (!Physics.Raycast(rayOrigin, Vector3.down, out var hit, 2.5f, playerLayerMask, QueryTriggerInteraction.Ignore)) return;

            // While jogging or running both feet leave the floor in turn; the offset found while walking
            // or standing is kept, instead of dipping the body at every stride.
            if (feet != null && motor.HorizontalSpeed > 2f) return;

            var visibleSole = feet != null
                ? Mathf.Min(feet[0].position.y, feet[1].position.y) - ankleHeight
                : LowestBoundsY();
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
