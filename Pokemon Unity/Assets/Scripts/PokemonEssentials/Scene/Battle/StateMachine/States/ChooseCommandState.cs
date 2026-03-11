using System.Collections;
using PokemonEssentials.Interface.PokeBattle;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Waits for the player to select Fight / Bag / Pokémon / Run.
    /// Sets <see cref="BattleContext.PlayerCommand"/> then transitions to
    /// <see cref="ExecuteMoveState"/> (or <see cref="BattleEndState"/> for Run).
    /// </summary>
    public class ChooseCommandState : IBattleState
    {
        public BattleStateId StateId => BattleStateId.ChooseCommand;

        public void Enter(BattleContext ctx)
        {
            ctx.PlayerCommand   = MenuCommands.NONE;
            ctx.PlayerMoveIndex = -1;
            Debug.Log("[Battle] → ChooseCommand");
        }

        public IEnumerator Execute(BattleContext ctx)
        {
            // Ask the scene to show the command menu and wait for the player's choice
            if (ctx.Scene != null)
            {
                yield return ctx.Scene.StartCoroutine(
                    ctx.Scene.CommandMenu(0, cmd => ctx.PlayerCommand = cmd));
            }
            else
            {
                // Headless / test mode: default to FIGHT
                ctx.PlayerCommand = MenuCommands.FIGHT;
                yield return null;
            }

            switch (ctx.PlayerCommand)
            {
                case MenuCommands.RUN:
                    ctx.BattleEnded = true;
                    ctx.PlayerWon   = false;
                    ctx.TransitionTo(new BattleEndState());
                    break;

                default:
                    ctx.TransitionTo(new ExecuteMoveState());
                    break;
            }
        }

        public void Exit(BattleContext ctx) { }
    }
}
