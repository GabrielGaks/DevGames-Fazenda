using DevGames.Core;
using UnityEngine;

namespace DevGames.Player
{
    /// <summary>Permite ao jogador agarrar bordas durante a queda, ficar pendurado e subir.</summary>
    public sealed class LedgeClimber : MonoBehaviour
    {
        public enum ClimbState { None, Hanging, Climbing }

        [SerializeField] LayerMask climbableMask;
        [SerializeField, Min(0.1f)] float reach = 0.7f;
        [SerializeField] Vector2 ledgeHeightRange = new(0.9f, 1.5f);
        [SerializeField, Min(0.05f)] float climbDuration = 0.7f;
        // Measured on the Mixamo clips, as multiples of the controller height so they follow the character's size.
        [Tooltip("Hands above the feet in the hang pose: they rest on the ledge.")]
        [SerializeField, Min(0.1f)] float handReachRatio = 1.14f;
        [Tooltip("How much lower the hands are in the first pull-up frame than in the hang: the body is raised by it so they stay on the edge.")]
        [SerializeField, Min(0f)] float climbLiftRatio = 0.39f;
        [Tooltip("How far past the hang spot the player stands after the pull-up: the clip's own travel (0.3) plus enough for the whole collider to be on the top.")]
        [SerializeField, Min(0f)] float climbForwardRatio = 0.37f;
        [Tooltip("Feet-to-edge distance while hanging: the body hangs just under the hands, clear of the wall.")]
        [SerializeField, Min(0f)] float wallGapRatio = 0.17f;

        [Tooltip("Character model under the player. During the pull-up it stays where the clip puts it while the player (and the camera following it) glides to the top.")]
        [SerializeField] Transform visual;

        CharacterController controller;
        Vector3 hangPosition;
        Vector3 climbStart;
        Vector3 climbTarget;
        float climbTime;
        Vector3 visualAnchor;
        Vector3 visualHome;

        /// <summary>Estado atual: livre, pendurado ou subindo.</summary>
        public ClimbState State { get; private set; }
        /// <summary>True enquanto pendurado ou subindo (o PlayerMotor deixa de mover o jogador).</summary>
        public bool IsBusy => State != ClimbState.None;

        /// <summary>Recebe o CharacterController do jogador.</summary>
        /// <param name="characterController">Controller desligado durante a escalada.</param>
        public void Initialize(CharacterController characterController) => controller = characterController;
        /// <summary>Define quais layers podem ser escaladas (usado pelo gerador da cena).</summary>
        /// <param name="mask">Máscara de layers escaláveis.</param>
        public void Configure(LayerMask mask) => climbableMask = mask;

        /// <summary>Tenta agarrar uma borda à frente do jogador.</summary>
        /// <param name="verticalSpeed">Velocidade vertical atual; só agarra caindo (≤ 0).</param>
        /// <returns>True se agarrou a borda.</returns>
        /// <remarks>
        /// 1) raio à frente do peito procura parede escalável; 2) raio para baixo acha o topo da borda na altura certa;
        /// 3) esfera confere se cabe em cima; 4) desliga o controller e prende o corpo pendurado.
        /// </remarks>
        public bool TryGrab(float verticalSpeed)
        {
            if (State != ClimbState.None || controller == null || verticalSpeed > 0f) return false;

            var forward = transform.forward;
            var chest = transform.position + Vector3.up * (controller.height * 0.62f);
            if (!Physics.Raycast(chest, forward, out var wall, reach, climbableMask, QueryTriggerInteraction.Ignore)) return false;

            var probeOrigin = wall.point + forward * 0.08f + Vector3.up * ledgeHeightRange.y;
            if (!Physics.Raycast(probeOrigin, Vector3.down, out var top, ledgeHeightRange.y - ledgeHeightRange.x + 0.35f, climbableMask, QueryTriggerInteraction.Ignore)) return false;

            var height = controller.height;
            hangPosition = top.point - forward * (height * wallGapRatio) - Vector3.up * (height * handReachRatio);
            // Where the pull-up clip leaves the feet: on the top, its forward travel past the hang spot.
            climbTarget = new Vector3(hangPosition.x, top.point.y + 0.03f, hangPosition.z) + forward * (height * climbForwardRatio);
            var head = climbTarget + Vector3.up * (height - controller.radius);
            if (Physics.CheckSphere(head, controller.radius * 0.9f, climbableMask, QueryTriggerInteraction.Ignore)) return false;

            controller.enabled = false;
            transform.position = hangPosition;
            State = ClimbState.Hanging;
            return true;
        }

        /// <summary>Atualiza a escalada a cada quadro.</summary>
        /// <param name="input">Leitor de teclado (S solta, Espaço sobe).</param>
        /// <returns>True enquanto a escalada controla o jogador.</returns>
        /// <remarks>
        /// Pendurado: mantém a posição, solta com S ou começa a subir com Espaço.
        /// Subindo: segura o jogador parado enquanto a animação leva o corpo até o topo e, ao terminar,
        /// coloca o jogador em cima do objeto e libera.
        /// </remarks>
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
                    climbStart = hangPosition + Vector3.up * (controller.height * climbLiftRatio);
                    climbTime = 0f;
                }
                return true;
            }

            // The pull-up clip carries the body from the hang onto the top by itself (its travel is baked
            // into the pose). The player glides from the hang to the top so the camera follows the climb,
            // while the model is pinned where it started: on screen the body moves only as the clip moves it
            // (letting both move pushed it through the object).
            if (climbTime == 0f)
            {
                transform.position = climbStart;
                if (visual != null)
                {
                    visualAnchor = visual.position;
                    visualHome = visual.localPosition;
                }
            }
            climbTime += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(climbTime / climbDuration));
            transform.position = Vector3.Lerp(climbStart, climbTarget, t);
            if (visual != null) visual.position = visualAnchor;
            if (climbTime < climbDuration) return true;
            transform.position = climbTarget;
            Release();
            return true;
        }

        /// <summary>Interrompe a escalada (ex.: ao voltar ao início).</summary>
        public void Cancel() => Release();

        /// <summary>Volta ao estado livre, devolve o modelo ao jogador e religa o CharacterController.</summary>
        void Release()
        {
            if (State == ClimbState.Climbing && visual != null) visual.localPosition = visualHome;
            State = ClimbState.None;
            if (controller != null) controller.enabled = true;
        }
    }
}

