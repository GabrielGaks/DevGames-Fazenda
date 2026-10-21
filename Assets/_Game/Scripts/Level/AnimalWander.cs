using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Makes a decorative farm animal wander calmly inside a bounded grazing area.</summary>
    public sealed class AnimalWander : MonoBehaviour
    {
        [SerializeField] Vector3 areaCenter;
        [SerializeField, Min(1f)] float radius = 5f;
        [SerializeField, Min(0.1f)] float speed = 0.6f;
        [SerializeField, Min(1f)] float turnSpeed = 4f;
        Vector3 target;
        float pauseUntil;

        /// <summary>Defines the centre, size and speed of this animal's grazing route.</summary>
        /// <param name="center">World-space centre that the animal must remain near.</param>
        /// <param name="walkRadius">Maximum horizontal distance from the centre.</param>
        /// <param name="walkSpeed">Forward movement speed in metres per second.</param>
        public void Configure(Vector3 center, float walkRadius, float walkSpeed)
        {
            areaCenter = center;
            radius = Mathf.Max(1f, walkRadius);
            speed = Mathf.Max(0.1f, walkSpeed);
            PickTarget();
        }

        /// <summary>Chooses an initial destination when the scene begins.</summary>
        void Start()
        {
            if (target == Vector3.zero) PickTarget();
        }

        /// <summary>Turns and moves the animal, pausing briefly after reaching each destination.</summary>
        void Update()
        {
            if (Time.time < pauseUntil) return;
            var offset = target - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.2f)
            {
                pauseUntil = Time.time + Random.Range(1.5f, 4f);
                PickTarget();
                return;
            }

            var facing = Quaternion.LookRotation(offset.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        /// <summary>Selects a random reachable point inside the configured circular grazing area.</summary>
        void PickTarget()
        {
            var point = Random.insideUnitCircle * radius;
            target = areaCenter + new Vector3(point.x, 0f, point.y);
        }
    }
}
