using DevGames.Core;
using UnityEngine;

namespace DevGames.Player
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] InputReader input;
        [SerializeField] LayerMask collisionMask = ~0;
        [SerializeField, Min(1f)] float distance = 5f;
        [SerializeField, Min(0f)] float pivotHeight = 1.5f;
        [SerializeField] Vector2 pitchLimits = new(-35f, 70f);
        [SerializeField, Min(0f)] float positionSmoothTime = 0.04f;

        float yaw;
        float pitch = 18f;
        Vector3 velocity;

        public void Configure(Transform followTarget, InputReader inputReader)
        {
            target = followTarget;
            input = inputReader;
        }

        void LateUpdate()
        {
            if (target == null || input == null) return;

            var look = input.Look;
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, pitchLimits.x, pitchLimits.y);

            var pivot = target.position + Vector3.up * pivotHeight;
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var direction = rotation * Vector3.back;
            var actualDistance = distance;
            if (Physics.SphereCast(pivot, 0.2f, direction, out var hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
                actualDistance = Mathf.Max(0.35f, hit.distance - 0.1f);

            var desired = pivot + direction * actualDistance;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, positionSmoothTime);
            transform.rotation = rotation;
        }
    }
}

