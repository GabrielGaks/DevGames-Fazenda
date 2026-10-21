using UnityEngine;

namespace DevGames.Level
{
    public sealed class SplitTimerTrigger : MonoBehaviour
    {
        [SerializeField] string sectorName = "Setor";
        bool triggered;

        void OnTriggerEnter(Collider other)
        {
            if (triggered || !other.CompareTag("Player") || LevelSession.Current == null) return;
            triggered = true;
            Debug.Log($"[Fazenda] {sectorName}: {LevelSession.Current.ElapsedSeconds:0.00}s");
        }
    }
}
