using UnityEngine;

namespace DevGames.Player
{
    public sealed class PlayerAnimator : MonoBehaviour
    {
        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        static readonly int GroundedHash = Animator.StringToHash("Grounded");
        static readonly int HangingHash = Animator.StringToHash("Hanging");
        static readonly int ClimbingHash = Animator.StringToHash("Climbing");
        static readonly int HardLandHash = Animator.StringToHash("HardLand");
        static readonly int LocomotionStateHash = Animator.StringToHash("Base Layer.Locomotion");

        [SerializeField] PlayerMotor motor;
        [SerializeField] Animator animator;
        [SerializeField, Min(0f)] float locomotionDamping = 0.12f;
        [Tooltip("Falling faster than this (m/s) lands with the heavy crouch: about a 4.5 m drop.")]
        [SerializeField, Min(0f)] float hardLandingSpeed = 15f;

        float fallPeak;
        bool wasGrounded = true;

        /// <summary>
        /// Updates the animation parameters from the current movement state. The speed
        /// value is damped so transitions between idle, walking and sprinting stay fluid.
        /// </summary>
        void Update()
        {
            if (motor == null || animator == null) return;
            animator.SetFloat(SpeedHash, motor.HorizontalSpeed, locomotionDamping, Time.deltaTime);
            animator.SetFloat(VerticalSpeedHash, motor.VerticalSpeed);
            animator.SetBool(GroundedHash, motor.State == PlayerMotor.MovementState.Grounded);
            animator.SetBool(HangingHash, motor.State == PlayerMotor.MovementState.Hanging);
            animator.SetBool(ClimbingHash, motor.State == PlayerMotor.MovementState.Climbing);
            TrackHardLanding();
            KeepLocomotionPlaying();
        }

        /// <summary>
        /// Remembers the fastest fall of the current airtime and fires HardLand on touch-down when it
        /// was a big drop. The trigger lives for one frame only, so a stale one never fires later.
        /// </summary>
        void TrackHardLanding()
        {
            animator.ResetTrigger(HardLandHash);
            var grounded = motor.State == PlayerMotor.MovementState.Grounded;
            if (!grounded) fallPeak = Mathf.Min(fallPeak, motor.VerticalSpeed);
            else
            {
                if (!wasGrounded && fallPeak < -hardLandingSpeed) animator.SetTrigger(HardLandHash);
                fallPeak = 0f;
            }
            wasGrounded = grounded;
        }

        /// <summary>
        /// Restarts the locomotion state after one full cycle when an imported FBX clip
        /// does not preserve its loop setting. The fractional time keeps the step cycle
        /// continuous instead of returning to a fixed left-foot pose.
        /// </summary>
        void KeepLocomotionPlaying()
        {
            if (motor.HorizontalSpeed < 0.1f || motor.State != PlayerMotor.MovementState.Grounded) return;

            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.fullPathHash != LocomotionStateHash || state.normalizedTime < 1f) return;

            animator.Play(LocomotionStateHash, 0, state.normalizedTime % 1f);
        }
    }
}

