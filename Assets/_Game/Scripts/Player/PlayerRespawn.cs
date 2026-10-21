using DevGames.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevGames.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerMotor))]
    public sealed class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] float voidHeight = -30f;
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
            // Falling off the island into the void restarts the level (timer included);
            // R only returns to the start and keeps the timer running.
            if (transform.position.y < voidHeight) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            else if (input?.ResetPressed == true) Respawn();
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

