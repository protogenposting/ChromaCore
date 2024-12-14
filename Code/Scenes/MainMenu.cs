using ChromaCore.Code.Objects.Players.Characters;
using ChromaCore.Code.Utils.Network;

namespace ChromaCore.Code.Scenes
{
    public class MainMenu : Scene
    {
        public override void Load()
        {
            Game.Instance.music.FadeOut(120);

            UI = new UICanvas();

            List<UIElement> buttons = new List<UIElement>()
            {
                new TextBlock("Versus", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m) => Game.Instance.ChangeScene(new CharacterSelect([0, -1]) { gamemode = Gamemodes.Versus }),
                    size = new Vector2(240, 80),
                    drawScale = 1.5f
                },
                new TextBlock("Training", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m) => Game.Instance.ChangeScene(new CharacterSelect([0, -1]) { gamemode = Gamemodes.Training }),
                    size = new Vector2(240, 80),
                    drawScale = 1.5f
                },
                new TextBlock("Net Host", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m) =>
                    {
                        Game.Instance.ChangeScene(new ConnectionScreen() {player1Controller = c == null ? -1 : c.input.id});
                    },
                    size = new Vector2(240, 80),
                    drawScale = 1.5f
                },
                new TextBox("", "Net Connect: ", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onEnter = t =>
                    {
                        Game.Instance.ChangeScene(new ConnectionScreen(t.entryText, 9000){player1Controller = t.mouseTriggered ? -1 : cursors[0].input.id});
                    },
                    size = new Vector2(240, 80),
                    drawScale = 1.5f
                },
                new TextBlock("Quit", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton")
                {
                    onClick = (c, m) => Game.Instance.Exit(),
                    size = new Vector2(240, 80),
                    drawScale = 1.5f
                },
            };

            ButtonMatrix mainUI = ButtonMatrix.Create1DMatrix(true, 960, 400, 140, buttons, null, true);
            UI.buttonMatricies.Add(mainUI);

            cursors.Add(new MenuCursor(0, mainUI));

            Game.Instance.music.FadeOut(120);
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
                    //c.input.UpdateBuffers();
                }
        }
    }
}
