using UnityEngine;

namespace DevGames.Level
{
    public sealed class WindmillRotor : MonoBehaviour
    {
        [SerializeField] float degreesPerSecond = 18f;

        void Update() => transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
