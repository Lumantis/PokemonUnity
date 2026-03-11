using System;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Lightweight service locator interface.
    /// Prefer constructor injection where possible; use this for cross-cutting
    /// Unity singletons (AudioManager, InputManager, etc.).
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>Register a service instance under its type.</summary>
        void Register<T>(T service) where T : class;

        /// <summary>Retrieve a registered service. Throws if not registered.</summary>
        T Get<T>() where T : class;

        /// <summary>Try to retrieve a service without throwing.</summary>
        bool TryGet<T>(out T service) where T : class;

        /// <summary>Remove a service registration.</summary>
        void Unregister<T>() where T : class;

        /// <summary>Remove all registrations (useful in tests).</summary>
        void Clear();
    }
}
