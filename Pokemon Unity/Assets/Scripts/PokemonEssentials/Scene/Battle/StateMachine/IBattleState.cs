using System.Collections;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>Unique identifier for each battle state.</summary>
    public enum BattleStateId
    {
        Idle,
        Start,
        ChooseCommand,
        ChooseTarget,
        ExecuteMove,
        Faint,
        SwitchPokemon,
        End
    }

    /// <summary>
    /// Interface for a single step in the battle FSM.
    /// All state logic is pure C# (no MonoBehaviour).
    /// </summary>
    public interface IBattleState
    {
        BattleStateId StateId { get; }

        /// <summary>Called once when entering this state.</summary>
        void Enter(BattleContext ctx);

        /// <summary>
        /// Main coroutine. May yield and call
        /// <see cref="BattleContext.TransitionTo"/> to move to the next state.
        /// </summary>
        IEnumerator Execute(BattleContext ctx);

        /// <summary>Called once when leaving this state.</summary>
        void Exit(BattleContext ctx);
    }
}
