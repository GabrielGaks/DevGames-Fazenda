using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Plataforma que sobe e desce (ou vai e volta) suavemente usando transform.Translate.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class MovingPlatform : MonoBehaviour
    {
        [SerializeField] Vector3 localOffset = new(0f, 3f, 0f);
        [SerializeField, Min(0.1f)] float cycleSeconds = 4f;
        Vector3 start;
        Vector3 previous;

        /// <summary>Deslocamento feito neste quadro; o jogador em cima soma este valor ao próprio movimento.</summary>
        public Vector3 Delta { get; private set; }

        /// <summary>Define o percurso do movimento (usado pelo gerador da cena).</summary>
        /// <param name="offset">Deslocamento máximo, no espaço local, a partir da posição inicial.</param>
        /// <param name="seconds">Duração de um ciclo completo de ida e volta.</param>
        public void Configure(Vector3 offset, float seconds)
        {
            localOffset = offset;
            cycleSeconds = seconds;
        }

        /// <summary>Guarda a posição inicial da plataforma.</summary>
        void Awake()
        {
            start = transform.position;
            previous = start;
        }

        /// <summary>Move a plataforma a cada quadro.</summary>
        /// <remarks>
        /// Uma senoide gera t entre 0 e 1 (vai e volta em <c>cycleSeconds</c>); o alvo é start + offset * t;
        /// Translate desloca a plataforma até o alvo e Delta guarda quanto ela andou.
        /// </remarks>
        void Update()
        {
            var t = (Mathf.Sin(Time.time * Mathf.PI * 2f / cycleSeconds - Mathf.PI * 0.5f) + 1f) * 0.5f;
            var target = start + transform.rotation * localOffset * t;
            transform.Translate(target - transform.position, Space.World);
            Delta = transform.position - previous;
            previous = transform.position;
        }
    }
}
