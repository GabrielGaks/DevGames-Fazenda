using System;
using DevGames.Persistence;
using UnityEngine;

namespace DevGames.Level
{
    public sealed class LevelSession : MonoBehaviour
    {
        [SerializeField] LevelDefinition definition;
        float startedAt;

        public static LevelSession Current { get; private set; }
        public event Action<int, bool> Completed;
        public bool IsComplete { get; private set; }
        public float ElapsedSeconds => IsComplete ? FinalMilliseconds / 1000f : Mathf.Max(0f, Time.time - startedAt);
        public int FinalMilliseconds { get; private set; }
        public LevelDefinition Definition => definition;

        public void Configure(LevelDefinition levelDefinition) => definition = levelDefinition;

        void Awake() => Current = this;

        void Start() => startedAt = Time.time;

        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        public void Complete()
        {
            if (IsComplete || definition == null) return;
            IsComplete = true;
            FinalMilliseconds = Mathf.RoundToInt((Time.time - startedAt) * 1000f);
            var record = ProgressStore.CompleteLevel(definition.id, definition.nextLevelId, FinalMilliseconds);
            Completed?.Invoke(FinalMilliseconds, record);
        }
    }
}

