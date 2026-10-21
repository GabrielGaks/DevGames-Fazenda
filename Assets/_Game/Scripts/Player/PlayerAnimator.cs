using UnityEngine;

namespace DevGames.Player
{
    public sealed class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] PlayerMotor motor;
        [SerializeField] Animator animator;

        void Update()
        {
            if (motor == null || animator == null) return;
            animator.SetFloat("Speed", motor.HorizontalSpeed);
            animator.SetFloat("VerticalSpeed", motor.VerticalSpeed);
            animator.SetBool("Grounded", motor.State == PlayerMotor.MovementState.Grounded);
            animator.SetBool("Hanging", motor.State == PlayerMotor.MovementState.Hanging);
            animator.SetBool("Climbing", motor.State == PlayerMotor.MovementState.Climbing);
        }
    }
}

