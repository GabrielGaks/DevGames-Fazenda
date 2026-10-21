using DevGames.Core;
using UnityEngine;

namespace DevGames.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMotor))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] float voidHeight = -15f;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        CharacterController controller;
        PlayerMotor motor;

        public void Configure(InputReader inputReader) => input = inputReader;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            motor = GetComponent<PlayerMotor>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        void Update()
        {
            if (input?.ResetPressed == true || transform.position.y < voidHeight) Respawn();
        }

        /// <summary>
        /// Returns the player to the position and rotation captured when the level started.
        /// The current attempt timer is preserved because the scene is not reloaded.
        /// </summary>
        public void Respawn()
        {
            motor.ResetMotion();
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            controller.enabled = true;
        }
    }
}

