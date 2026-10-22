using DevGames.Core;
using DevGames.Level;
using UnityEngine;

namespace DevGames.Player
{
    /// <summary>Movimento do jogador: andar, correr, pular, gravidade, escalada e plataformas móveis.</summary>
    [RequireComponent(typeof(CharacterController), typeof(LedgeClimber))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public enum MovementState { Grounded, Airborne, Hanging, Climbing }

        [SerializeField] InputReader input;
        [SerializeField] Transform cameraTransform;
        [Tooltip("W alone walks; Shift runs at sprintSpeed (the long jumps of the course need the run).")]
        [SerializeField, Min(0f)] float walkSpeed = 1.8f;
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

        /// <summary>Estado atual (no chão, no ar, pendurado, subindo) — usado pelas animações.</summary>
        public MovementState State { get; private set; }
        /// <summary>Velocidade horizontal em m/s (parado → andando → correndo).</summary>
        public float HorizontalSpeed => horizontalVelocity.magnitude;
        /// <summary>Velocidade vertical em m/s (positiva subindo, negativa caindo).</summary>
        public float VerticalSpeed => verticalVelocity;
        /// <summary>True quando o jogador corre (Shift + direção).</summary>
        public bool IsSprinting => input != null && input.SprintHeld && input.Move.sqrMagnitude > 0.01f;

        /// <summary>Liga teclado e câmera (usado pelo gerador da cena).</summary>
        /// <param name="inputReader">Leitor de teclado/mouse.</param>
        /// <param name="gameplayCamera">Câmera usada como referência de "frente".</param>
        public void Configure(InputReader inputReader, Transform gameplayCamera)
        {
            input = inputReader;
            cameraTransform = gameplayCamera;
        }

        /// <summary>Busca o CharacterController e o LedgeClimber e os conecta.</summary>
        void Awake()
        {
            controller = GetComponent<CharacterController>();
            ledge = GetComponent<LedgeClimber>();
            ledge.Initialize(controller);
        }

        /// <summary>Calcula e aplica o movimento do quadro.</summary>
        /// <remarks>
        /// 1) se pendurado/subindo, delega ao LedgeClimber; 2) no ar, tenta agarrar borda;
        /// 3) converte WASD em direção relativa à câmera e acelera até a velocidade de andar/correr;
        /// 4) gira o corpo para a direção; 5) pulo com coyote time e buffer; 6) gravidade;
        /// 7) CharacterController.Move somando o deslocamento da plataforma móvel.
        /// </remarks>
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

        /// <summary>Descobre se o jogador está sobre uma plataforma móvel.</summary>
        /// <returns>Deslocamento da plataforma neste quadro, ou zero.</returns>
        /// <remarks>Raio curto para baixo; se acertar um MovingPlatform, devolve o Delta dela.</remarks>
        Vector3 FindPlatformDelta()
        {
            if (!controller.isGrounded) return Vector3.zero;
            var origin = transform.position + Vector3.up * 0.15f;
            return Physics.Raycast(origin, Vector3.down, out var hit, 0.4f, ~0, QueryTriggerInteraction.Ignore)
                && hit.collider.TryGetComponent<MovingPlatform>(out var platform) ? platform.Delta : Vector3.zero;
        }

        /// <summary>Zera velocidades e cancela a escalada (usado ao voltar ao início).</summary>
        public void ResetMotion()
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            ledge.Cancel();
        }
    }
}

