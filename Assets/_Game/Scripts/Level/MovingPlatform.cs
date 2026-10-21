using UnityEngine;

namespace DevGames.Level
{
    [DefaultExecutionOrder(-100)]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] Vector3 localOffset = new(0f, 3f, 0f);
        [SerializeField, Min(0.1f)] float cycleSeconds = 4f;
        Vector3 start;
        Vector3 previous;

        public Vector3 Delta { get; private set; }

        void Awake()
        {
            start = transform.position;
            previous = start;
        }

        void Update()
        {
            var t = (Mathf.Sin(Time.time * Mathf.PI * 2f / cycleSeconds - Mathf.PI * 0.5f) + 1f) * 0.5f;
            transform.position = start + transform.rotation * localOffset * t;
            Delta = transform.position - previous;
            previous = transform.position;
        }
    }
}

