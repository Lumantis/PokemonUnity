namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Interface that every mod DLL must implement on at least one public class.
    /// Mod DLLs are discovered by <see cref="ModLoader"/> via reflection.
    /// </summary>
    public interface IModPlugin
    {
        string ModId      { get; }
        string ModName    { get; }
        string ModVersion { get; }
        string ModAuthor  { get; }

        /// <summary>Called once when the mod is loaded. Register services or hooks here.</summary>
        void OnLoad(IServiceLocator services);

        /// <summary>Called when the mod is unloaded or the game closes.</summary>
        void OnUnload();

        /// <summary>Called when a new game session starts.</summary>
        void OnGameStart();

        /// <summary>Called every time a Unity scene is loaded.</summary>
        void OnSceneLoaded(string sceneName);

        /// <summary>Returns true if the mod is compatible with <paramref name="gameVersion"/>.</summary>
        bool IsCompatible(string gameVersion);
    }
}
