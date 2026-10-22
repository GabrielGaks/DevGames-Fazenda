using UnityEngine;

namespace DevGames.Player
{
    /// <summary>Sons do corpo do jogador: impulso do pulo, vento durante a queda e impacto ao pousar.</summary>
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerAudio : MonoBehaviour
    {
        [SerializeField] AudioClip[] jumpClips;
        [SerializeField] AudioClip[] landClips;
        [SerializeField] AudioClip[] hardLandClips;
        [Tooltip("Falling faster than this (m/s) lands with the heavy impact, like the animation: about a 4.5 m drop.")]
        [SerializeField, Min(0f)] float hardLandingSpeed = 15f;
        [Tooltip("Below this fall speed (m/s) a landing is a step, not an impact.")]
        [SerializeField, Min(0f)] float softLandingSpeed = 5f;
        [SerializeField, Range(0f, 1f)] float windVolume = 0.25f;

        PlayerMotor motor;
        AudioSource voice;
        AudioSource wind;
        bool wasGrounded = true;
        bool wasClimbing;
        float fallPeak;

        /// <summary>Recebe os sons de pulo e de pouso (usado pelo gerador da cena).</summary>
        public void Configure(AudioClip[] jump, AudioClip[] land, AudioClip[] hardLand)
        {
            jumpClips = jump;
            landClips = land;
            hardLandClips = hardLand;
        }

        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.volume = 0.5f; // scales every one-shot: jump, landing, pull-up
            wind = gameObject.AddComponent<AudioSource>();
            wind.clip = CreateWindClip();
            wind.loop = true;
            wind.volume = 0f;
            wind.playOnAwake = false;
            wind.Play();
        }

        /// <summary>Detecta a saída do chão (pulo), acompanha a queda (vento) e o retorno ao chão (impacto).</summary>
        void Update()
        {
            var grounded = motor.State == PlayerMotor.MovementState.Grounded;
            var falling = motor.State == PlayerMotor.MovementState.Airborne ? -motor.VerticalSpeed : 0f;

            if (wasGrounded && !grounded && motor.VerticalSpeed > 0.5f) Play(jumpClips, 0.7f);
            // ponytail: pull-up reuses the cloth clips, lower and louder; add a dedicated clip if it doesn't sell.
            var climbing = motor.State == PlayerMotor.MovementState.Climbing;
            if (climbing && !wasClimbing) Play(jumpClips, 1f, 0.75f);
            wasClimbing = climbing;
            if (!grounded) fallPeak = Mathf.Max(fallPeak, falling);
            else if (!wasGrounded)
            {
                if (fallPeak > hardLandingSpeed) Play(hardLandClips, 1f);
                else if (fallPeak > softLandingSpeed) Play(landClips, Mathf.InverseLerp(softLandingSpeed, hardLandingSpeed, fallPeak) * 0.6f + 0.4f);
                fallPeak = 0f;
            }

            // The wind rises with the fall speed and fades quickly once on the ground.
            var target = Mathf.InverseLerp(6f, 25f, falling) * windVolume;
            wind.volume = Mathf.MoveTowards(wind.volume, target, Time.deltaTime * 1.5f);
            wind.pitch = 0.8f + Mathf.InverseLerp(6f, 25f, falling) * 0.5f;
            wasGrounded = grounded;
        }

        void Play(AudioClip[] clips, float volume, float pitch = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            voice.pitch = pitch * Random.Range(0.92f, 1.08f);
            voice.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }

        /// <summary>Two seconds of softly filtered noise that loops without a seam: rushing air.</summary>
        static AudioClip CreateWindClip()
        {
            const int rate = 22050;
            const int length = rate * 2;
            var data = new float[length];
            var random = new System.Random(11);
            float low = 0f, band = 0f;
            for (var i = 0; i < length; i++)
            {
                var noise = (float)random.NextDouble() * 2f - 1f;
                low += (noise - low) * 0.08f;   // keep the rumble
                band += (low - band) * 0.02f;   // and drop the hiss
                data[i] = (low - band) * 1.6f;
            }
            // Crossfade the end into the start so the loop point is silent.
            const int fade = rate / 10;
            for (var i = 0; i < fade; i++)
            {
                var t = (float)i / fade;
                data[i] = data[i] * t + data[length - fade + i] * (1f - t);
            }
            var clip = AudioClip.Create("Vento da Queda", length - fade, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
