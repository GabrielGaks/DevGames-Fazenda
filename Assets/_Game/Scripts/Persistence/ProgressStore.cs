using UnityEngine;

namespace DevGames.Persistence
{
    public static class ProgressStore
    {
        const string HighestUnlockedKey = "progress.highestUnlocked";

        public static int HighestUnlockedLevel => PlayerPrefs.GetInt(HighestUnlockedKey, 1);
        public static bool IsCompleted(int levelId) => PlayerPrefs.GetInt($"progress.level.{levelId}.completed", 0) == 1;
        public static int BestMilliseconds(int levelId) => PlayerPrefs.GetInt($"progress.level.{levelId}.bestMs", -1);

        public static bool CompleteLevel(int levelId, int nextLevelId, int elapsedMilliseconds)
        {
            PlayerPrefs.SetInt($"progress.level.{levelId}.completed", 1);
            if (nextLevelId > HighestUnlockedLevel) PlayerPrefs.SetInt(HighestUnlockedKey, nextLevelId);

            var previous = BestMilliseconds(levelId);
            var isRecord = previous < 0 || elapsedMilliseconds < previous;
            if (isRecord) PlayerPrefs.SetInt($"progress.level.{levelId}.bestMs", elapsedMilliseconds);
            PlayerPrefs.Save();
            return isRecord;
        }

        public static string FormatMilliseconds(int milliseconds)
        {
            if (milliseconds < 0) return "--:--.--";
            var minutes = milliseconds / 60000;
            var seconds = milliseconds % 60000 / 1000;
            var hundredths = milliseconds % 1000 / 10;
            return $"{minutes:00}:{seconds:00}.{hundredths:00}";
        }
    }
}

