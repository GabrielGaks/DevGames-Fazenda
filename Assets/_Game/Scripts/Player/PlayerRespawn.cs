using DevGames.Core;
using UnityEngine;

namespace DevGames.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMotor))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] InputReader input;
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
            if (input?.ResetPressed == true) Respawn();
        }

        public void Respawn()
        {
            motor.ResetMotion();
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            controller.enabled = true;
        }
    }
}

