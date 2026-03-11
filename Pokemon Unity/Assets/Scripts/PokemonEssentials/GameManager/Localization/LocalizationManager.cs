using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Runtime localisation manager.
    /// Loads JSON string tables from Resources/Localization/{lang}.json.
    /// JSON format: { "KEY": "Translated value", ... }
    /// Falls back to the key itself if no translation is found.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static LocalizationManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField] private SystemLanguage defaultLanguage = SystemLanguage.English;

        // ── State ─────────────────────────────────────────────────────────────
        private Dictionary<string, string> _strings = new Dictionary<string, string>();
        private SystemLanguage              _current;

        /// <summary>Currently active language.</summary>
        public SystemLanguage CurrentLanguage => _current;

        /// <summary>Fired when the active language changes.</summary>
        public event Action<SystemLanguage> OnLanguageChanged;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else                  { Destroy(gameObject); return; }

            // Auto-detect system language; fall back to default
            SystemLanguage sys = Application.systemLanguage;
            SetLanguage(sys);
        }

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>
        /// Returns the localised string for <paramref name="key"/>.
        /// Falls back to <paramref name="key"/> itself if not found.
        /// </summary>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return _strings.TryGetValue(key, out string val) ? val : key;
        }

        /// <summary>
        /// Returns the localised string for <paramref name="key"/> with
        /// <see cref="string.Format"/> applied to the result.
        /// </summary>
        public string Get(string key, params object[] args)
        {
            string raw = Get(key);
            try   { return string.Format(raw, args); }
            catch { return raw; }
        }

        /// <summary>Switch the active language and reload strings.</summary>
        public void SetLanguage(SystemLanguage lang)
        {
            if (!LoadStrings(lang) && lang != defaultLanguage)
            {
                Debug.LogWarning($"[Localization] No table for {lang}, falling back to {defaultLanguage}.");
                LoadStrings(defaultLanguage);
                _current = defaultLanguage;
            }
            else
            {
                _current = lang;
            }

            OnLanguageChanged?.Invoke(_current);
        }

        // ── Private ───────────────────────────────────────────────────────────
        private bool LoadStrings(SystemLanguage lang)
        {
            // Resource path: Resources/Localization/{lang}.json  e.g. "English.json"
            string resourcePath = $"Localization/{lang}";
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);

            if (asset == null)
            {
                // Also try short ISO code (e.g., "en", "fr", "ja")
                string iso = LanguageToISO(lang);
                asset = Resources.Load<TextAsset>($"Localization/{iso}");
            }

            if (asset == null)
            {
                Debug.LogWarning($"[Localization] File not found: Resources/Localization/{lang}.json");
                return false;
            }

            try
            {
                _strings = JsonConvert.DeserializeObject<Dictionary<string, string>>(asset.text)
                           ?? new Dictionary<string, string>();
                Debug.Log($"[Localization] Loaded {_strings.Count} strings for {lang}.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Localization] Parse error for {lang}: {ex.Message}");
                return false;
            }
        }

        private static string LanguageToISO(SystemLanguage lang) => lang switch
        {
            SystemLanguage.English    => "en",
            SystemLanguage.French     => "fr",
            SystemLanguage.German     => "de",
            SystemLanguage.Spanish    => "es",
            SystemLanguage.Italian    => "it",
            SystemLanguage.Japanese   => "ja",
            SystemLanguage.Korean     => "ko",
            SystemLanguage.Chinese    => "zh",
            SystemLanguage.Portuguese => "pt",
            _                         => lang.ToString().ToLowerInvariant()
        };
    }
}
