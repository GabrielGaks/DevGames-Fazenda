using UnityEngine;
using UnityEngine.InputSystem;

namespace DevGames.Core
{
    public sealed class InputReader : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] float mouseSensitivity = 0.08f;

        public Vector2 Move
        {
            get
            {
                var keyboard = Keyboard.current;
                if (keyboard == null) return Vector2.zero;

                return new Vector2(
                    (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                    (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f)).normalized;
            }
        }

        public Vector2 Look => Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue() * mouseSensitivity;
        public bool JumpPressed => Keyboard.current?.spaceKey.wasPressedThisFrame == true;
        public bool SprintHeld => Keyboard.current?.leftShiftKey.isPressed == true;
        public bool ResetPressed => Keyboard.current?.rKey.wasPressedThisFrame == true;
        public bool PausePressed => Keyboard.current?.escapeKey.wasPressedThisFrame == true;

        public void SetGameplayCursor(bool gameplay)
        {
            Cursor.lockState = gameplay ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !gameplay;
        }
    }
}

