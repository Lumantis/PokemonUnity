using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Wrapper around <see cref="Resources.Load"/> that adds a weak-reference
    /// cache and async loading helpers.
    /// Drop-in replacement ready for future migration to Addressables.
    /// </summary>
    public static class ResourceLoader
    {
        // ── Cache ─────────────────────────────────────────────────────────────
        private static readonly Dictionary<string, WeakReference<UnityEngine.Object>>
            _cache = new Dictionary<string, WeakReference<UnityEngine.Object>>(StringComparer.OrdinalIgnoreCase);

        // ── Sync ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Load an asset of type <typeparamref name="T"/> from Resources.
        /// Result is cached; repeated calls return the cached instance.
        /// </summary>
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) return null;

            // Check cache
            if (_cache.TryGetValue(path, out var wr) && wr.TryGetTarget(out var cached))
                return cached as T;

            T asset = Resources.Load<T>(path);
            if (asset != null)
                _cache[path] = new WeakReference<UnityEngine.Object>(asset);
            else
                Debug.LogWarning($"[ResourceLoader] Asset not found: '{path}'");

            return asset;
        }

        /// <summary>Returns true if a resource exists at <paramref name="path"/>.</summary>
        public static bool Exists(string path, Type type)
            => Resources.Load(path, type) != null;

        // ── Async ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Asynchronously load an asset and invoke <paramref name="onComplete"/>.
        /// Use inside a coroutine:
        /// <code>yield return StartCoroutine(ResourceLoader.LoadAsync&lt;Sprite&gt;("Icons/pikachu", s => icon.sprite = s));</code>
        /// </summary>
        public static IEnumerator LoadAsync<T>(string path, Action<T> onComplete)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) { onComplete?.Invoke(null); yield break; }

            // Return cached synchronously if available
            if (_cache.TryGetValue(path, out var wr) && wr.TryGetTarget(out var cached))
            {
                onComplete?.Invoke(cached as T);
                yield break;
            }

            ResourceRequest req = Resources.LoadAsync<T>(path);
            yield return req;

            T asset = req.asset as T;
            if (asset != null)
                _cache[path] = new WeakReference<UnityEngine.Object>(asset);
            else
                Debug.LogWarning($"[ResourceLoader] Async: Asset not found: '{path}'");

            onComplete?.Invoke(asset);
        }

        // ── Cache management ──────────────────────────────────────────────────
        /// <summary>Remove a specific path from the cache.</summary>
        public static void Unload(string path)
        {
            if (_cache.Remove(path))
                Resources.UnloadUnusedAssets();
        }

        /// <summary>Clear the entire cache and unload unused assets.</summary>
        public static void ClearCache()
        {
            _cache.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}
