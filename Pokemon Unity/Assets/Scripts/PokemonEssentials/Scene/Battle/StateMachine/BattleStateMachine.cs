using System;
using System.Collections;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// MonoBehaviour orchestrator for the battle FSM.
    /// Attach this to the same GameObject as <see cref="BattleScene"/>.
    /// Call <see cref="StartBattleFSM"/> to begin.
    /// </summary>
    public class BattleStateMachine : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [SerializeField] private BattleScene battleScene;

        // ── State ─────────────────────────────────────────────────────────────
        private IBattleState _currentState;
        private BattleContext _ctx;
        private Coroutine    _stateCoroutine;

        /// <summary>Currently active state (read-only).</summary>
        public IBattleState CurrentState => _currentState;

        /// <summary>Fires when a state transition occurs (from, to).</summary>
        public event Action<IBattleState, IBattleState> OnStateChanged;

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Initialise the FSM with an active battle and start in BattleStartState.</summary>
        public void StartBattleFSM(IBattleIE battle)
        {
            _ctx = new BattleContext
            {
                Battle       = battle,
                Scene        = battleScene,
                StateMachine = this
            };

            TransitionTo(new BattleStartState());
        }

        /// <summary>
        /// Request a transition to <paramref name="newState"/>.
        /// Can be called from inside a state's Execute coroutine.
        /// </summary>
        public void TransitionTo(IBattleState newState)
        {
            if (newState == null) return;

            if (_stateCoroutine != null)
            {
                StopCoroutine(_stateCoroutine);
                _stateCoroutine = null;
            }

            IBattleState prev = _currentState;
            prev?.Exit(_ctx);

            _currentState = newState;
            OnStateChanged?.Invoke(prev, _currentState);

            _currentState.Enter(_ctx);
            _stateCoroutine = StartCoroutine(RunState(_currentState));
        }

        // ── Private ───────────────────────────────────────────────────────────
        private IEnumerator RunState(IBattleState state)
        {
            yield return StartCoroutine(state.Execute(_ctx));
            _stateCoroutine = null;
        }
    }
}
