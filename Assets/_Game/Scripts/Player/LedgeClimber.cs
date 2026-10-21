using DevGames.Core;
using UnityEngine;

namespace DevGames.Player
{
    public sealed class LedgeClimber : MonoBehaviour
    {
        public enum ClimbState { None, Hanging, Climbing }

        [SerializeField] LayerMask climbableMask;
        [SerializeField, Min(0.1f)] float reach = 0.7f;
        [SerializeField] Vector2 ledgeHeightRange = new(0.9f, 1.5f);
        [SerializeField, Min(0.05f)] float climbDuration = 0.35f;

        CharacterController controller;
        Vector3 hangPosition;
        Vector3 climbStart;
        Vector3 climbTarget;
        float climbTime;

        public ClimbState State { get; private set; }
        public bool IsBusy => State != ClimbState.None;

        public void Initialize(CharacterController characterController) => controller = characterController;
        public void Configure(LayerMask mask) => climbableMask = mask;

        public bool TryGrab(float verticalSpeed)
        {
            if (State != ClimbState.None || controller == null || verticalSpeed > 0f) return false;

            var forward = transform.forward;
            var chest = transform.position + Vector3.up * (controller.height * 0.62f);
            if (!Physics.Raycast(chest, forward, out var wall, reach, climbableMask, QueryTriggerInteraction.Ignore)) return false;

            var probeOrigin = wall.point + forward * 0.08f + Vector3.up * ledgeHeightRange.y;
            if (!Physics.Raycast(probeOrigin, Vector3.down, out var top, ledgeHeightRange.y - ledgeHeightRange.x + 0.35f, climbableMask, QueryTriggerInteraction.Ignore)) return false;

            climbTarget = top.point + forward * (controller.radius + 0.12f) + Vector3.up * 0.03f;
            var head = climbTarget + Vector3.up * (controller.height - controller.radius);
            if (Physics.CheckSphere(head, controller.radius * 0.9f, climbableMask, QueryTriggerInteraction.Ignore)) return false;

            hangPosition = top.point - forward * (controller.radius + 0.08f) - Vector3.up * (controller.height - 0.35f);
            controller.enabled = false;
            transform.position = hangPosition;
            State = ClimbState.Hanging;
            return true;
        }

        public bool Tick(InputReader input)
        {
            if (State == ClimbState.None) return false;

            if (State == ClimbState.Hanging)
            {
                transform.position = hangPosition;
                if (input.Move.y < -0.5f)
                {
                    Release();
                    return false;
                }

                if (input.JumpPressed)
                {
                    State = ClimbState.Climbing;
                    climbStart = transform.position;
                    climbTime = 0f;
                }
                return true;
            }

            climbTime += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(climbTime / climbDuration));
            transform.position = Vector3.Lerp(climbStart, climbTarget, t);
            if (t >= 1f) Release();
            return true;
        }

        public void Cancel() => Release();

        void Release()
        {
            State = ClimbState.None;
            if (controller != null) controller.enabled = true;
        }
    }
}

