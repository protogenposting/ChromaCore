using ChromaCore.Code.Objects.Players.Characters;
using ChromaCore.Code.Utils.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ChromaCore.Code.Scenes
{
    internal class CharacterSelect : Scene
    {
        Type[] selectedCharacters = new Type[2];
        int[] palettes = new int[2];
        ControlProfile[] playerProfiles = new ControlProfile[] { new ControlProfile(), new ControlProfile() };
        ButtonMatrix characterIcons;
        ButtonMatrix readyButton;

        List<Type> characterList = new List<Type>();

        NetConnection netConnection;
        bool netReady = false;
        bool otherReady = false;

        TextBlock[] profileText;

        public Gamemodes gamemode = Gamemodes.Versus;

        public CharacterSelect(int[] controllers)
        {
            UI = new UICanvas();

            CSSIcon[,] charIcons = new CSSIcon[,]
            {
                { new CSSIcon(typeof(Jet), "Characters/Jet/CSSIcon") { onClick = SelectCharacter, paletteCount = 6 } },
            };
            for (int x = 0; x < charIcons.GetLength(0); x++)
            {
                for (int y = 0; y < charIcons.GetLength(1); y++)
                {
                    charIcons[x, y].position = new Vector2(960 - (64 * (charIcons.GetLength(0) - 1)) + (128 * x), 400 - (64 * charIcons.GetLength(1) / 2) + (128 * y));
                    characterList.Add(charIcons[x, y].character);
                }
            }

            characterIcons = new ButtonMatrix(charIcons, ReturnToMainMenu, true) { cursorOffset = new Vector2(-64, 0), multiCursorOffset = new Vector2(0, 32) };

            readyButton = new ButtonMatrix(new UIElement[1, 1], DeselectCharacter, false) { cursorOffset = new Vector2(-64, 0), multiCursorOffset = new Vector2(0, 32) };
            readyButton.buttons[0, 0] = new TextBlock("Ready", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", 960, 800) { onClick = (c, m) =>
            {
                if (netConnection != null)
                {
                    cursors.Remove(c);
                    UI.buttonMatricies.Remove(m);

                    netConnection.SendMessage("Ready", new JsonObject()
                    {
                        { "Character", characterList.IndexOf(selectedCharacters[0]) },
                        { "Palette", palettes[0] }
                    });
                }
                else if (cursors.All(cursor => cursor == null || cursor.matrix == readyButton))
                {
                    foreach (MenuCursor cursor in cursors) cursor?.SwitchMatrix(null);
                    Game.Instance.screenTransition = new FadeToBlackTransition()
                    {
                        duration = 40,
                        transitionTime = 20,
                        onTransition = StartGame
                    };
                }
            } };

            foreach (int i in controllers)
            {
                if (i == -2) cursors.Add(null);
                else cursors.Add(new MenuCursor(i, characterIcons)
                {
                    UpInput = (c, m) =>
                    {
                        Vector2 pos = c.position;
                        MenuCursor.DefaultUpPressed(c, m);
                        if (c.position != pos) palettes[cursors.IndexOf(c)] = 0;
                    },
                    DownInput = (c, m) =>
                    {
                        Vector2 pos = c.position;
                        MenuCursor.DefaultDownPressed(c, m);
                        if (c.position != pos) palettes[cursors.IndexOf(c)] = 0;
                    },
                    LeftInput = (c, m) =>
                    {
                        Vector2 pos = c.position;
                        MenuCursor.DefaultLeftPressed(c, m);
                        if (c.position != pos) palettes[cursors.IndexOf(c)] = 0;
                    },
                    RightInput = (c, m) =>
                    {
                        Vector2 pos = c.position;
                        MenuCursor.DefaultRightPressed(c, m);
                        if (c.position != pos) palettes[cursors.IndexOf(c)] = 0;
                    }
                });
            }

            UI.buttonMatricies.Add(characterIcons);
            UI.buttonMatricies.Add(readyButton);

            profileText = new TextBlock[]
            {
                new TextBlock("Profile: " + playerProfiles[0].name, "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", 320, 80) {drawScale = 1.5f},
                new TextBlock("Profile: " + playerProfiles[0].name, "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", 1600, 80) {drawScale = 1.5f}
            };

            UI.elements.AddRange(profileText);
            if (controllers[0] >= 0) profileText[0].text = "Select | " + profileText[0].text;
            else if (controllers[0] == -1) profileText[0].text = "Tab | " + profileText[0].text;
            else UI.elements.Remove(profileText[0]);
            if (controllers[1] >= 0) profileText[1].text = profileText[1].text + " | Select";
            else if (controllers[1] == -1) profileText[1].text = profileText[1].text + " | Tab";
            else UI.elements.Remove(profileText[1]);

            int[] loadedProfiles = DataManager.LoadLastUsedProfiles();
            if (loadedProfiles != null)
            {
                playerProfiles[0] = DataManager.controllerProfiles[loadedProfiles[0]];
                profileText[0].text = "Profile: " + playerProfiles[0].name;
                if (controllers[0] == -1) profileText[0].text = "Tab | " + profileText[0].text;

                playerProfiles[1] = DataManager.controllerProfiles[loadedProfiles[1]];
                profileText[1].text = "Profile: " + playerProfiles[1].name;
                if (controllers[1] == -1) profileText[1].text = profileText[1].text + " | Tab";
            }

            Game.Instance.music.FadeOut(120);
        }

        public CharacterSelect(NetConnection connection, int controller) : this([controller, -2])
        {
            this.netConnection = connection;
            gamemode = Gamemodes.Netplay;
        }

        public override void Load()
        {
            
        }

        public override void Update(GameTime gametime)
        {
            UI.Update();
            foreach (MenuCursor c in cursors.ToArray()) if (c != null)
                {
                    c.Update();
                    if (c.matrix == characterIcons && c.input.KeyPressed(Controller.Key_Select))
                    {
                        c.input.ClearAllBuffers();
                        OpenProfiles(c);
                    }
                    if (c.matrix == characterIcons && c.input.KeyPressed(Controller.Key_MenuLB))
                    {
                        c.input.ClearBuffer(Controller.Key_MenuLB);
                        palettes[cursors.IndexOf(c)]--;
                        if (palettes[cursors.IndexOf(c)] < 0) palettes[cursors.IndexOf(c)] = ((CSSIcon)c.matrix.buttons[(int)c.position.X, (int)c.position.Y]).paletteCount - 1;
                    }
                    if (c.matrix == characterIcons && c.input.KeyPressed(Controller.Key_MenuRB))
                    {
                        c.input.ClearBuffer(Controller.Key_MenuRB);
                        palettes[cursors.IndexOf(c)]++;
                        if (palettes[cursors.IndexOf(c)] > ((CSSIcon)c.matrix.buttons[(int)c.position.X, (int)c.position.Y]).paletteCount - 1) palettes[cursors.IndexOf(c)] = 0;
                    }
                }

            if (netConnection != null)
            {
                var messages = netConnection.ReceiveMessages();
                foreach (var message in messages)
                {
                    string type = message.RootElement.GetProperty("type").GetString();
                    if (type == "Ready")
                    {
                        otherReady = true;
                        selectedCharacters[1] = characterList[message.RootElement.GetProperty("Character").GetInt32()];
                        palettes[1] = message.RootElement.GetProperty("Palette").GetInt32();
                        netConnection.SendMessage("ConfirmReady", new JsonObject());
                    }
                    if (type == "ConfirmReady")
                    {
                        netReady = true;
                    }
                }

                if (netReady && otherReady && Game.Instance.screenTransition == null)
                {
                    Game.Instance.screenTransition = new FadeToBlackTransition()
                    {
                        duration = 40,
                        transitionTime = 20,
                        onTransition = StartGame
                    };
                }
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            UI.Draw(spritebatch);
            foreach (MenuCursor c in cursors) c?.Draw(spritebatch);
            spritebatch.End();
        }

        void StartGame()
        {
            DataManager.SetLastUsedProfiles((byte)DataManager.controllerProfiles.IndexOf(playerProfiles[0]), (byte)DataManager.controllerProfiles.IndexOf(playerProfiles[1]));

            switch (gamemode)
            {
                case Gamemodes.Training:
                    Game.Instance.ChangeScene(new InGameTraining(selectedCharacters, new int[] { cursors[0] != null ? cursors[0].input.id : -2, cursors[1] != null ? cursors[1].input.id : -2 }, new int[] { DataManager.controllerProfiles.IndexOf(playerProfiles[0]), DataManager.controllerProfiles.IndexOf(playerProfiles[1]) }, palettes));
                    break;

                case Gamemodes.Netplay:
                    if (netConnection.playerID == 0)
                    {
                        Game.Instance.ChangeScene(new InGameNetplay(netConnection, selectedCharacters, [netConnection.playerController, -3], [DataManager.controllerProfiles.IndexOf(playerProfiles[0]), 0], palettes));
                    }
                    else
                    {
                        Game.Instance.ChangeScene(new InGameNetplay(netConnection, [selectedCharacters[1], selectedCharacters[0]], [-3, netConnection.playerController], [0, DataManager.controllerProfiles.IndexOf(playerProfiles[0])], [palettes[1], palettes[0]]));
                    }
                    
                    break;

                default:
                    Game.Instance.ChangeScene(new InGameVersus(selectedCharacters, new int[] { cursors[0] != null ? cursors[0].input.id : -2, cursors[1] != null ? cursors[1].input.id : -2 }, new int[] { DataManager.controllerProfiles.IndexOf(playerProfiles[0]), DataManager.controllerProfiles.IndexOf(playerProfiles[1]) }, palettes));
                    break;
            }
        }

        void ReturnToMainMenu(MenuCursor cursor, ButtonMatrix matrix)
        {
            Game.Instance.ChangeScene(new MainMenu());
            if (netConnection != null)
            {
                netConnection.SendMessage("disconnect", new JsonObject());
            }
        }

        void SelectCharacter(MenuCursor cursor, ButtonMatrix matrix)
        {
            selectedCharacters[cursors.IndexOf(cursor)] = ((CSSIcon)characterIcons.buttons[(int)cursor.position.X, (int)cursor.position.Y]).character;
            cursor.SwitchMatrix(readyButton);
        }

        void DeselectCharacter(MenuCursor cursor, ButtonMatrix matrix)
        {
            int x = 0;
            int y = 0;
            for (x = 0; x < characterIcons.buttons.GetLength(0); x++)
            {
                bool found = false;
                for (y = 0; y < characterIcons.buttons.GetLength(1); y++)
                {
                    if (characterIcons.buttons[x, y] is CSSIcon c && c.character == selectedCharacters[cursors.IndexOf(cursor)])
                    {
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
            cursor.SwitchMatrix(characterIcons, x, y);
            selectedCharacters[cursors.IndexOf(cursor)] = null;
        }

        void OpenProfiles(MenuCursor c)
        {
            TextBlock[,] profileList = new TextBlock[1, DataManager.controllerProfiles.Count];
            for (int i = 0; i < DataManager.controllerProfiles.Count; i++)
            {
                profileList[0, i] = new TextBlock(DataManager.controllerProfiles[i].name, "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton", cursors.IndexOf(c) == 0 ? 320 : 1600, 200 + 100 * i)
                {
                    onClick = SelectProfile
                };
            }
            ButtonMatrix profileSelect = new ButtonMatrix(profileList, CancelProfile, true) { cursorOffset = new Vector2(Game.LoadAsset<Texture2D>("UI/Menu/MenuButton").Width * -2, 0), multiCursorOffset = new Vector2(0, 32) };
            UI.buttonMatricies.Add(profileSelect);
            c.SwitchMatrix(profileSelect);
        }

        void SelectProfile(MenuCursor cursor, ButtonMatrix matrix)
        {
            playerProfiles[cursors.IndexOf(cursor)] = DataManager.controllerProfiles[(int)cursor.position.Y];
            profileText[cursors.IndexOf(cursor)].text = "Profile: " + playerProfiles[cursors.IndexOf(cursor)].name;
            if (cursor.input.id == -1 && cursors.IndexOf(cursor) == 0) profileText[0].text = "Tab | " + profileText[0].text;
            if (cursor.input.id == -1 && cursors.IndexOf(cursor) == 1) profileText[1].text = profileText[1].text + " | Tab";
            CancelProfile(cursor, matrix);
        }

        void CancelProfile(MenuCursor cursor, ButtonMatrix matrix)
        {
            cursor.SwitchMatrix(characterIcons);
            UI.buttonMatricies.Remove(matrix);
            matrix.Dispose();
        }
    }

    class CSSIcon : UIElement
    {
        public Type character = typeof(Jet);
        public int paletteCount = 1;

        public CSSIcon(Type character, string texture) : base(texture, 0, 0)
        {
            this.character = character;
        }
    }
}
