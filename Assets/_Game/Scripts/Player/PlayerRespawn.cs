using DevGames.Core;
using UnityEngine;

namespace DevGames.Player
{
    /// <summary>Devolve o jogador ao início se ele cair no vazio ou apertar R.</summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerMotor))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] float voidHeight = -30f;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        CharacterController controller;
        PlayerMotor motor;

        /// <summary>Define o leitor de teclado (usado pelo gerador da cena).</summary>
        /// <param name="inputReader">Leitor de teclado/mouse.</param>
        public void Configure(InputReader inputReader) => input = inputReader;

        /// <summary>Busca os componentes e memoriza a posição/rotação inicial.</summary>
        void Awake()
        {
            controller = GetComponent<CharacterController>();
            motor = GetComponent<PlayerMotor>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        /// <summary>Verifica a cada quadro se precisa voltar ao início.</summary>
        /// <remarks>Abaixo de <c>voidHeight</c> (caiu no vazio) ou com R pressionado, chama Respawn.</remarks>
        void Update()
        {
            if (transform.position.y < voidHeight || input?.ResetPressed == true) Respawn();
        }

        /// <summary>Leva o jogador de volta ao ponto inicial, mantendo o cronômetro.</summary>
        /// <remarks>
        /// Zera o movimento, desliga o CharacterController (senão ele bloqueia o teleporte),
        /// reposiciona e religa.
        /// </remarks>
        public void Respawn()
        {
            motor.ResetMotion();
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            controller.enabled = true;
        }
    }
}
