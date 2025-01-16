using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using RCArena.Code.Objects;
using RCArena.Code.Objects.Players.Characters;
using RCArena.Code.Utils.Network;

namespace RCArena.Code.Scenes
{
    public class InGameTraining : InGame
    {
        GamePadState prevPad1State = GamePad.GetState(PlayerIndex.One);
        KeyboardState prevKeyboardState = Keyboard.GetState();

        public bool frameByFrameMode = false;
        public bool displayHitboxes = false;
        public DISetting diSetting = DISetting.None;
        public int comboDamage = 0;

        private int damageDisplay;
        private int frameAdvantage;
        private bool bothCommited;
        public int startupDisplay;
        private int holdFrameAdvantage;

        //Character Settings
        public Dictionary<string, int> characterSettings = new()
        {
            { "JetHeatSetting", 0 },
        };

        public override bool PlayIntro => false;

        Vector2[] lastResetPositions;
        int lastResetFlipped = 0;

        public InGameTraining(Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : base(players, controllerPorts, controllerProfiles, palettes, entryRoom) { }
        public InGameTraining(NetConnection connection, Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : base(connection, players, controllerPorts, controllerProfiles, palettes, entryRoom) { }

        public override void Update(GameTime gametime)
        {
            GamePadState pad1State = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboardState = Keyboard.GetState();

            if (pad1State.IsButtonDown(Buttons.Back) && !prevPad1State.IsButtonDown(Buttons.Back) || keyboardState.IsKeyDown(Keys.Escape) && !prevKeyboardState.IsKeyDown(Keys.Escape))
            {
                frameByFrameMode = !frameByFrameMode;
            }

            if (paused) frameByFrameMode = false;

            if (frameByFrameMode)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && !prevKeyboardState.IsKeyDown(Keys.Tab))
                {
                    base.Update(gametime);
                    UpdateTrainingData();
                }
                else
                {
                    foreach (Fighter p in players) p.UpdateHurtboxes();
                    if (hitpause > 0) hitpause--;
                }
            }
            else
            {
                base.Update(gametime);
                if (!paused) UpdateTrainingData();
            }

            //Quick Restart
            if (((keyboardState.IsKeyDown(Keys.F5) && !prevKeyboardState.IsKeyDown(Keys.F5)) ||
                (players[0].input.KeyDown(Controller.Key_MenuLB) && players[0].input.KeyDown(Controller.Key_MenuRB) && players[0].input.KeyPressed(Controller.Key_MenuConfirm))) && Game.Instance.screenTransition == null)
            {
                players[0].input.ClearAllBuffers();
                int dir = players[0].inputDir;
                int yDir = (players[0].input.KeyDown(Controller.Key_Down) ? 1 : 0) - (players[0].input.KeyDown(Controller.Key_Up) ? 1 : 0);
                Game.Instance.screenTransition = new FadeToBlackTransition()
                {
                    duration = 12,
                    transitionTime = 6,
                    onTransition = () =>
                    {
                        InGameTraining scene = new InGameTraining([players[0].GetType(), players[1].GetType()], [players[0].input.id, players[1].input.id], controllerProfiles.ToArray(),
                        [
                            players[0].currentPalette, players[1].currentPalette
                        ], roomType)
                        { displayHitboxes = displayHitboxes, diSetting = diSetting, characterSettings = characterSettings, music = music };

                        Game.Instance.ChangeScene(scene, (s) =>
                        {
                            if (yDir == 1 || lastResetPositions == null)
                            {
                                lastResetPositions = scene.room.spawn;
                            }
                            if (dir == 1)
                            {
                                lastResetPositions[(0 + lastResetFlipped) % 2].X = scene.room.bounds.Width - 160;
                                lastResetPositions[(1 + lastResetFlipped) % 2].X = scene.room.bounds.Width - 96;
                            }
                            if (dir == -1)
                            {
                                lastResetPositions[(0 + lastResetFlipped) % 2].X = 96;
                                lastResetPositions[(1 + lastResetFlipped) % 2].X = 160;
                            }
                            if (yDir == -1)
                            {
                                float temp = lastResetPositions[0].X;
                                lastResetPositions[0].X = lastResetPositions[1].X;
                                lastResetPositions[1].X = temp;
                                lastResetFlipped = (lastResetFlipped + 1) % 2;
                            }
                            scene.room.spawn = lastResetPositions;
                            scene.lastResetPositions = lastResetPositions;
                            scene.lastResetFlipped = lastResetFlipped;
                            scene.players[0].position = scene.room.spawn[0];
                            scene.players[1].position = scene.room.spawn[1];
                            scene.camera.Update(true);
                        });
                    }
                };
            }

            prevPad1State = GamePad.GetState(PlayerIndex.One);
            prevKeyboardState = Keyboard.GetState();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch);

