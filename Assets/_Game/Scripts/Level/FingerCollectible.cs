using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Objetivo da fase (o "dedo"): ao ser tocado pelo jogador, conclui a fase.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class FingerCollectible : MonoBehaviour
    {
        bool collected;

        /// <summary>Chamado quando algo entra no collider trigger do objetivo.</summary>
        /// <param name="other">Collider que entrou.</param>
        /// <remarks>Só reage uma vez e à tag "Player": conclui a sessão e esconde o objeto.</remarks>
        void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player") || LevelSession.Current == null) return;
            collected = true;
            LevelSession.Current.Complete();
            gameObject.SetActive(false);
        }
    }
}
