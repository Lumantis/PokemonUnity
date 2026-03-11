using System.Collections;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// First state: plays the battle intro animation then moves to ChooseCommand.
    /// </summary>
    public class BattleStartState : IBattleState
    {
        public BattleStateId StateId => BattleStateId.Start;

        public void Enter(BattleContext ctx)
        {
            Debug.Log("[Battle] → Start");
        }

        public IEnumerator Execute(BattleContext ctx)
        {
            // Give the scene one frame to settle before starting the battle
            yield return null;

            if (ctx.Scene != null)
                yield return ctx.Scene.StartCoroutine(ctx.Scene.StartBattle(ctx.Battle));

            ctx.TurnNumber = 0;
            ctx.TransitionTo(new ChooseCommandState());
        }

        public void Exit(BattleContext ctx) { }
    }
}
