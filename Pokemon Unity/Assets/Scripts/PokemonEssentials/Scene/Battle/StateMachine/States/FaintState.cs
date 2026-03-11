using System.Collections;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Handles one or more Pokémon fainting.
    /// Checks win/loss condition and transitions accordingly.
    /// </summary>
    public class FaintState : IBattleState
    {
        public BattleStateId StateId => BattleStateId.Faint;

        public void Enter(BattleContext ctx)
            => Debug.Log("[Battle] → Faint");

        public IEnumerator Execute(BattleContext ctx)
        {
            yield return null; // one frame for animations to start

            if (ctx.Battle == null)
            {
                ctx.TransitionTo(new ChooseCommandState());
                yield break;
            }

            bool playerAllFainted   = AllFainted(ctx, playerSide: true);
            bool opponentAllFainted = AllFainted(ctx, playerSide: false);

            if (playerAllFainted || opponentAllFainted)
            {
                ctx.BattleEnded = true;
                ctx.PlayerWon   = opponentAllFainted && !playerAllFainted;
                ctx.TransitionTo(new BattleEndState());
            }
            else
            {
                // Someone fainted but the battle continues — choose replacement
                ctx.TransitionTo(new ChooseCommandState());
            }
        }

        public void Exit(BattleContext ctx) { }

        // ── Helpers ───────────────────────────────────────────────────────────
        private bool AllFainted(BattleContext ctx, bool playerSide)
        {
            var battlers = ctx.Battle.battlers;
            for (int i = 0; i < battlers.Length; i++)
            {
                var b = battlers[i];
                if (b == null) continue;
                bool isPlayer = b.index % 2 == 0;
                if (isPlayer == playerSide && b.hp > 0)
                    return false;
            }
            return true;
        }
    }
}
