using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Tracks, persists and broadcasts achievement state.
    /// Save file: Application.persistentDataPath/achievements.json
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static AchievementManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField] private AchievementCollection collection;
        [SerializeField] private float notificationDuration = 3f;

        // ── Persistence ───────────────────────────────────────────────────────
        [Serializable]
        private class AchievementState
        {
            public bool   unlocked   = false;
            public int    progress   = 0;
            public string unlockedAt = "";  // ISO timestamp
        }

        private Dictionary<string, AchievementState> _states
            = new Dictionary<string, AchievementState>();

        private const string SaveFile = "achievements.json";

        // ── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired when an achievement is unlocked for the first time.</summary>
        public event Action<AchievementDefinition> OnAchievementUnlocked;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else                  { Destroy(gameObject); return; }

            LoadState();
        }

        private void OnApplicationPause(bool pause) { if (pause) SaveState(); }
        private void OnApplicationQuit()            => SaveState();

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Returns true if achievement <paramref name="id"/> has been unlocked.</summary>
        public bool IsUnlocked(string id)
            => GetState(id).unlocked;

        /// <summary>Returns the current progress value for <paramref name="id"/>.</summary>
        public int GetProgress(string id)
            => GetState(id).progress;

        /// <summary>Directly unlock an achievement (bypasses progress check).</summary>
        public void Unlock(string id)
        {
            AchievementState state = GetState(id);
            if (state.unlocked) return;

            state.unlocked   = true;
            state.unlockedAt = DateTime.UtcNow.ToString("o");

            AchievementDefinition def = collection?.Get(id);
            if (def != null)
            {
                Debug.Log($"[Achievement] Unlocked: {def.title}");
                OnAchievementUnlocked?.Invoke(def);
            }

            SaveState();
        }

        /// <summary>Add <paramref name="amount"/> to a progressive achievement's counter.</summary>
        public void AddProgress(string id, int amount = 1)
        {
            if (IsUnlocked(id)) return;

            AchievementDefinition def = collection?.Get(id);
            if (def == null || !def.isProgressive) return;

            AchievementState state = GetState(id);
            state.progress = Mathf.Min(state.progress + amount, def.progressGoal);

            if (state.progress >= def.progressGoal)
                Unlock(id);
        }

        /// <summary>Set the progress counter to an absolute value.</summary>
        public void SetProgress(string id, int value)
        {
            if (IsUnlocked(id)) return;

            AchievementDefinition def = collection?.Get(id);
            if (def == null || !def.isProgressive) return;

            AchievementState state = GetState(id);
            state.progress = Mathf.Clamp(value, 0, def.progressGoal);

            if (state.progress >= def.progressGoal)
                Unlock(id);
        }

        /// <summary>Export current state as JSON (for inclusion in SaveData).</summary>
        public string ExportJson()
            => JsonConvert.SerializeObject(_states);

        /// <summary>Import state from JSON (called when loading a save).</summary>
        public void ImportJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var loaded = JsonConvert.DeserializeObject<Dictionary<string, AchievementState>>(json);
                if (loaded != null) _states = loaded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AchievementManager] Import failed: {ex.Message}");
            }
        }

        // ── Persistence ───────────────────────────────────────────────────────
        private void SaveState()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, SaveFile);
                File.WriteAllText(path, JsonConvert.SerializeObject(_states, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AchievementManager] Save failed: {ex.Message}");
            }
        }

        private void LoadState()
        {
            string path = Path.Combine(Application.persistentDataPath, SaveFile);
            if (!File.Exists(path)) return;
            try
            {
                string json = File.ReadAllText(path);
                var loaded  = JsonConvert.DeserializeObject<Dictionary<string, AchievementState>>(json);
                if (loaded != null) _states = loaded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AchievementManager] Load failed: {ex.Message}");
            }
        }

        private AchievementState GetState(string id)
        {
            if (!_states.TryGetValue(id, out AchievementState state))
            {
                state     = new AchievementState();
                _states[id] = state;
            }
            return state;
        }
    }
}
