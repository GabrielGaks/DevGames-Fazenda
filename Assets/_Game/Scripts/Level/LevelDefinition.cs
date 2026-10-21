using UnityEngine;

namespace DevGames.Level
{
    [CreateAssetMenu(menuName = "DevGames/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [Min(1)] public int id = 1;
        public string displayName = "Fazenda";
        public string sceneName = "Farm";
        [Min(0)] public int nextLevelId = 2;
        [Min(1f)] public float referenceTimeSeconds = 300f;

        void OnValidate()
        {
            id = Mathf.Max(1, id);
            referenceTimeSeconds = Mathf.Max(1f, referenceTimeSeconds);
        }
    }
}

