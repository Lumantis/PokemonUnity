using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Static service locator. One global instance accessible everywhere.
    /// Call <see cref="Global"/> to obtain it.
    /// </summary>
    public sealed class ServiceLocator : IServiceLocator
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        private static ServiceLocator _global;

        /// <summary>The application-wide service locator.</summary>
        public static ServiceLocator Global
            => _global ??= new ServiceLocator();

        /// <summary>Replace the global locator (useful in tests).</summary>
        public static void SetGlobal(ServiceLocator locator)
            => _global = locator;

        // ── Registry ───────────────────────────────────────────────────────────
        private readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

        private ServiceLocator() { }

        /// <inheritdoc/>
        public void Register<T>(T service) where T : class
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _registry[typeof(T)] = service;
        }

        /// <inheritdoc/>
        public T Get<T>() where T : class
        {
            if (_registry.TryGetValue(typeof(T), out object obj))
                return (T)obj;
            throw new InvalidOperationException(
                $"[ServiceLocator] Service of type '{typeof(T).Name}' is not registered.");
        }

        /// <inheritdoc/>
        public bool TryGet<T>(out T service) where T : class
        {
            if (_registry.TryGetValue(typeof(T), out object obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        /// <inheritdoc/>
        public void Unregister<T>() where T : class
            => _registry.Remove(typeof(T));

        /// <inheritdoc/>
        public void Clear() => _registry.Clear();
    }

    /// <summary>
    /// MonoBehaviour bootstrapper that registers core Unity services into
    /// <see cref="ServiceLocator.Global"/> on Awake.
    /// Attach this to the GameManager GameObject.
    /// </summary>
    public class GameServices : MonoBehaviour
    {
        [SerializeField] private GameManager    gameManager;
        [SerializeField] private AudioManager   audioManager;
        [SerializeField] private InputManager   inputManager;
        [SerializeField] private SaveManager    saveManager;
        [SerializeField] private AchievementManager achievementManager;

        private void Awake()
        {
            var loc = ServiceLocator.Global;

            if (gameManager       != null) loc.Register(gameManager);
            if (audioManager      != null) loc.Register(audioManager);
            if (inputManager      != null) loc.Register(inputManager);
            if (saveManager       != null) loc.Register(saveManager);
            if (achievementManager!= null) loc.Register(achievementManager);

            Debug.Log("[GameServices] Core services registered.");
        }

        private void OnDestroy()
        {
            // Unregister on scene unload to avoid stale references
            ServiceLocator.Global.Clear();
        }
    }
}
