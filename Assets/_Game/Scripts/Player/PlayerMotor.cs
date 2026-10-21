using DevGames.Core;
using DevGames.Level;
using UnityEngine;

namespace DevGames.Player
{
    [RequireComponent(typeof(CharacterController), typeof(LedgeClimber))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public enum MovementState { Grounded, Airborne, Hanging, Climbing }

        [SerializeField] InputReader input;
        [SerializeField] Transform cameraTransform;
        [SerializeField, Min(0f)] float walkSpeed = 4.5f;
        [SerializeField, Min(0f)] float sprintSpeed = 7f;
        [SerializeField, Min(0f)] float acceleration = 25f;
        [SerializeField, Range(0f, 1f)] float airControl = 0.35f;
        [SerializeField, Min(0.1f)] float jumpHeight = 1.6f;
        [SerializeField] float gravity = -24f;
        [SerializeField, Min(0f)] float coyoteTime = 0.1f;
        [SerializeField, Min(0f)] float jumpBuffer = 0.12f;

        CharacterController controller;
        LedgeClimber ledge;
        Vector3 horizontalVelocity;
        float verticalVelocity;
        float lastGroundedTime = float.NegativeInfinity;
        float lastJumpPressedTime = float.NegativeInfinity;

        public MovementState State { get; private set; }
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        public float VerticalSpeed => verticalVelocity;
        public bool IsSprinting => input != null && input.SprintHeld && input.Move.sqrMagnitude > 0.01f;

        public void Configure(InputReader inputReader, Transform gameplayCamera)
        {
            input = inputReader;
            cameraTransform = gameplayCamera;
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            ledge = GetComponent<LedgeClimber>();
            ledge.Initialize(controller);
        }

        void Update()
        {
            if (input == null || cameraTransform == null) return;

            if (ledge.IsBusy)
            {
                State = ledge.State == LedgeClimber.ClimbState.Hanging ? MovementState.Hanging : MovementState.Climbing;
                ledge.Tick(input);
                horizontalVelocity = Vector3.zero;
                verticalVelocity = 0f;
                return;
            }

            if (!controller.enabled) return;
            if (!controller.isGrounded && ledge.TryGrab(verticalVelocity)) return;

            if (controller.isGrounded)
            {
                lastGroundedTime = Time.time;
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            if (input.JumpPressed) lastJumpPressedTime = Time.time;

            var moveInput = input.Move;
            var forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            var desiredDirection = (forward * moveInput.y + right * moveInput.x).normalized;
            var targetSpeed = input.SprintHeld ? sprintSpeed : walkSpeed;
            var targetVelocity = desiredDirection * targetSpeed * moveInput.magnitude;
            var control = controller.isGrounded ? 1f : airControl;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * control * Time.deltaTime);

            if (desiredDirection.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredDirection), 18f * Time.deltaTime);

            if (Time.time - lastGroundedTime <= coyoteTime && Time.time - lastJumpPressedTime <= jumpBuffer)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                lastGroundedTime = float.NegativeInfinity;
                lastJumpPressedTime = float.NegativeInfinity;
            }

            verticalVelocity += gravity * Time.deltaTime;
            var platformDelta = FindPlatformDelta();
            controller.Move(platformDelta + (horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
            State = controller.isGrounded ? MovementState.Grounded : MovementState.Airborne;
        }

        Vector3 FindPlatformDelta()
        {
            if (!controller.isGrounded) return Vector3.zero;
            var origin = transform.position + Vector3.up * 0.15f;
            return Physics.Raycast(origin, Vector3.down, out var hit, 0.4f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.TryGetComponent<MovingPlatform>(out var platform) ? platform.Delta : Vector3.zero;
        }

        public void ResetMotion()
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            ledge.Cancel();
        }
    }
}

