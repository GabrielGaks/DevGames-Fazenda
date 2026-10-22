using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Dados de uma fase (asset ScriptableObject): número, nome, cena e próxima fase.</summary>
    [CreateAssetMenu(menuName = "DevGames/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [Min(1)] public int id = 1;
        public string displayName = "Fazenda";
        public string sceneName = "Farm";
        [Min(0)] public int nextLevelId = 2;
        [Min(1f)] public float referenceTimeSeconds = 300f;

        /// <summary>Chamado pelo Unity quando um valor é alterado no Inspector.</summary>
        /// <remarks>Garante id mínimo 1 e tempo de referência mínimo de 1 s.</remarks>
        void OnValidate()
        {
            id = Mathf.Max(1, id);
            referenceTimeSeconds = Mathf.Max(1f, referenceTimeSeconds);
        }
    }
}
