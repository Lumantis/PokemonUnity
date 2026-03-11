using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Manages save slots: write, read, delete and schema migration.
    /// Saves are stored in <see cref="Application.persistentDataPath"/>/saves/.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static SaveManager Instance { get; private set; }

        // ── Config ────────────────────────────────────────────────────────────
        [SerializeField] private int maxSlots = 3;
        private const string SaveFolder = "saves";
        private const string SavePrefix = "save_slot_";
        private const string SaveExt    = ".json";

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<int>      OnSaved;
        public event Action<int>      OnLoaded;
        public event Action<int>      OnDeleted;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else                  { Destroy(gameObject); return; }

            Directory.CreateDirectory(SaveDirectory());
        }

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Returns true if a save file exists for <paramref name="slot"/>.</summary>
        public bool HasSave(int slot)
            => File.Exists(SlotPath(slot));

        /// <summary>Serialise and write <paramref name="data"/> to <paramref name="slot"/>.</summary>
        public void Save(int slot, SaveData data)
        {
            if (!IsValidSlot(slot)) return;

            data.version   = SaveData.CURRENT_VERSION;
            data.timestamp = DateTime.UtcNow.ToString("o");

            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(SlotPath(slot), json);
                Debug.Log($"[SaveManager] Slot {slot} saved → {SlotPath(slot)}");
                OnSaved?.Invoke(slot);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Save failed (slot {slot}): {ex.Message}");
            }
        }

        /// <summary>
        /// Load and deserialise <paramref name="slot"/>.
        /// Applies schema migration if the file version is older.
        /// Returns null if the slot does not exist or is corrupt.
        /// </summary>
        public SaveData Load(int slot)
        {
            if (!IsValidSlot(slot) || !HasSave(slot)) return null;

            try
            {
                string json = File.ReadAllText(SlotPath(slot));
                SaveData data = JsonConvert.DeserializeObject<SaveData>(json);

                if (data.version < SaveData.CURRENT_VERSION)
                    data = MigrateData(data);

                Debug.Log($"[SaveManager] Slot {slot} loaded (schema v{data.version}).");
                OnLoaded?.Invoke(slot);
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Load failed (slot {slot}): {ex.Message}");
                return null;
            }
        }

        /// <summary>Delete the save file for <paramref name="slot"/>.</summary>
        public void DeleteSave(int slot)
        {
            if (!IsValidSlot(slot)) return;
            string path = SlotPath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveManager] Slot {slot} deleted.");
                OnDeleted?.Invoke(slot);
            }
        }

        /// <summary>
        /// Read only the header of every slot (cheap — used in save-select screen).
        /// </summary>
        public SaveData[] GetAllSaveHeaders()
        {
            var headers = new SaveData[maxSlots];
            for (int i = 0; i < maxSlots; i++)
                headers[i] = HasSave(i) ? Load(i) : null;
            return headers;
        }

        // ── Migration ─────────────────────────────────────────────────────────
        /// <summary>
        /// Apply incremental migrations from <c>data.version</c> up to
        /// <see cref="SaveData.CURRENT_VERSION"/>.
        /// Add a new case here every time CURRENT_VERSION is incremented.
        /// </summary>
        private SaveData MigrateData(SaveData data)
        {
            // Example migration pattern (v0 → v1):
            // if (data.version < 1) { data.someNewField = defaultValue; data.version = 1; }

            Debug.Log($"[SaveManager] Migrated save data from v{data.version} to v{SaveData.CURRENT_VERSION}.");
            data.version = SaveData.CURRENT_VERSION;
            return data;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private string SaveDirectory() =>
            Path.Combine(Application.persistentDataPath, SaveFolder);

        private string SlotPath(int slot) =>
            Path.Combine(SaveDirectory(), $"{SavePrefix}{slot}{SaveExt}");

        private bool IsValidSlot(int slot)
        {
            if (slot >= 0 && slot < maxSlots) return true;
            Debug.LogWarning($"[SaveManager] Invalid slot index: {slot} (max {maxSlots - 1}).");
            return false;
        }
    }
}
