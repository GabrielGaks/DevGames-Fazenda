using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Gira um objeto continuamente (pás do moinho, anel de nuvens).</summary>
    public sealed class WindmillRotor : MonoBehaviour
    {
        [SerializeField] float degreesPerSecond = 18f;
        [SerializeField] Vector3 axis = Vector3.forward;

        /// <summary>Define velocidade e eixo de rotação (usado pelo gerador da cena).</summary>
        /// <param name="speed">Graus por segundo.</param>
        /// <param name="localAxis">Eixo local em torno do qual o objeto gira.</param>
        public void Configure(float speed, Vector3 localAxis)
        {
            degreesPerSecond = speed;
            axis = localAxis;
        }

        /// <summary>Rotação: aplica transform.Rotate a cada quadro.</summary>
        /// <remarks>Multiplica por Time.deltaTime para a velocidade não depender do FPS.</remarks>
        void Update() => transform.Rotate(axis * (degreesPerSecond * Time.deltaTime), Space.Self);
    }
}
