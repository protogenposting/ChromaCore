using RCArena.Code.Objects.Players.Characters;
using RCArena.Code.Utils.Network;

namespace RCArena.Code.Scenes
{
    public class MainMenu : Scene
    {
        ButtonMatrix mainButtons;
        ButtonMatrix netplayButtons;
        ButtonMatrix settingsButtons;
        ButtonMatrix controlProfilesUI;

        public override void Load()
        {
            Game.Instance.music.FadeOut(120);

            UI = new UICanvas();

            //Main Menu Buttons
            List<UIElement> buttons = new List<UIElement>()
            {
                new TextBlock("Versus", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) => Game.Instance.ChangeScene(new CharacterSelect([0, -1]) { gamemode = Gamemodes.Versus }),
                    size = new Vector2(240, 80)
                },
                new TextBlock("Training", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) => Game.Instance.ChangeScene(new CharacterSelect([0, -1]) { gamemode = Gamemodes.Training }),
                    size = new Vector2(240, 80)
                },
                new TextBlock("Online", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) =>
                    {
                        mainButtons.enabled = false;
                        netplayButtons.enabled = true;
                        cursors[0].SwitchMatrix(netplayButtons);
                    },
                    size = new Vector2(240, 80)
                },
                new TextBlock("Options", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) =>
                    {
                        mainButtons.enabled = false;
                        settingsButtons.enabled = true;
                        cursors[0].SwitchMatrix(settingsButtons);
                    },
                    size = new Vector2(240, 80)
                },
                new TextBlock("Quit", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) => Game.Instance.Exit(),
                    size = new Vector2(240, 80)
                },
            };

            mainButtons = ButtonMatrix.Create1DMatrix(true, 960, 400, 140, buttons, null, true);
            UI.buttonMatricies.Add(mainButtons);

            //Netplay Menu Buttons
            buttons = new List<UIElement>()
            {
                new TextBlock("Host Game", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) =>
                    {
                        Game.Instance.ChangeScene(new ConnectionScreen() {player1Controller = c == null ? -1 : c.input.id});
                    },
                    size = new Vector2(240, 80)
                },
                new TextBox("", "Connect: ", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onEnter = t =>
                    {
                        Game.Instance.ChangeScene(new ConnectionScreen(t.entryText, 9000) {player1Controller = t.mouseTriggered ? -1 : cursors[0].input.id});
                    },
                    size = new Vector2(240, 80)
                },
            };
            netplayButtons = ButtonMatrix.Create1DMatrix(true, 960, 480, 160, buttons, (c, m) =>
            {
                netplayButtons.enabled = false;
                mainButtons.enabled = true;
                c.SwitchMatrix(mainButtons, 0, 2);
            }, false);
            netplayButtons.enabled = false;
            UI.buttonMatricies.Add(netplayButtons);

            //Options Menu Buttons
            buttons = new List<UIElement>()
            {
                new TextBlock("Music: " + (DataManager.musicVolume > 0 ? "On" : "Off"), "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) =>
                    {
                        if (DataManager.musicVolume > 0)
                        {
                            DataManager.musicVolume = 0;
                            ((TextBlock)b).text = "Music: Off";
                        }
                        else
                        {
                            DataManager.musicVolume = 1;
                            ((TextBlock)b).text = "Music: On";
                        }
                    },
                    size = new Vector2(240, 80)
                },
                new TextBlock("Sounds: " + (DataManager.soundEffectVolume > 0 ? "On" : "Off"), "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) =>
                    {
                        if (DataManager.soundEffectVolume > 0)
                        {
                            DataManager.soundEffectVolume = 0;
                            ((TextBlock)b).text = "Sounds: Off";
                        }
                        else
                        {
                            DataManager.soundEffectVolume = 1;
                            ((TextBlock)b).text = "Sounds: On";
                        }
                    },
                    size = new Vector2(240, 80)
                },
                new TextBlock("Control Profiles", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m, b) =>
                    {
                        SetupControlProfileUI();
                    },
                    size = new Vector2(240, 80)
                },
            };
            settingsButtons = ButtonMatrix.Create1DMatrix(true, 960, 480, 160, buttons, (c, m) =>
            {
                settingsButtons.enabled = false;
                mainButtons.enabled = true;
                c.SwitchMatrix(mainButtons, 0, 3);
                DataManager.SaveSettings();
            }, false);
            settingsButtons.enabled = false;
            UI.buttonMatricies.Add(settingsButtons);

            cursors.Add(new SharedMenuCursor(mainButtons));
        }

        public override void Update(GameTime gametime)
        {
            UI.Update();
            foreach (MenuCursor c in cursors) c.Update();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Game.Instance.GraphicsDevice.Clear(Color.SlateGray);

            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp);
            UI.Draw(spritebatch);
            foreach (MenuCursor c in cursors) c.Draw(spritebatch);
            spritebatch.End();
        }

        public override void UpdateCursors()
        {
            if (!UI.elements.Exists(e => e is PopupMessage)) foreach (MenuCursor c in cursors.ToArray())
                {
                    c.Update();
                }
        }

        ControlProfile selectedControlProfile = null;
        void SetupControlProfileUI()
        {
            if (controlProfilesUI != null) UI.buttonMatricies.Remove(controlProfilesUI);
            List<UIElement> buttons = new List<UIElement>() { new TextBlock("Add Profile", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
            {
                onClick = AddControlProfile,
                size = new Vector2(240, 80)
            }};

            for (int i = 0; i < DataManager.controllerProfiles.Count && i < 8; i++)
            {
                ControlProfile prof = DataManager.controllerProfiles[i];
                buttons.Add(new TextBlock(prof.name, "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = OpenProfileSettings,
                    size = new Vector2(240, 80)
                });
            }
            controlProfilesUI = ButtonMatrix.Create1DMatrix(true, 200, 100, 100, buttons, (c, m) =>
            {
                controlProfilesUI.enabled = false;
                settingsButtons.enabled = true;
                cursors[0].SwitchMatrix(settingsButtons, 0, 2);
            }, true, (b, i) => b.position.Y += 50 * Math.Sign(i));
            controlProfilesUI.cursorOffset = new Vector2(Game.LoadAsset<Texture2D>("UI/Menu/MenuButton").Width * -2, 0);
            controlProfilesUI.multiCursorOffset = new Vector2(0, 32);

            settingsButtons.enabled = false;
            controlProfilesUI.enabled = true;
            UI.buttonMatricies.Add(controlProfilesUI);
            cursors[0].SwitchMatrix(controlProfilesUI, 0, 0);
        }

        void AddControlProfile(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            if (DataManager.controllerProfiles.Count < 8)
            {
                DataManager.controllerProfiles.Add(new ControlProfile() { name = "New profile" });
                DataManager.SaveControlProfiles();
                SetupControlProfileUI();
                cursors[0].SwitchMatrix(controlProfilesUI, 0, DataManager.controllerProfiles.Count);
            }
        }

        void DeleteControlProfile(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            if (DataManager.controllerProfiles.IndexOf(selectedControlProfile) != 0) DataManager.controllerProfiles.Remove(selectedControlProfile);
            DataManager.SaveControlProfiles();
            SetupControlProfileUI();
            UI.buttonMatricies.Remove(matrix);
            matrix.Dispose();
            cursors[0].SwitchMatrix(controlProfilesUI, 0, 0);
        }

        void OpenProfileSettings(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            selectedControlProfile = DataManager.controllerProfiles[button.matrixID - 1];
            float targetY = controlProfilesUI.buttons[0, button.matrixID].position.Y;

            ButtonMatrix newMatrix = ButtonMatrix.Create1DMatrix(true, 600, (int)targetY - 200, 100, new List<UIElement>()
            {
                new TextBox(selectedControlProfile.name, "Name: ", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onEnter = ApplyProfileName,
                    size = new Vector2(240, 80)
                },
                new TextBlock("Edit Bindings", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = OpenBindList,
                    size = new Vector2(240, 80)
                },
                new TextBlock("Delete", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = DeleteControlProfile,
                    size = new Vector2(240, 80)
                }
            }, (c, m) =>
            {
                UI.buttonMatricies.Remove(m);
                m.Dispose();
                DataManager.SaveControlProfiles();
                SetupControlProfileUI();
                cursors[0].SwitchMatrix(controlProfilesUI, 0, DataManager.controllerProfiles.IndexOf(selectedControlProfile) + 1);
            }, false);
            newMatrix.cursorOffset = new Vector2(Game.LoadAsset<Texture2D>("UI/Menu/MenuButton").Width * -2, 0);
            newMatrix.multiCursorOffset = new Vector2(0, 32);

            UI.buttonMatricies.Add(newMatrix);
            cursors[0].SwitchMatrix(newMatrix);
        }

        void OpenBindList(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            int id = cursor == null ? -1 : cursor is SharedMenuCursor smc ? smc.lastPressedID : cursor.input.id;
            if (id == -1)
            {
                List<UIElement> buttons = new List<UIElement>();

                for (int i = 0; i < selectedControlProfile.keyBinds.Length; i++)
                {
                    buttons.Add(new ControlBindButton(DataManager.keyBindNames.ElementAt(i).Key + ": ", selectedControlProfile.keyBinds[i].ToString(),
                        id, selectedControlProfile, i, 1, "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton"));
                }

                ButtonMatrix newMatrix = ButtonMatrix.Create1DMatrix(true, 1000, 40, 90, buttons, (c, m) =>
                {
                    UI.buttonMatricies.Remove(UI.buttonMatricies[^1]);
                    UI.buttonMatricies[^1].Dispose();
                    cursors[0].SwitchMatrix(UI.buttonMatricies[^1], 0, 1);
                }, true);
                newMatrix.cursorOffset = new Vector2(Game.LoadAsset<Texture2D>("UI/Menu/MenuButton").Width * -2, 0);
                newMatrix.multiCursorOffset = new Vector2(0, 32);

                UI.buttonMatricies.Add(newMatrix);
                cursors[0].SwitchMatrix(newMatrix);
            }
            else
            {
                List<UIElement> buttons = new List<UIElement>();

                for (int i = 0; i < selectedControlProfile.padBinds.Length; i++)
                {
                    buttons.Add(new ControlBindButton(DataManager.keyBindNames.ElementAt(i + 4).Key + ": ", selectedControlProfile.padBinds[i].ToString(),
                        id, selectedControlProfile, i, 2, "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton"));
                }

                ButtonMatrix newMatrix = ButtonMatrix.Create1DMatrix(true, 1000, 100, 90, buttons, (c, m) =>
                {
                    UI.buttonMatricies.Remove(UI.buttonMatricies[^1]);
                    UI.buttonMatricies[^1].Dispose();
                    cursors[0].SwitchMatrix(UI.buttonMatricies[^1], 0, 1);
                    DataManager.SaveControlProfiles();
                }, true);
                newMatrix.cursorOffset = new Vector2(Game.LoadAsset<Texture2D>("UI/Menu/MenuButton").Width * -2, 0);
                newMatrix.multiCursorOffset = new Vector2(0, 32);

                UI.buttonMatricies.Add(newMatrix);
                cursors[0].SwitchMatrix(newMatrix);
            }
        }

        void ApplyProfileName(TextBox textbox)
        {
            selectedControlProfile.name = textbox.entryText;
            DataManager.SaveControlProfiles();
        }
    }

    class ControlBindButton : TextBlock
    {
        public bool active;
        public string nameText;
        public string keyText;

        private ButtonMatrix holdMatrix;
        private MenuCursor holdCursor;

        private ControlProfile profile;
        private int controllerID;
        private int bindID;
        /// <summary>
        /// 1 = Keyboard
        /// 2 = Gamepad
        /// </summary>
        private int type;

        KeyboardState prevKeyState = Keyboard.GetState();
        GamePadState prevPadState = GamePad.GetState(0);

        public ControlBindButton(string nameText, string keyText, int controllerID, ControlProfile profile, int bindID, int type, string fontURL, Color textColor, string texture, float x = 0, float y = 0) : base(nameText + keyText, fontURL, textColor, texture, x, y)
        {
            this.nameText = nameText;
            this.keyText = keyText;

            this.controllerID = controllerID;
            this.profile = profile;
            this.bindID = bindID;
            this.type = type;

            onClick = StartBindingButton;
            size = new Vector2(240, 80);
        }

        public override void Update()
        {
            base.Update();

            if (active)
            {
                if (type == 1)
                {
                    List<Keys> availableKeys = new List<Keys>();
                    for (Keys k = Keys.A; k <= Keys.Z; k++) availableKeys.Add(k);
                    for (Keys k = Keys.Left; k <= Keys.Down; k++) availableKeys.Add(k);

                    foreach (Keys k in availableKeys)
                    {
                        if (Keyboard.GetState().IsKeyDown(k) && !prevKeyState.IsKeyDown(k))
                        {
                            profile.keyBinds[bindID] = k;
                            holdCursor.input.UpdateKeys(1);
                            holdCursor.input.ClearAllBuffers();
                            holdCursor.SwitchMatrix(holdMatrix, 0, matrixID);
                            active = false;
                        }
                    }
                }
                if (type == 2)
                {
                    List<Buttons> availableButtons = new List<Buttons>()
                    {
                        Buttons.A,
                        Buttons.B,
                        Buttons.X,
                        Buttons.Y,
                        Buttons.LeftShoulder,
                        Buttons.RightShoulder,
                        Buttons.LeftTrigger,
                        Buttons.RightTrigger,
                        Buttons.LeftStick,
                        Buttons.RightStick
                    };

                    foreach (Buttons b in availableButtons)
                    {
                        if (GamePad.GetState(controllerID).IsButtonDown(Controller.CorrectedButton(controllerID, b)) && !prevPadState.IsButtonDown(Controller.CorrectedButton(controllerID, b)))
                        {
                            profile.padBinds[bindID] = b;
                            holdCursor.input.UpdateKeys(1);
                            holdCursor.input.ClearAllBuffers();
                            holdCursor.SwitchMatrix(holdMatrix, 0, matrixID);
                            active = false;
                        }
                    }
                }
                if ((Keyboard.GetState().IsKeyDown(Keys.Escape) && !prevKeyState.IsKeyDown(Keys.Escape)) || (GamePad.GetState(controllerID).IsButtonDown(Buttons.Back) && !prevPadState.IsButtonDown(Buttons.Back)))
                {
                    holdCursor.matrix = holdMatrix;
                    active = false;
                }
                prevKeyState = Keyboard.GetState();
                prevPadState = GamePad.GetState(controllerID);

                keyText = "...";
            }
            else keyText = type == 1 ? profile.keyBinds[bindID].ToString() : "";

            text = nameText + keyText;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch);
            if (!active && type == 2 && Controller.ControllerButtonIcons.ContainsKey(profile.padBinds[bindID]))
            {
                Texture2D spr = Controller.ControllerButtonIcons[profile.padBinds[bindID]];
                spritebatch.Draw(spr, position + new Vector2(80, 0), null, Color.White, 0, new Vector2(spr.Width, spr.Height) / 2, 2, SpriteEffects.None, 0.901f);
            }
        }

        void StartBindingButton(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            if (cursor == null) cursor = Game.Instance.Scene.cursors[0];
            if (matrix == null) matrix = cursor.matrix;
            active = true;
            holdMatrix = matrix;
            holdCursor = cursor;

            if (cursor != null)
                cursor.matrix = null;

            prevKeyState = Keyboard.GetState();
            prevPadState = GamePad.GetState(controllerID);
        }
    }
}
