using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Área invisível que registra no Console o tempo em que o jogador passou por um setor.</summary>
    public sealed class SplitTimerTrigger : MonoBehaviour
    {
        [SerializeField] string sectorName = "Setor";
        bool triggered;

        /// <summary>Chamado pelo Unity quando algo entra na área (collider trigger).</summary>
        /// <param name="other">Collider que entrou.</param>
        /// <remarks>Só reage uma vez e só ao objeto com a tag "Player"; registra o tempo da sessão.</remarks>
        void OnTriggerEnter(Collider other)
        {
            if (triggered || !other.CompareTag("Player") || LevelSession.Current == null) return;
            triggered = true;
            Debug.Log($"[Fazenda] {sectorName}: {LevelSession.Current.ElapsedSeconds:0.00}s");
        }
    }
}
