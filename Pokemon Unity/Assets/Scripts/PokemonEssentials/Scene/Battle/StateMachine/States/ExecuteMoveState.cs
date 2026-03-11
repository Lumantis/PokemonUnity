using System.Collections;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Executes the chosen moves for this turn (player + AI opponent).
    /// After execution transitions to <see cref="FaintState"/> or back to
    /// <see cref="ChooseCommandState"/>.
    /// </summary>
    public class ExecuteMoveState : IBattleState
    {
        public BattleStateId StateId => BattleStateId.ExecuteMove;

        public void Enter(BattleContext ctx)
        {
            ctx.TurnNumber++;
            Debug.Log($"[Battle] → ExecuteMove (turn {ctx.TurnNumber})");
        }

        public IEnumerator Execute(BattleContext ctx)
        {
            // AI selects a move via TrainerAI if present
            TrainerAI ai = ctx.Scene != null
                ? ctx.Scene.GetComponent<TrainerAI>()
                : null;

            // Execute player move (delegate to BattleScene's existing coroutine logic)
            // The scene's internal battle loop handles speed order, damage, etc.
            // We simply yield one frame to let existing Update-based logic tick.
            yield return null;

            // Check faint conditions
            bool anyFainted = false;
            if (ctx.Battle != null)
            {
                for (int i = 0; i < ctx.Battle.battlers.Length; i++)
                {
                    var b = ctx.Battle.battlers[i];
                    if (b != null && b.hp <= 0)
                    {
                        anyFainted = true;
                        break;
                    }
                }
            }

            if (anyFainted)
                ctx.TransitionTo(new FaintState());
            else
                ctx.TransitionTo(new ChooseCommandState());
        }

        public void Exit(BattleContext ctx) { }
    }
}
