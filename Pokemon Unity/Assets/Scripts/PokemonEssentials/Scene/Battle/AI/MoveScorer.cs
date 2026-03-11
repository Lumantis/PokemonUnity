using System;
using System.Collections.Generic;
using UnityEngine;
using PokemonUnity.Combat;
using PokemonEssentials.Interface.PokeBattle;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Stateless scoring utility for Pokémon moves.
    /// Used by <see cref="TrainerAI"/> to rank available moves.
    /// Higher score = better choice for the attacker.
    /// </summary>
    public static class MoveScorer
    {
        // ── Type effectiveness table (single-key: attackType * 20 + defType) ─
        // Values: 0=immune, 50=not very effective, 100=normal, 200=super effective
        private static readonly Dictionary<int, int> _typeChart =
            BuildTypeChart();

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Score a move in [0..1000]. Higher is better.
        /// </summary>
        public static float ScoreMove(IMove move, IBattlerIE attacker, IBattlerIE defender)
        {
            if (move == null || attacker == null || defender == null) return 0f;

            // Status moves use a separate heuristic
            if (move.baseDamage == 0)
                return ScoreStatusMove(move, attacker, defender);

            float score = move.baseDamage;

            // STAB bonus (+50%)
            score *= GetSTAB(move.type, attacker);

            // Type effectiveness
            score *= GetEffectiveness(move.type, defender) / 100f;

            // Accuracy penalty (miss chance)
            if (move.accuracy > 0)
                score *= move.accuracy / 100f;

            // PP scarcity: avoid moves with 1 PP left
            if (move.pp == 1) score *= 0.5f;

            // Priority bonus (good for finishing off weakened foes)
            if (move.priority > 0 && defender.hp < defender.totalhp * 0.3f)
                score *= 1.3f;

            return Mathf.Max(score, 0f);
        }

        /// <summary>Return the type effectiveness multiplier × 100 (100 = normal).</summary>
        public static int GetEffectiveness(Types attackType, IBattlerIE defender)
        {
            int mult = 100;
            // Defender may have 1 or 2 types
            mult = mult * GetTypeMultiplier(attackType, defender.type1) / 100;
            if (defender.type2 != defender.type1)
                mult = mult * GetTypeMultiplier(attackType, defender.type2) / 100;
            return mult;
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static float GetSTAB(Types moveType, IBattlerIE attacker)
        {
            if (moveType == attacker.type1 || moveType == attacker.type2)
                return 1.5f;
            return 1.0f;
        }

        private static float ScoreStatusMove(IMove move, IBattlerIE attacker, IBattlerIE defender)
        {
            // Simple heuristic: value status moves based on expected impact
            float score = 40f; // base value for any status move

            // Boost moves are less useful at high HP (already healthy)
            if (attacker.hp > attacker.totalhp * 0.7f)
                score += 20f;

            // Moves that worsen opponent's stats are valuable
            // (we can't easily check effect without deep move internals)
            return score;
        }

        private static int GetTypeMultiplier(Types attack, Types defense)
        {
            int key = (int)attack * 50 + (int)defense;
            return _typeChart.TryGetValue(key, out int v) ? v : 100;
        }

        // ── Type chart (Gen 6+) ───────────────────────────────────────────────
        private static Dictionary<int, int> BuildTypeChart()
        {
            var d = new Dictionary<int, int>();

            void Add(Types a, Types def, int mult)
                => d[(int)a * 50 + (int)def] = mult;

            // FIRE
            Add(Types.FIRE,   Types.GRASS,  200);
            Add(Types.FIRE,   Types.ICE,    200);
            Add(Types.FIRE,   Types.BUG,    200);
            Add(Types.FIRE,   Types.STEEL,  200);
            Add(Types.FIRE,   Types.FIRE,    50);
            Add(Types.FIRE,   Types.WATER,   50);
            Add(Types.FIRE,   Types.ROCK,    50);
            Add(Types.FIRE,   Types.DRAGON,  50);
            // WATER
            Add(Types.WATER,  Types.FIRE,   200);
            Add(Types.WATER,  Types.GROUND, 200);
            Add(Types.WATER,  Types.ROCK,   200);
            Add(Types.WATER,  Types.WATER,   50);
            Add(Types.WATER,  Types.GRASS,   50);
            Add(Types.WATER,  Types.DRAGON,  50);
            // ELECTRIC
            Add(Types.ELECTRIC, Types.WATER,   200);
            Add(Types.ELECTRIC, Types.FLYING,  200);
            Add(Types.ELECTRIC, Types.ELECTRIC, 50);
            Add(Types.ELECTRIC, Types.GRASS,    50);
            Add(Types.ELECTRIC, Types.DRAGON,   50);
            Add(Types.ELECTRIC, Types.GROUND,    0);
            // GRASS
            Add(Types.GRASS, Types.WATER,  200);
            Add(Types.GRASS, Types.GROUND, 200);
            Add(Types.GRASS, Types.ROCK,   200);
            Add(Types.GRASS, Types.FIRE,    50);
            Add(Types.GRASS, Types.GRASS,   50);
            Add(Types.GRASS, Types.POISON,  50);
            Add(Types.GRASS, Types.FLYING,  50);
            Add(Types.GRASS, Types.BUG,     50);
            Add(Types.GRASS, Types.DRAGON,  50);
            Add(Types.GRASS, Types.STEEL,   50);
            // ICE
            Add(Types.ICE, Types.GRASS,  200);
            Add(Types.ICE, Types.GROUND, 200);
            Add(Types.ICE, Types.FLYING, 200);
            Add(Types.ICE, Types.DRAGON, 200);
            Add(Types.ICE, Types.FIRE,    50);
            Add(Types.ICE, Types.WATER,   50);
            Add(Types.ICE, Types.ICE,     50);
            Add(Types.ICE, Types.STEEL,   50);
            // FIGHTING
            Add(Types.FIGHTING, Types.NORMAL, 200);
            Add(Types.FIGHTING, Types.ICE,    200);
            Add(Types.FIGHTING, Types.ROCK,   200);
            Add(Types.FIGHTING, Types.DARK,   200);
            Add(Types.FIGHTING, Types.STEEL,  200);
            Add(Types.FIGHTING, Types.POISON,  50);
            Add(Types.FIGHTING, Types.FLYING,  50);
            Add(Types.FIGHTING, Types.PSYCHIC, 50);
            Add(Types.FIGHTING, Types.BUG,     50);
            Add(Types.FIGHTING, Types.FAIRY,   50);
            Add(Types.FIGHTING, Types.GHOST,    0);
            // POISON
            Add(Types.POISON, Types.GRASS,  200);
            Add(Types.POISON, Types.FAIRY,  200);
            Add(Types.POISON, Types.POISON,  50);
            Add(Types.POISON, Types.GROUND,  50);
            Add(Types.POISON, Types.ROCK,    50);
            Add(Types.POISON, Types.GHOST,   50);
            Add(Types.POISON, Types.STEEL,    0);
            // GROUND
            Add(Types.GROUND, Types.FIRE,    200);
            Add(Types.GROUND, Types.ELECTRIC,200);
            Add(Types.GROUND, Types.POISON,  200);
            Add(Types.GROUND, Types.ROCK,    200);
            Add(Types.GROUND, Types.STEEL,   200);
            Add(Types.GROUND, Types.GRASS,    50);
            Add(Types.GROUND, Types.BUG,      50);
            Add(Types.GROUND, Types.FLYING,    0);
            // FLYING
            Add(Types.FLYING, Types.GRASS,    200);
            Add(Types.FLYING, Types.FIGHTING, 200);
            Add(Types.FLYING, Types.BUG,      200);
            Add(Types.FLYING, Types.ELECTRIC,  50);
            Add(Types.FLYING, Types.ROCK,      50);
            Add(Types.FLYING, Types.STEEL,     50);
            // PSYCHIC
            Add(Types.PSYCHIC, Types.FIGHTING, 200);
            Add(Types.PSYCHIC, Types.POISON,   200);
            Add(Types.PSYCHIC, Types.PSYCHIC,   50);
            Add(Types.PSYCHIC, Types.STEEL,     50);
            Add(Types.PSYCHIC, Types.DARK,       0);
            // BUG
            Add(Types.BUG, Types.GRASS,    200);
            Add(Types.BUG, Types.PSYCHIC,  200);
            Add(Types.BUG, Types.DARK,     200);
            Add(Types.BUG, Types.FIRE,      50);
            Add(Types.BUG, Types.FIGHTING,  50);
            Add(Types.BUG, Types.FLYING,    50);
            Add(Types.BUG, Types.GHOST,     50);
            Add(Types.BUG, Types.STEEL,     50);
            Add(Types.BUG, Types.FAIRY,     50);
            // ROCK
            Add(Types.ROCK, Types.FIRE,    200);
            Add(Types.ROCK, Types.ICE,     200);
            Add(Types.ROCK, Types.FLYING,  200);
            Add(Types.ROCK, Types.BUG,     200);
            Add(Types.ROCK, Types.FIGHTING, 50);
            Add(Types.ROCK, Types.GROUND,   50);
            Add(Types.ROCK, Types.STEEL,    50);
            // GHOST
            Add(Types.GHOST, Types.PSYCHIC, 200);
            Add(Types.GHOST, Types.GHOST,   200);
            Add(Types.GHOST, Types.DARK,     50);
            Add(Types.GHOST, Types.NORMAL,    0);
            // DRAGON
            Add(Types.DRAGON, Types.DRAGON, 200);
            Add(Types.DRAGON, Types.STEEL,   50);
            Add(Types.DRAGON, Types.FAIRY,    0);
            // DARK
            Add(Types.DARK, Types.PSYCHIC,  200);
            Add(Types.DARK, Types.GHOST,    200);
            Add(Types.DARK, Types.FIGHTING,  50);
            Add(Types.DARK, Types.DARK,      50);
            Add(Types.DARK, Types.FAIRY,     50);
            // STEEL
            Add(Types.STEEL, Types.ICE,    200);
            Add(Types.STEEL, Types.ROCK,   200);
            Add(Types.STEEL, Types.FAIRY,  200);
            Add(Types.STEEL, Types.FIRE,    50);
            Add(Types.STEEL, Types.WATER,   50);
            Add(Types.STEEL, Types.ELECTRIC,50);
            Add(Types.STEEL, Types.STEEL,   50);
            // FAIRY
            Add(Types.FAIRY, Types.FIGHTING,200);
            Add(Types.FAIRY, Types.DRAGON,  200);
            Add(Types.FAIRY, Types.DARK,    200);
            Add(Types.FAIRY, Types.FIRE,     50);
            Add(Types.FAIRY, Types.POISON,   50);
            Add(Types.FAIRY, Types.STEEL,    50);

            return d;
        }
    }
}
