using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RCArena.Code.Objects;
using RCArena.Code.Utils.Network;

namespace RCArena.Code.Scenes
{
    internal class InGameVersus : InGame
    {
        public override bool InfiniteTime => false;

        public InGameVersus(Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : base(players, controllerPorts, controllerProfiles, palettes, entryRoom) { }
        public InGameVersus(NetConnection connection, Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : base(connection, players, controllerPorts, controllerProfiles, palettes, entryRoom) { }

        public override void Update(GameTime gametime)
        {
            if (winner == 0)
            {
                base.Update(gametime);
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
                base.Update(gametime);
                UpdateCursors();
            }

            if (winScreenTimer > 0)
            {
                if (winScreenTimer == 45)
                {
                    ButtonMatrix matrix = new ButtonMatrix(new UIElement[1, 3], null, false) { cursorOffset = new Vector2(-128, 0), multiCursorOffset = new Vector2(0, 32) };
                    matrix.buttons[0, 0] = new TextBlock("Rematch", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", 960, 480)
                    {
                        onClick = Rematch
                    };
                    matrix.buttons[0, 1] = new TextBlock("Character Select", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", 960, 620)
                    {
                        onClick = ReturnToCSS
                    };
                    matrix.buttons[0, 2] = new TextBlock("Main Menu", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", 960, 760)
                    {
                        onClick = QuitToMenu
                    };

                    UI.buttonMatricies.Add(matrix);

                    foreach (Fighter p in players) if (p.input.id >= -1) cursors.Add(new MenuCursor(p.input.id, matrix));
                }
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

        void Rematch(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            Game.Instance.ChangeScene(new InGameVersus(new Type[] { players[0].GetType(), players[1].GetType() }, new int[] { players[0].input.id, players[1].input.id }, controllerProfiles.ToArray(), new int[]
            {
                players[0].currentPalette, players[1].currentPalette
            }, roomType));
        }
        void QuitToMenu(MenuCursor cursor, ButtonMatrix matrix, UIElement button) => Game.Instance.ChangeScene(new MainMenu());
    }
}
