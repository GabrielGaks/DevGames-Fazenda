using UnityEngine;

namespace DevGames.Level
{
    /// <summary>
    /// Moves a decorative bird around an elliptical flight path above the farm.
    /// </summary>
    public sealed class BirdFlight : MonoBehaviour
    {
        [SerializeField] Vector3 center;
        [SerializeField, Min(1f)] float radius = 10f;
        [SerializeField, Min(1f)] float cycleSeconds = 9f;
        [SerializeField] float phase;
        [SerializeField, Min(0f)] float verticalMotion = 1.2f;

        /// <summary>Configures the world-space flight path.</summary>
        /// <param name="flightCenter">Center of the flight path.</param>
        /// <param name="flightRadius">Horizontal path radius in metres.</param>
        /// <param name="flightCycleSeconds">Time required for one complete lap.</param>
        /// <param name="flightPhase">Initial angular offset in radians.</param>
        public void Configure(Vector3 flightCenter, float flightRadius, float flightCycleSeconds, float flightPhase)
        {
            center = flightCenter;
            radius = flightRadius;
            cycleSeconds = flightCycleSeconds;
            phase = flightPhase;
        }

        /// <summary>Advances and orients the bird once per rendered frame.</summary>
        void Update()
        {
            var angle = phase + Time.time * Mathf.PI * 2f / cycleSeconds;
            var position = FlightPosition(angle);
            var forward = FlightPosition(angle + 0.02f) - position;
            transform.position = position;
            if (forward.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up) *
                                     Quaternion.Euler(0f, 0f, Mathf.Sin(angle) * 12f);
        }

        /// <summary>Calculates a point on the current elliptical route.</summary>
        /// <param name="angle">Angular position in radians.</param>
        /// <returns>A world-space position on the route.</returns>
        Vector3 FlightPosition(float angle)
        {
            return center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle * 2f) * verticalMotion,
                Mathf.Sin(angle) * radius * 0.65f);
        }
    }
}
