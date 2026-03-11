using System;
using System.Collections.Generic;
using UnityEngine;
using PokemonUnity.Combat;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Generates procedural "fakemon" Pokémon data from a numeric seed.
    /// Results are fully deterministic for a given seed.
    /// </summary>
    public static class ProceduralPokemonGenerator
    {
        // ── Result container ──────────────────────────────────────────────────
        [Serializable]
        public class ProceduralPokemonData
        {
            public string      name;
            public Types       primaryType;
            public Types       secondaryType;  // Types.NONE = single-type
            public int[]       baseStats;      // [HP, Atk, Def, SpAtk, SpDef, Spe] — length 6
            public int         bst;            // Base Stat Total
            public Moves[]     learnset;       // 4–8 moves
            public GrowthRate  growthRate;
            public int         catchRate;
            public float       genderRatio;    // 0=male-only 127.5=50/50 254=female-only 255=genderless
            public int         seed;           // original seed for reproducibility
        }

        // ── Type weight table (frequency-based, approximated from Gen 1–9) ────
        private static readonly (Types type, float weight)[] TypeWeights =
        {
            (Types.NORMAL,   8f), (Types.FIRE,    6f), (Types.WATER,    8f),
            (Types.ELECTRIC, 5f), (Types.GRASS,   7f), (Types.ICE,      4f),
            (Types.FIGHTING, 5f), (Types.POISON,  5f), (Types.GROUND,   5f),
            (Types.FLYING,   7f), (Types.PSYCHIC, 6f), (Types.BUG,      7f),
            (Types.ROCK,     5f), (Types.GHOST,   4f), (Types.DRAGON,   3f),
            (Types.DARK,     4f), (Types.STEEL,   4f), (Types.FAIRY,    4f),
        };

        // Stat distribution profiles per type [HP, Atk, Def, SpAtk, SpDef, Spe]
        private static readonly Dictionary<Types, float[]> StatProfiles =
            new Dictionary<Types, float[]>
        {
            { Types.FIRE,     new[]{ 0.14f, 0.16f, 0.12f, 0.22f, 0.12f, 0.24f } },
            { Types.WATER,    new[]{ 0.18f, 0.15f, 0.18f, 0.17f, 0.17f, 0.15f } },
            { Types.ELECTRIC, new[]{ 0.12f, 0.14f, 0.12f, 0.20f, 0.12f, 0.30f } },
            { Types.GRASS,    new[]{ 0.16f, 0.15f, 0.18f, 0.18f, 0.18f, 0.15f } },
            { Types.ICE,      new[]{ 0.16f, 0.14f, 0.16f, 0.22f, 0.18f, 0.14f } },
            { Types.FIGHTING, new[]{ 0.16f, 0.28f, 0.18f, 0.10f, 0.14f, 0.14f } },
            { Types.POISON,   new[]{ 0.16f, 0.16f, 0.16f, 0.18f, 0.20f, 0.14f } },
            { Types.GROUND,   new[]{ 0.16f, 0.24f, 0.18f, 0.10f, 0.16f, 0.16f } },
            { Types.FLYING,   new[]{ 0.14f, 0.16f, 0.14f, 0.16f, 0.14f, 0.26f } },
            { Types.PSYCHIC,  new[]{ 0.14f, 0.10f, 0.14f, 0.26f, 0.20f, 0.16f } },
            { Types.BUG,      new[]{ 0.14f, 0.18f, 0.18f, 0.14f, 0.16f, 0.20f } },
            { Types.ROCK,     new[]{ 0.14f, 0.20f, 0.28f, 0.12f, 0.16f, 0.10f } },
            { Types.GHOST,    new[]{ 0.14f, 0.16f, 0.14f, 0.22f, 0.22f, 0.12f } },
            { Types.DRAGON,   new[]{ 0.16f, 0.20f, 0.18f, 0.20f, 0.14f, 0.12f } },
            { Types.DARK,     new[]{ 0.16f, 0.22f, 0.14f, 0.16f, 0.16f, 0.16f } },
            { Types.STEEL,    new[]{ 0.14f, 0.18f, 0.30f, 0.14f, 0.16f, 0.08f } },
            { Types.FAIRY,    new[]{ 0.16f, 0.12f, 0.16f, 0.22f, 0.24f, 0.10f } },
            { Types.NORMAL,   new[]{ 0.18f, 0.16f, 0.16f, 0.16f, 0.16f, 0.18f } },
        };

        // ── Name syllables for procedural names ───────────────────────────────
        private static readonly string[] Prefixes =
        {
            "Vex","Zar","Pyx","Nor","Vel","Tor","Fen","Gal","Hyx","Dro",
            "Lum","Ryx","Sev","Wyr","Bri","Cal","Eks","Jov","Kae","Myx"
        };
        private static readonly string[] Suffixes =
        {
            "don","mon","eon","ite","ax","ex","ix","oz","us","ar",
            "on","en","yn","in","um","ux","yx","os","as","es"
        };

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Generate a complete ProceduralPokemonData from <paramref name="seed"/>.
        /// <paramref name="targetBST"/> guides the overall power level (default 500).
        /// </summary>
        public static ProceduralPokemonData Generate(int seed, int targetBST = 500)
        {
            var rng = new System.Random(seed);

            // Clamp BST to a reasonable range
            targetBST = Mathf.Clamp(targetBST, 200, 720);

            Types primary   = SelectType(rng);
            Types secondary = SelectSecondType(rng, primary);

            int[]      stats      = DistributeStats(rng, primary, targetBST);
            Moves[]    learnset   = SelectMoves(rng, primary, secondary, stats);
            GrowthRate growthRate = SelectGrowthRate(targetBST);
            string     name       = GenerateName(rng);

            return new ProceduralPokemonData
            {
                name         = name,
                primaryType  = primary,
                secondaryType= secondary,
                baseStats    = stats,
                bst          = SumStats(stats),
                learnset     = learnset,
                growthRate   = growthRate,
                catchRate    = SelectCatchRate(rng, targetBST),
                genderRatio  = SelectGenderRatio(rng),
                seed         = seed
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static Types SelectType(System.Random rng)
        {
            float total = 0f;
            foreach (var (_, w) in TypeWeights) total += w;
            float r = (float)(rng.NextDouble() * total);
            float acc = 0f;
            foreach (var (t, w) in TypeWeights)
            {
                acc += w;
                if (r <= acc) return t;
            }
            return Types.NORMAL;
        }

        private static Types SelectSecondType(System.Random rng, Types primary)
        {
            // 60% chance of being dual-type
            if (rng.NextDouble() > 0.6) return Types.NONE;
            Types second;
            int attempts = 0;
            do { second = SelectType(rng); attempts++; }
            while (second == primary && attempts < 10);
            return second == primary ? Types.NONE : second;
        }

        private static int[] DistributeStats(System.Random rng, Types type, int bst)
        {
            float[] profile = StatProfiles.TryGetValue(type, out float[] p)
                ? p
                : new[] { 0.167f, 0.167f, 0.167f, 0.167f, 0.167f, 0.167f };

            int[] stats = new int[6];
            int   remaining = bst;

            for (int i = 0; i < 5; i++)
            {
                // Add ±15% jitter to each stat
                float jitter = 1f + (float)(rng.NextDouble() * 0.3 - 0.15);
                int   val    = Mathf.RoundToInt(bst * profile[i] * jitter);
                val          = Mathf.Clamp(val, 20, 255);
                stats[i]     = val;
                remaining   -= val;
            }
            // Last stat gets the remainder, clamped
            stats[5] = Mathf.Clamp(remaining, 20, 255);

            return stats;
        }

        private static Moves[] SelectMoves(System.Random rng, Types primary, Types secondary, int[] stats)
        {
            // Build a candidate move pool from both types
            // (In a real game this would query the move database — here we use a fixed sample)
            var pool = new List<Moves>
            {
                Moves.TACKLE, Moves.SCRATCH, Moves.POUND,   // Normal coverage
                Moves.GROWL,  Moves.LEER,    Moves.TAIL_WHIP // Status
            };

            // Shuffle and pick 4–6 moves
            int count = rng.Next(4, 7);
            count     = Mathf.Min(count, pool.Count);
            ShuffleList(pool, rng);

            var result = new Moves[count];
            for (int i = 0; i < count; i++)
                result[i] = pool[i];
            return result;
        }

        private static GrowthRate SelectGrowthRate(int bst)
        {
            if (bst >= 580) return GrowthRate.Slow;
            if (bst >= 500) return GrowthRate.MediumSlow;
            if (bst >= 400) return GrowthRate.MediumFast;
            return GrowthRate.Fast;
        }

        private static int SelectCatchRate(System.Random rng, int bst)
        {
            // Higher BST = harder to catch
            if (bst >= 580) return rng.Next(3, 30);
            if (bst >= 500) return rng.Next(30, 90);
            if (bst >= 400) return rng.Next(90, 150);
            return rng.Next(150, 255);
        }

        private static float SelectGenderRatio(System.Random rng)
        {
            float[] ratios = { 0f, 31.3f, 50f, 75f, 100f, 127.5f, 254f, 255f };
            return ratios[rng.Next(ratios.Length)];
        }

        private static string GenerateName(System.Random rng)
        {
            string pre = Prefixes[rng.Next(Prefixes.Length)];
            string suf = Suffixes[rng.Next(Suffixes.Length)];
            return pre + suf;
        }

        private static int SumStats(int[] stats)
        {
            int sum = 0;
            foreach (int s in stats) sum += s;
            return sum;
        }

        private static void ShuffleList<T>(List<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
