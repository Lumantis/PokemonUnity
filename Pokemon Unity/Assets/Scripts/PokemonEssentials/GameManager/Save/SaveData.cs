using System;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Versioned save data container. All game state is serialised here.
    /// Version is checked on load to trigger migrations when the schema changes.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // ── Schema version ────────────────────────────────────────────────────
        /// <summary>Current save-file schema version. Increment when adding fields.</summary>
        public const int CURRENT_VERSION = 1;

        public int    version   = CURRENT_VERSION;
        public string timestamp = DateTime.UtcNow.ToString("o"); // ISO 8601

        // ── Player info ───────────────────────────────────────────────────────
        public string playerName   = "";
        public int    playtimeSeconds;
        public int    badgeCount;
        public int    money;
        public string currentLocation = "";
        public float  positionX;
        public float  positionY;

        // ── Pokémon / Bag / Party ─────────────────────────────────────────────
        /// <summary>
        /// JSON-encoded party (IPokemon[6]).
        /// Serialised by the game layer using Newtonsoft.Json.
        /// </summary>
        public string partyJson   = "";

        /// <summary>JSON-encoded bag contents.</summary>
        public string bagJson     = "";

        /// <summary>JSON-encoded Pokédex state.</summary>
        public string pokedexJson = "";

        // ── Settings ──────────────────────────────────────────────────────────
        public float masterVolume = 1f;
        public float musicVolume  = 1f;
        public float sfxVolume    = 1f;
        public int   textSpeed    = 1;   // 0 = slow, 1 = normal, 2 = fast
        public bool  battleAnimations = true;

        // ── Achievements ──────────────────────────────────────────────────────
        /// <summary>JSON-encoded achievement states.</summary>
        public string achievementsJson = "";

        // ── Slot display header (loaded without deserialising full data) ───────
        /// <summary>
        /// Returns a short human-readable summary for the save-select screen.
        /// </summary>
        public string GetDisplayHeader()
        {
            TimeSpan ts = TimeSpan.FromSeconds(playtimeSeconds);
            return $"{playerName}  |  {badgeCount} badges  |  "
                 + $"{(int)ts.TotalHours:00}:{ts.Minutes:00}  |  {currentLocation}";
        }
    }
}
