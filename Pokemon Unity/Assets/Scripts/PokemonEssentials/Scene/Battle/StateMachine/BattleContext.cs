using PokemonEssentials.Interface.PokeBattle;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Shared data passed between battle states.
    /// Holds all mutable state for the ongoing battle.
    /// </summary>
    public class BattleContext
    {
        // ── Core references ───────────────────────────────────────────────────
        public IBattleIE    Battle;
        public BattleScene  Scene;

        // ── Player decision (set by ChooseCommandState) ───────────────────────
        public MenuCommands PlayerCommand    = MenuCommands.NONE;
        public int          PlayerMoveIndex  = -1;
        public int          PlayerTargetIndex = 0;

        // ── Round tracking ────────────────────────────────────────────────────
        public bool IsPlayerTurn   = true;
        public int  TurnNumber     = 0;

        // ── End condition ─────────────────────────────────────────────────────
        public bool BattleEnded = false;
        public bool PlayerWon   = false;

        // ── State machine back-reference ──────────────────────────────────────
        internal BattleStateMachine StateMachine;

        /// <summary>Request a transition to <paramref name="newState"/>.</summary>
        public void TransitionTo(IBattleState newState)
            => StateMachine.TransitionTo(newState);
    }
}
