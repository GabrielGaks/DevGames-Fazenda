using UnityEngine;

namespace DevGames.Level
{
    /// <summary>
    /// Decoração do pátio: no início da fase sorteia prefabs do vetor e pontos de spawn,
    /// instancia alguns objetos que caem girando e ficam como pilha no chão. Não interfere no percurso.
    /// </summary>
    public sealed class SkillChallengeSpawner : MonoBehaviour
    {
        [Tooltip("Vetor de prefabs que podem ser criados (caixa, feno, barril).")]
        [SerializeField] GameObject[] prefabs;
        [Tooltip("Vetor de posições possíveis de spawn.")]
        [SerializeField] Transform[] spawnPoints;
        [SerializeField, Min(1)] int count = 6;
        [SerializeField, Min(0.05f)] float interval = 0.4f;
        [SerializeField, Min(0f)] float launchForce = 1.5f;
        [SerializeField, Min(0f)] float launchTorque = 3f;

        int spawned;

        /// <summary>Define os vetores usados pelo spawner (chamado pelo gerador da cena).</summary>
        /// <param name="objectPrefabs">Prefabs que podem ser sorteados.</param>
        /// <param name="points">Pontos onde os objetos podem surgir.</param>
        /// <remarks>Apenas copia as referências para os campos serializados.</remarks>
        public void Configure(GameObject[] objectPrefabs, Transform[] points)
        {
            prefabs = objectPrefabs;
            spawnPoints = points;
        }

        /// <summary>Começa a criar os objetos quando a fase inicia.</summary>
        /// <remarks>InvokeRepeating chama <see cref="SpawnTick"/> a cada <c>interval</c> segundos.</remarks>
        void Start() => InvokeRepeating(nameof(SpawnTick), 0.5f, interval);

        /// <summary>Cria um objeto por chamada até atingir <c>count</c>.</summary>
        /// <remarks>Ao chegar no total, CancelInvoke para o ciclo; os objetos ficam na cena.</remarks>
        void SpawnTick()
        {
            if (spawned >= count)
            {
                CancelInvoke(nameof(SpawnTick));
                return;
            }
            if (Spawn() != null) spawned++;
        }

        /// <summary>Spawn de objeto por função: sorteia um prefab e uma posição nos vetores e instancia.</summary>
        /// <returns>O objeto criado, ou null se os vetores estiverem vazios.</returns>
        /// <remarks>
        /// 1) Random.Range sorteia o índice do vetor de prefabs e o índice do vetor de pontos;
        /// 2) Instantiate cria a cópia com rotação aleatória;
        /// 3) se tiver Rigidbody, recebe um impulso (AddForce) e um giro (AddTorque) aleatórios.
        /// </remarks>
        public GameObject Spawn()
        {
            if (prefabs == null || prefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0) return null;

            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var instance = Instantiate(prefab, point.position, Random.rotation);

            if (instance.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(Random.insideUnitSphere * launchForce, ForceMode.Impulse);
                body.AddTorque(Random.insideUnitSphere * launchTorque, ForceMode.Impulse);
            }
            return instance;
        }
    }
}
