using UnityEngine;

namespace DevGames.Level
{
    [RequireComponent(typeof(Collider))]
    public sealed class FingerCollectible : MonoBehaviour
    {
        bool collected;

        void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player") || LevelSession.Current == null) return;
            collected = true;
            LevelSession.Current.Complete();
            gameObject.SetActive(false);
        }
    }
}

