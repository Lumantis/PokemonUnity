using System;
using System.Collections.Generic;
using UnityEngine;
using PokemonEssentials.Interface.PokeBattle;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Selects moves and Pokémon replacements for an AI trainer.
    /// Attach to the same GameObject as <see cref="BattleScene"/>.
    /// </summary>
    public class TrainerAI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField]
        [Range(0, 4)]
        [Tooltip("0 = random, 4 = always optimal")]
        private int difficultyLevel = 2;

        private System.Random _rng;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
            => _rng = new System.Random();

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the index [0–3] of the move the AI should use this turn.
        /// Returns -1 if no valid move found (struggle).
        /// </summary>
        public int SelectMoveIndex(IBattlerIE attacker, IBattlerIE defender, IBattleIE battle)
        {
            if (attacker == null || defender == null) return 0;

            int[]   sorted = GetSortedMoveIndices(attacker, defender);
            int     count  = sorted.Length;

            if (count == 0) return -1;

            switch (difficultyLevel)
            {
                case 0:
                    // Fully random
                    return sorted[_rng.Next(count)];

                case 1:
                    // Pick randomly from the bottom 75%
                    return sorted[_rng.Next(Mathf.Max(1, count * 3 / 4), count)];

                case 2:
                    // Pick randomly from the top 50%
                    return sorted[_rng.Next(0, Mathf.Max(1, count / 2))];

                case 3:
                    // Pick randomly from the top 25%
                    return sorted[_rng.Next(0, Mathf.Max(1, count / 4))];

                case 4:
                default:
                    // Always pick the best
                    return sorted[0];
            }
        }

        /// <summary>
        /// Select the best replacement Pokémon after a faint.
        /// Returns the party index of the best available Pokémon, or -1 if none.
        /// </summary>
        public int SelectReplacementIndex(IBattleIE battle, int trainerSide)
        {
            if (battle == null) return -1;

            float  bestScore  = -1f;
            int    bestIndex  = -1;
            var    battlers   = battle.battlers;
            IBattlerIE opponent = null;

            // Find the current player battler as reference for scoring
            for (int i = 0; i < battlers.Length; i++)
                if (battlers[i] != null && battlers[i].index % 2 != trainerSide % 2)
                    opponent = battlers[i];

            // Score each conscious party member
            for (int i = 0; i < battle.party2.Length; i++)
            {
                var pkmn = battle.party2[i];
                if (pkmn == null || pkmn.hp <= 0) continue;

                // Placeholder score: higher HP % = better candidate
                float score = (float)pkmn.hp / Mathf.Max(1, pkmn.totalhp);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        // ── Private ───────────────────────────────────────────────────────────

        /// <summary>Returns move indices sorted best-to-worst.</summary>
        private int[] GetSortedMoveIndices(IBattlerIE attacker, IBattlerIE defender)
        {
            var moves = attacker.moves;
            if (moves == null || moves.Length == 0) return Array.Empty<int>();

            var scored = new List<(int idx, float score)>();
            for (int i = 0; i < moves.Length; i++)
            {
                var m = moves[i];
                if (m == null || m.pp <= 0) continue;
                float s = MoveScorer.ScoreMove(m, attacker, defender);
                scored.Add((i, s));
            }

            scored.Sort((a, b) => b.score.CompareTo(a.score));

            int[] result = new int[scored.Count];
            for (int i = 0; i < scored.Count; i++)
                result[i] = scored[i].idx;
            return result;
        }
    }
}
