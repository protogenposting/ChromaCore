global using ChromaCore.Code.UI;
global using ChromaCore.Code.Utils;
global using ChromaCore.Code.Utils.Collision;
global using ChromaCore.Code.Utils.Combat;
global using ChromaCore.Code.Utils.Input;
global using ChromaCore.Code.Utils.Map;
global using ChromaCore.Code.Utils.ScreenTransitions;
global using ChromaCore.Code.Utils.Visual;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Audio;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
using ChromaCore.Code.Scenes;

namespace ChromaCore
{
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        public GraphicsDeviceManager Graphics => graphics;

        private SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch => spriteBatch;
        public RenderTarget2D mainRender;

        List<Texture2D> tempTextures = new List<Texture2D>();

        private static Game instance;
        public static Game Instance => instance;

        private Scene scene;
        public Scene Scene => scene;

        private Scene changeScene;
        private Action<Scene> postLoadScene;

        public static float SoundEffectVolume => DataManager.soundEffectVolume * DataManager.masterVolume;
        public static float MusicVolume => DataManager.musicVolume * DataManager.masterVolume * musicVolumeMultiplier;
        public static float musicVolumeMultiplier = 1f;
        public MusicPlayer music = new MusicPlayer();

        public Random random = new Random();
        public float randomLayerOffset => (float)random.NextDouble() * 0.00001f;

        public Matrix screenSizeMatrix => Matrix.CreateScale(Window.ClientBounds.Width / 1920.0f, Window.ClientBounds.Height / 1080.0f, 1);

        public const string VersionNumber = "0.0.1";

        public bool DebugMode => false;

        public ScreenTransition screenTransition;

        public Vector2 mouseScreenPosition;
        public Vector2 mouseWorldPosition;
        public bool mouseLeftClick = false;

        public Game()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
            InactiveSleepTime = TimeSpan.Zero;
        }

        protected override void Initialize()
        {
            mainRender = new RenderTarget2D(GraphicsDevice, 1920, 1080);
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Window.IsBorderless = true;
            graphics.ApplyChanges();
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            DataManager.LoadControlProfiles();
            scene = new MainMenu();
            scene.Load();

            base.Initialize();
            DataManager.SetupFileStructure();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            Vector2 mp = Vector2.Transform(Mouse.GetState(Window).Position.ToVector2(), Matrix.Invert(screenSizeMatrix));
            mouseLeftClick = Mouse.GetState().LeftButton == ButtonState.Pressed;
            mouseScreenPosition = mp;
            if (scene is InGame ig) mouseWorldPosition = mouseScreenPosition / ig.camera.zoom + ig.camera.position;

            scene?.Update(gameTime);
            screenTransition?.Update();
            music.Update(gameTime);

            if (changeScene != null)
            {
                scene = changeScene;
                changeScene = null;
                scene.Load();
                if (postLoadScene != null)
                {
                    postLoadScene(scene);
                    postLoadScene = null;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(mainRender);
            GraphicsDevice.Clear(new Color(25, 25, 25));

            scene.Draw(spriteBatch);
            screenTransition?.Draw(spriteBatch);

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, screenSizeMatrix);
            spriteBatch.Draw(mainRender, Vector2.Zero, Color.White);
            if (DebugMode)
                spriteBatch.DrawString(LoadAsset<SpriteFont>("ExampleContent/UIFont"), "FPS: " + Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(40, 20), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.9f);
            spriteBatch.End();

            foreach (Texture2D t in tempTextures.ToArray())
            {
                t.Dispose();
                tempTextures.Remove(t);
            }
        }

        public static T LoadAsset<T>(string url)
        {
            return Instance.Content.Load<T>(url);
        }

        public static Texture2D LoadTexture(string url)
        {
            return Instance.Content.Load<Texture2D>(url);
        }

        public static SoundEffect LoadSound(string url)
        {
            return Instance.Content.Load<SoundEffect>(url);
        }

        public static Effect LoadShader(string url)
        {
            return Instance.Content.Load<Effect>(url);
        }

        public Texture2D GetTemporaryTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            tempTextures.Add(texture);
            return texture;
        }
        public RenderTarget2D GetTemporaryRenderTarget(int width, int height)
        {
            RenderTarget2D texture = new RenderTarget2D(GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            tempTextures.Add(texture);
            return texture;
        }

        public void ChangeScene(Scene scene) => changeScene = scene;
        public void ChangeScene(Scene scene, Action<Scene> postLoadScene)
        {
            changeScene = scene;
            this.postLoadScene = postLoadScene;
        }
    }
}
