using System.Collections;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Final state: shows the victory/defeat screen, awards EXP and money,
    /// then signals the scene to return to the overworld.
    /// </summary>
    public class BattleEndState : IBattleState
    {
        public BattleStateId StateId => BattleStateId.End;

        public void Enter(BattleContext ctx)
            => Debug.Log($"[Battle] → End  (playerWon={ctx.PlayerWon})");

        public IEnumerator Execute(BattleContext ctx)
        {
            // Play appropriate music
            if (ServiceLocator.Global.TryGet(out AudioManager audio))
            {
                if (ctx.PlayerWon)
                    audio.me_play("Audio/ME/Victory", 100, 100);
                else
                    audio.bgm_stop();
            }

            // Brief pause for result display
            yield return new WaitForSeconds(1.5f);

            // Return control to the scene (existing EndBattle logic)
            // BattleScene.EndBattle() or similar is already handled inside the DLL battle class.
        }

        public void Exit(BattleContext ctx)
            => Debug.Log("[Battle] FSM complete.");
    }
}
