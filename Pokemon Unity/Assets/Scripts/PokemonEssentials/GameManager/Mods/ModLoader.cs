using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Discovers, loads and manages mod plugins from DLL files.
    /// Mod DLLs are placed in: <see cref="Application.persistentDataPath"/>/Mods/
    /// or <see cref="Application.streamingAssetsPath"/>/Mods/
    /// </summary>
    public class ModLoader : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static ModLoader Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField] private bool   loadFromPersistentData  = true;
        [SerializeField] private bool   loadFromStreamingAssets = false;
        [SerializeField] private string modSubfolder            = "Mods";
        [SerializeField] private string gameVersion             = "1.0.0";

        // ── State ─────────────────────────────────────────────────────────────
        private readonly List<IModPlugin>    _mods       = new List<IModPlugin>();
        private readonly List<Assembly>      _assemblies = new List<Assembly>();

        public IReadOnlyList<IModPlugin> LoadedMods => _mods;

        // ── Events ────────────────────────────────────────────────────────────
        public event Action<IModPlugin>           OnModLoaded;
        public event Action<string>               OnModUnloaded;
        public event Action<string, Exception>    OnModLoadFailed;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else                  { Destroy(gameObject); return; }

            SceneManager.sceneLoaded += (scene, _) => BroadcastSceneLoaded(scene.name);
        }

        private void Start() => DiscoverAndLoad();

        private void OnDestroy() => UnloadAll();

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Scan all configured mod folders and load valid DLLs.</summary>
        public void DiscoverAndLoad()
        {
            if (loadFromPersistentData)
                ScanFolder(Path.Combine(Application.persistentDataPath, modSubfolder));

            if (loadFromStreamingAssets)
                ScanFolder(Path.Combine(Application.streamingAssetsPath, modSubfolder));
        }

        /// <summary>Load a single mod DLL by path.</summary>
        public bool LoadMod(string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                Debug.LogWarning($"[ModLoader] DLL not found: {dllPath}");
                return false;
            }

            try
            {
                Assembly asm = Assembly.LoadFrom(dllPath);
                _assemblies.Add(asm);

                int found = 0;
                foreach (Type t in asm.GetExportedTypes())
                {
                    if (!typeof(IModPlugin).IsAssignableFrom(t) || t.IsAbstract) continue;

                    var plugin = (IModPlugin)Activator.CreateInstance(t);

                    if (!plugin.IsCompatible(gameVersion))
                    {
                        Debug.LogWarning($"[ModLoader] '{plugin.ModName}' is not compatible with game v{gameVersion}. Skipping.");
                        continue;
                    }

                    plugin.OnLoad(ServiceLocator.Global);
                    _mods.Add(plugin);
                    found++;
                    Debug.Log($"[ModLoader] Loaded mod: {plugin.ModName} v{plugin.ModVersion} by {plugin.ModAuthor}");
                    OnModLoaded?.Invoke(plugin);
                }

                if (found == 0)
                    Debug.LogWarning($"[ModLoader] No IModPlugin found in: {Path.GetFileName(dllPath)}");

                return found > 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModLoader] Failed to load {Path.GetFileName(dllPath)}: {ex.Message}");
                OnModLoadFailed?.Invoke(dllPath, ex);
                return false;
            }
        }

        /// <summary>Unload a specific mod by id.</summary>
        public void UnloadMod(string modId)
        {
            IModPlugin mod = _mods.Find(m => m.ModId == modId);
            if (mod == null) return;

            mod.OnUnload();
            _mods.Remove(mod);
            OnModUnloaded?.Invoke(modId);
        }

        /// <summary>Unload all mods.</summary>
        public void UnloadAll()
        {
            foreach (var m in _mods)
                try { m.OnUnload(); } catch (Exception ex) { Debug.LogWarning($"[ModLoader] Unload error: {ex.Message}"); }
            _mods.Clear();
        }

        /// <summary>Broadcast game-start to all loaded mods.</summary>
        public void BroadcastGameStart()
        {
            foreach (var m in _mods)
                try { m.OnGameStart(); } catch (Exception ex) { Debug.LogWarning(ex.Message); }
        }

        /// <summary>Broadcast scene-loaded to all loaded mods.</summary>
        public void BroadcastSceneLoaded(string sceneName)
        {
            foreach (var m in _mods)
                try { m.OnSceneLoaded(sceneName); } catch (Exception ex) { Debug.LogWarning(ex.Message); }
        }

        // ── Private ───────────────────────────────────────────────────────────
        private void ScanFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                return;
            }

            foreach (string dll in Directory.GetFiles(folder, "*.dll"))
                LoadMod(dll);
        }
    }
}
