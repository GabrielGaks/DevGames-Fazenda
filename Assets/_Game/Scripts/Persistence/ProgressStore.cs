using UnityEngine;

namespace DevGames.Persistence
{
    /// <summary>Salva o progresso do jogador (fases liberadas e melhores tempos) em PlayerPrefs.</summary>
    public static class ProgressStore
    {
        const string HighestUnlockedKey = "progress.highestUnlocked";

        /// <summary>Maior fase liberada (começa em 1).</summary>
        public static int HighestUnlockedLevel => PlayerPrefs.GetInt(HighestUnlockedKey, 1);

        /// <summary>Indica se a fase já foi concluída.</summary>
        /// <param name="levelId">Número da fase.</param>
        /// <returns>True se a fase já foi concluída alguma vez.</returns>
        public static bool IsCompleted(int levelId) => PlayerPrefs.GetInt($"progress.level.{levelId}.completed", 0) == 1;

        /// <summary>Melhor tempo salvo da fase.</summary>
        /// <param name="levelId">Número da fase.</param>
        /// <returns>Tempo em milissegundos, ou -1 se ainda não houver.</returns>
        public static int BestMilliseconds(int levelId) => PlayerPrefs.GetInt($"progress.level.{levelId}.bestMs", -1);

        /// <summary>Registra a conclusão de uma fase.</summary>
        /// <param name="levelId">Fase concluída.</param>
        /// <param name="nextLevelId">Fase que passa a ficar liberada.</param>
        /// <param name="elapsedMilliseconds">Tempo da partida em milissegundos.</param>
        /// <returns>True se o tempo for um novo recorde.</returns>
        /// <remarks>Marca a fase como concluída, libera a próxima, compara com o recorde anterior, salva e grava em disco.</remarks>
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

        /// <summary>Formata milissegundos como mm:ss.cc.</summary>
        /// <param name="milliseconds">Tempo em milissegundos (negativo = sem tempo).</param>
        /// <returns>Texto formatado, ou "--:--.--" quando não há tempo.</returns>
        /// <remarks>Separa minutos, segundos e centésimos por divisão inteira e resto.</remarks>
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