            if (!paused)
            {
                spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
                spritebatch.DrawString(Fonts.exampleFont, "Damage: " + damageDisplay, new Vector2(100, 200), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.9f);
                spritebatch.DrawString(Fonts.exampleFont, "Frame Advantage: " + holdFrameAdvantage, new Vector2(100, 280), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.9f);
                spritebatch.DrawString(Fonts.exampleFont, "Attack Startup: " + startupDisplay, new Vector2(100, 340), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.9f);
                if (frameByFrameMode) spritebatch.DrawString(Fonts.exampleFont, "Frame-by-frame mode enabled\nPress Tab to advance the frame\nPress " + (players[0].input.id == -1 ? "Escape " : "Select ") + "to exit", new Vector2(40, 960), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.9f);
                spritebatch.End();
            }
        }

        protected override List<UIElement> GetPauseMenuButtons()
        {
            List<UIElement> list = base.GetPauseMenuButtons();
            list.Insert(1, new TextBlock("Training Settings", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
            {
                onClick = OpenTrainingSettingsMenu
            });
            return list;
        }

        public void OpenTrainingSettingsMenu(MenuCursor c, ButtonMatrix m, UIElement button)
        {
            UI.buttonMatricies.Remove(pauseMenuUI);

            TrainingDummyController dummyController = null;
            if (players.Exists(p => p.input is TrainingDummyController)) dummyController = (TrainingDummyController)players.First(p => p.input is TrainingDummyController).input;

            List<UIElement> buttons = new List<UIElement>();
            buttons.Add(new TextBlock("Hitboxes: " + (displayHitboxes ? "On" : "Off"), "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
            {
                onClick = (c, m, b) =>
                {
                    displayHitboxes = !displayHitboxes;
                    ((TextBlock)b).text = "Hitboxes: " + (displayHitboxes ? "On" : "Off");
                },
                layer = 0.7f
            });

            if (dummyController != null)
            {
                buttons.Add(new TextBlock("DI: " + diSetting.ToString(), "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = (c, m, b) =>
                    {
                        diSetting++;
                        if (diSetting == DISetting.Length) diSetting = DISetting.None;

                        ((TextBlock)b).text = "DI: " + diSetting.ToString();
                    },
                    layer = 0.7f
                });
                buttons.Add(new TextBlock("Dummy: " + dummyController.action.ToString(), "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = (c, m, b) =>
                    {
                        dummyController.action++;
                        if (dummyController.action == TrainingDummyActions.EnumLength) dummyController.action = 0;

                        ((TextBlock)b).text = "Dummy: " + dummyController.action.ToString();
                    },
                    layer = 0.7f
                });

                buttons.Add(new TextBlock("Dummy Block: " + dummyController.blockMode.ToString(), "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = (c, m, b) =>
                    {
                        dummyController.blockMode++;
                        if (dummyController.blockMode == TrainingDummyBlockTypes.EnumLength) dummyController.blockMode = 0;

                        ((TextBlock)b).text = "Dummy Block: " + dummyController.blockMode.ToString();
                    },
                    layer = 0.7f
                });
            }

            if (players.Any(p => p is Jet))
            {
                buttons.Add(new TextBlock("Heat: " + Jet.heatSettings[characterSettings["JetHeatSetting"]], "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = (c, m, b) =>
                    {
                        characterSettings["JetHeatSetting"]++;
                        if (characterSettings["JetHeatSetting"] >= Jet.heatSettings.Count) characterSettings["JetHeatSetting"] = 0;
                        ((TextBlock)b).text = "Heat: " + Jet.heatSettings[characterSettings["JetHeatSetting"]];
                    },
                    layer = 0.7f
                });
            }

            ButtonMatrix settingsUI = ButtonMatrix.Create1DMatrix(true, 180, 240, 120, buttons, (c, m) =>
            {
                UI.buttonMatricies.Remove(m);
                UI.buttonMatricies.Add(pauseMenuUI);
                cursors[0].SwitchMatrix(pauseMenuUI, 0, 1);
            }, true);

            UI.buttonMatricies.Add(settingsUI);
            cursors[0].SwitchMatrix(settingsUI);
        }

        private void UpdateTrainingData()
        {
            if (players.Count > 1)
            {
                if (players[1].hitstunTimer > 0 || players[1].knockdownTimer > 10)
                {
                    damageDisplay = comboDamage;
                }
                else comboDamage = 0;
                if (hitpause == 0 && prepareHitpause == 0 || frameByFrameMode)
                {
                    bool p1Commited = players[0].CommitedState && (players[0].state != Fighter.States.Guard || players[0].blockStun > 0);
                    bool p2Commited = players[1].CommitedState && (players[1].state != Fighter.States.Guard || players[1].blockStun > 0);

                    if (!p1Commited && p2Commited) frameAdvantage++;
                    if (p1Commited && !p2Commited) frameAdvantage--;
                    if (p1Commited ^ p2Commited) holdFrameAdvantage = frameAdvantage;
                    else frameAdvantage = 0;
                    if (!p1Commited && !p2Commited && bothCommited) holdFrameAdvantage = 0;
                    if (p1Commited && p2Commited) bothCommited = true;
                    else bothCommited = false;
                }
            }
        }

        protected override void ReturnToCSS(MenuCursor c, ButtonMatrix m, UIElement button)
        {
            Game.Instance.ChangeScene(new CharacterSelect([players[0].input.id, players[1].input.id]) { gamemode = Gamemodes.Training });
        }

        public enum DISetting
        {
            None,
            In,
            Out,
            Length
        }
    }
}