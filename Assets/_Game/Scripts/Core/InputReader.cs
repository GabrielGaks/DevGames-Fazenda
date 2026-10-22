using UnityEngine;
using UnityEngine.InputSystem;

namespace DevGames.Core
{
    /// <summary>Lê teclado e mouse (Input System) e expõe os comandos do jogo em propriedades simples.</summary>
    public sealed class InputReader : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] float mouseSensitivity = 0.08f;

        /// <summary>Direção de movimento pedida pelo jogador (WASD), normalizada.</summary>
        /// <returns>X = direita/esquerda, Y = frente/trás; zero se não houver teclado.</returns>
        /// <remarks>Cada eixo vale +1, -1 ou 0 conforme as teclas; normalizar evita andar mais rápido na diagonal.</remarks>
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

        /// <summary>Movimento do mouse neste quadro, já multiplicado pela sensibilidade.</summary>
        public Vector2 Look => Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue() * mouseSensitivity;
        /// <summary>True apenas no quadro em que Espaço foi pressionado (pulo).</summary>
        public bool JumpPressed => Keyboard.current?.spaceKey.wasPressedThisFrame == true;
        /// <summary>True enquanto Shift esquerdo está segurado (correr).</summary>
        public bool SprintHeld => Keyboard.current?.leftShiftKey.isPressed == true;
        /// <summary>True no quadro em que R foi pressionado (voltar ao início).</summary>
        public bool ResetPressed => Keyboard.current?.rKey.wasPressedThisFrame == true;
        /// <summary>True no quadro em que Esc foi pressionado (pausar).</summary>
        public bool PausePressed => Keyboard.current?.escapeKey.wasPressedThisFrame == true;

        /// <summary>Trava ou libera o cursor do mouse.</summary>
        /// <param name="gameplay">True durante o jogo (cursor preso e invisível); false em menus.</param>
        /// <remarks>Altera Cursor.lockState e Cursor.visible de acordo com o modo.</remarks>
        public void SetGameplayCursor(bool gameplay)
        {
            Cursor.lockState = gameplay ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !gameplay;
        }
    }
}
