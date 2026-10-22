using System;
using DevGames.Persistence;
using UnityEngine;

namespace DevGames.Level
{
    /// <summary>Controla a partida atual: cronômetro, conclusão da fase e gravação do recorde.</summary>
    public sealed class LevelSession : MonoBehaviour
    {
        [SerializeField] LevelDefinition definition;
        float startedAt;

        /// <summary>Sessão ativa na cena (acesso global simples).</summary>
        public static LevelSession Current { get; private set; }
        /// <summary>Disparado ao concluir: (tempo em ms, se foi recorde).</summary>
        public event Action<int, bool> Completed;
        /// <summary>True depois que o jogador pega o objetivo.</summary>
        public bool IsComplete { get; private set; }
        /// <summary>Tempo decorrido em segundos; congela ao concluir.</summary>
        public float ElapsedSeconds => IsComplete ? FinalMilliseconds / 1000f : Mathf.Max(0f, Time.time - startedAt);
        /// <summary>Tempo final em milissegundos.</summary>
        public int FinalMilliseconds { get; private set; }
        /// <summary>Dados da fase em jogo.</summary>
        public LevelDefinition Definition => definition;

        /// <summary>Define os dados da fase (usado pelo gerador da cena).</summary>
        /// <param name="levelDefinition">Asset com id, nome e próxima fase.</param>
        /// <remarks>Apenas guarda a referência.</remarks>
        public void Configure(LevelDefinition levelDefinition) => definition = levelDefinition;

        /// <summary>Registra esta sessão como a atual.</summary>
        void Awake() => Current = this;

        /// <summary>Marca o instante de início do cronômetro.</summary>
        void Start() => startedAt = Time.time;

        /// <summary>Limpa a referência global quando a cena é descarregada.</summary>
        /// <remarks>Só limpa se a sessão atual ainda for esta.</remarks>
        void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        /// <summary>Conclui a fase.</summary>
        /// <remarks>
        /// Ignora chamadas repetidas; calcula o tempo final, salva progresso/recorde no ProgressStore
        /// e avisa os ouvintes (painel de conclusão) pelo evento Completed.
        /// </remarks>
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
