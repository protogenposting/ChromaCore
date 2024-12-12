using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ChromaCore.Code.Objects;
using ChromaCore.Code.Utils.Network;

namespace ChromaCore.Code.Scenes
{
    internal class InGameNetplay : InGame
    {
        public override bool InfiniteTime => false;

        public InGameNetplay(Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : base(players, controllerPorts, controllerProfiles, palettes, entryRoom) { }
        public InGameNetplay(NetConnection connection, Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : base(connection, players, controllerPorts, controllerProfiles, palettes, entryRoom) { }

        public override void AdvanceFrame()
        {
            if (winner == 0)
            {
                base.AdvanceFrame();
                if (players[0].health <= 0 && players[1].health <= 0)
                {
                    winner = -1;
                    UI.AddElement(new TextBlock("Draw!", "ExampleContent/UIFont", Color.White, "", 960, 400));
                    winScreenTimer++;
                }
                else if (!(players[0].health > 0 && players[1].health > 0))
                {
                    if (players[0].health <= 0) winner = 2;
                    else winner = 1;

                    UI.AddElement(new TextBlock("Player " + winner + " wins!", "ExampleContent/UIFont", Color.White, "", 960, 400));
                    winScreenTimer++;
                }

                if (versusTimer <= 0 && winner == 0)
                {
                    if ((float)players[0].health / players[0].healthMax == (float)players[1].health / players[1].healthMax)
                    {
                        winner = -1;
                        UI.AddElement(new TextBlock("Draw!", "ExampleContent/UIFont", Color.White, "", 960, 400));
                        winScreenTimer++;
                    }
                    else
                    {
                        if ((float)players[0].health / players[0].healthMax > (float)players[1].health / players[1].healthMax) winner = 1;
                        else winner = 2;

                        UI.AddElement(new TextBlock("Player " + winner + " wins!", "ExampleContent/UIFont", Color.White, "", 960, 400));
                        winScreenTimer++;
                    }
                }

                if (winner > 0 && versusTimer > 0)
                {
                    Fighter notWinner = players[winner % 2];
                    if (notWinner.Grounded)
                    {
                        notWinner.velocity = new Vector2(-4 * notWinner.direction, -8);
                        notWinner.hitstunTimer = 2;
                    }
                    notWinner.hitstunProperties = new List<Fighter.HitstunProperties>() { Fighter.HitstunProperties.Knockdown };
                }
            }
            else
            {
                Unpause(null, null);
                foreach (Fighter p in players) if (p.health <= 0 && p.Grounded && p.velocity.Y >= 0)
                    {
                        p.knockdownTimer = 60;
                        p.animation = p.knockdownAnim;
                    }
                base.AdvanceFrame();
                UpdateCursors();
            }

            if (winScreenTimer == 90)
            {
                Game.Instance.ChangeScene(new CharacterSelect(netcodeManager.connections[0], netcodeManager.connections[0].playerController));
            }
        }

        protected override void UpdateControllers()
        {
            if (winScreenTimer == 0)
            {
                base.UpdateControllers();
            }
            else
            {
                foreach (Fighter p in players) p.input.ClearAllInputs();
            }
        }

        public override void Pause(int controllerID)
        {
            if (winScreenTimer <= 0) base.Pause(controllerID);
        }

        void Rematch(MenuCursor cursor, ButtonMatrix matrix)
        {
            Game.Instance.ChangeScene(new InGameVersus(new Type[] { players[0].GetType(), players[1].GetType() }, new int[] { players[0].input.id, players[1].input.id }, controllerProfiles.ToArray(), new int[]
            {
                players[0].currentPalette, players[1].currentPalette
            }, roomType));
        }
        void QuitToMenu(MenuCursor cursor, ButtonMatrix matrix) => Game.Instance.ChangeScene(new MainMenu());
    }
}
