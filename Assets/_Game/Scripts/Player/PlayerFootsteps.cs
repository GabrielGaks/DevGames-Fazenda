using UnityEngine;

namespace DevGames.Player
{
    [RequireComponent(typeof(PlayerMotor), typeof(AudioSource))]
    public sealed class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float walkingStepInterval = 0.42f;
        [SerializeField, Min(0.1f)] float sprintingStepInterval = 0.27f;
        [SerializeField, Min(0f)] float minimumSpeed = 0.8f;
        [SerializeField] Vector3 leftFootOffset = new(-0.16f, 0.08f, 0.05f);
        [SerializeField] Vector3 rightFootOffset = new(0.16f, 0.08f, 0.05f);

        PlayerMotor motor;
        AudioSource audioSource;
        AudioClip dirtStep;
        ParticleSystem dirtParticles;
        float nextStepTime;
        bool useLeftFoot;

        /// <summary>Finds movement dependencies and creates the reusable sound and dirt particle effect.</summary>
        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.7f;
            audioSource.volume = 0.55f;
            dirtStep = CreateDirtStepClip();
            dirtParticles = CreateDirtParticles();
        }

        /// <summary>Emits one alternating footstep while the character is moving on the ground.</summary>
        void Update()
        {
            if (motor == null || motor.State != PlayerMotor.MovementState.Grounded || motor.HorizontalSpeed < minimumSpeed)
            {
                nextStepTime = Time.time;
                return;
            }

            if (Time.time < nextStepTime) return;
            PlayStep(useLeftFoot ? leftFootOffset : rightFootOffset);
            useLeftFoot = !useLeftFoot;
            nextStepTime = Time.time + (motor.IsSprinting ? sprintingStepInterval : walkingStepInterval);
        }

        /// <summary>Plays a varied dirt sound and throws a small puff from the selected foot.</summary>
        /// <param name="localFootOffset">Local position of the foot that touches the ground.</param>
        void PlayStep(Vector3 localFootOffset)
        {
            audioSource.pitch = Random.Range(0.91f, 1.09f);
            audioSource.PlayOneShot(dirtStep, Random.Range(0.82f, 1f));

            var footPosition = transform.TransformPoint(localFootOffset);
            if (Physics.Raycast(footPosition + Vector3.up * 0.35f, Vector3.down, out var hit, 0.8f, ~0, QueryTriggerInteraction.Ignore))
                footPosition = hit.point + hit.normal * 0.025f;

            var emission = new ParticleSystem.EmitParams
            {
                position = footPosition,
                velocity = -transform.forward * 0.65f + Vector3.up * 0.45f + transform.right * Random.Range(-0.2f, 0.2f),
                startColor = new Color(Random.Range(0.30f, 0.46f), Random.Range(0.17f, 0.28f), Random.Range(0.07f, 0.13f), 0.8f),
                startLifetime = Random.Range(0.32f, 0.55f),
                startSize = Random.Range(0.035f, 0.085f)
            };
            for (var i = 0; i < 7; i++)
            {
                emission.velocity += new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(0.05f, 0.28f), Random.Range(-0.25f, 0.2f));
                dirtParticles.Emit(emission, 1);
            }
        }

        /// <summary>Builds a short filtered-noise clip that resembles a shoe pressing dry soil.</summary>
        /// <returns>A reusable mono AudioClip containing one synthesized dirt step.</returns>
        static AudioClip CreateDirtStepClip()
        {
            const int sampleRate = 22050;
            const float duration = 0.11f;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            var previousNoise = 0f;
            for (var i = 0; i < samples; i++)
            {
                var time = i / (float)sampleRate;
                var decay = Mathf.Exp(-time * 34f);
                var noise = Random.Range(-1f, 1f);
                previousNoise = Mathf.Lerp(previousNoise, noise, 0.22f);
                var thump = Mathf.Sin(2f * Mathf.PI * 92f * time) * Mathf.Exp(-time * 45f);
                data[i] = (previousNoise * 0.24f + thump * 0.34f) * decay;
            }

            var clip = AudioClip.Create("Passo na Terra", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Creates a world-space particle system used for small soil grains behind both feet.</summary>
        /// <returns>The configured particle system with automatic emission disabled.</returns>
        ParticleSystem CreateDirtParticles()
        {
            var effect = new GameObject("Rastro de Terra");
            effect.transform.SetParent(transform, false);
            var particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 1.15f;
            main.maxParticles = 90;
            var emission = particles.emission;
            emission.enabled = false;
            var shape = particles.shape;
            shape.enabled = false;
            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.12f;
            noise.frequency = 0.7f;
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader != null) renderer.material = new Material(shader) { name = "Terra dos Passos (Runtime)" };
            return particles;
        }
    }
}
