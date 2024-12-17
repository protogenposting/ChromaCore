using ChromaCore.Code.Effects;
using ChromaCore.Code.Objects;
using ChromaCore.Code.Objects.Players.Characters;
using ChromaCore.Code.Stages;
using ChromaCore.Code.Utils.Network;
using Microsoft.Xna.Framework.Graphics;

namespace ChromaCore.Code.Scenes
{
    public class InGame : Scene
    {
        public List<Fighter> players = new List<Fighter>();
        public List<Hitbox> hitboxes = new List<Hitbox>();
        public List<GameObject> miscObjects = new List<GameObject>();
        public List<GameObject> solidObjects = new List<GameObject>();
        public Tile[,] tiles;
        public List<Particle> particles = new List<Particle>();

        public Type roomType;
        public Room room;

        public Color ambientLight = Color.LightGray;
        public List<Light> lights = new List<Light>();
        RenderTarget2D lightMap;
        RenderTarget2D bloomMap;
        RenderTarget2D sceneRender;
        const float lightScale = 8;

        public int prepareHitpause;
        public int hitpause;
        public Fighter playerInCorner;
        public bool supressPlayerLayering = false;

        public List<int> controllerPorts = new();
        public List<int> controllerProfiles = new();
        public List<int> fighterPalettes;

        public int winner = 0;
        public int winScreenTimer;
        public virtual bool InfiniteTime => true;
        public int versusTimer;
        public virtual bool PlayIntro => true;
        public int introTimer;

        public List<(SoundEffectInstance, Vector2, float)> soundEffects = new();

        protected ButtonMatrix pauseMenuUI;
        public bool paused = false;

        public HealthBar healthBar;
        public virtual bool SpawnHealthBars => true;

        protected NetcodeManager netcodeManager;

        public InGame(Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null)
        {
            //Setup UI
            UI = new UICanvas();
            SetupPauseMenu();

            lightMap = new RenderTarget2D(Game.Instance.GraphicsDevice, (int)(1920 / lightScale), (int)(1080 / lightScale));
            bloomMap = new RenderTarget2D(Game.Instance.GraphicsDevice, 1920, 1080);
            sceneRender = new RenderTarget2D(Game.Instance.GraphicsDevice, 1920, 1080);
            roomType = entryRoom;
            camera = new Camera();

            fighterPalettes = new List<int>(palettes);
            this.controllerPorts = new List<int>(controllerPorts);
            this.controllerProfiles = new List<int>(controllerProfiles);


            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    this.players.Add(new Jet());
                }
                else this.players.Add((Fighter)Activator.CreateInstance(players[i]));
            }
        }
        public InGame(NetConnection netConnection, Type[] players, int[] controllerPorts, int[] controllerProfiles, int[] palettes, Type entryRoom = null) : this(players, controllerPorts, controllerProfiles, palettes, entryRoom)
        {
            netcodeManager = new NetcodeManager(this, netConnection);
        }

        public override void Load()
        {
            if (roomType == null) room = new TestRoom();
            else room = (Room)Activator.CreateInstance(roomType);
            TileMap.GenerateMap(room.map);
            for (int i = 0; i < players.Count; i++)
            {
                players[i].Spawn(i, controllerPorts.Count > i ? controllerPorts[i] : -1, DataManager.controllerProfiles[controllerProfiles[i]], fighterPalettes[i]);

                if (PlayIntro && introTimer == 0 && players[i].introAnimation != null) players[i].animation = players[i].introAnimation;
                if (controllerPorts[i] == -2) players[i].input = new TrainingDummyController(players[i]);

                if (players.Count == 2)
                {
                    players[0].drawLayer = 0.51f;
                    players[1].drawLayer = 0.5f;
                }
            }

            if (SpawnHealthBars)
            {
                healthBar = new HealthBar();
                UI.AddElement(healthBar);
            }
            if (PlayIntro && introTimer == 0) introTimer = 150;

            if (!InfiniteTime) versusTimer = 120 * 60;

            Game.Instance.music.FadeTo(new CompoundMusicTrack("Music/track1intro", 9.193, "Music/track1loop", 24.774), 60);

            camera.Update(true);

            if (netcodeManager != null)
            {
                netcodeManager.players = this.players.ToArray();
                netcodeManager.rollbackState = new GameState(this);

                for (int i = 0; i < players.Count; i++)
                {
                    if (i != netcodeManager.myPlayerID) players[i].input.id = -2;
                }
            }
        }

        public override void Update(GameTime gametime)
        {
            lights.Clear();
            Game game = Game.Instance;
            if (!paused)
            {
                if (netcodeManager == null)
                {
                    UpdateControllers();
                    AdvanceFrame();
                    foreach (Fighter f in players) if (f.input.KeyPressed(Controller.Key_Start)) Pause(f.input.id);
                }
                else
                {
                    netcodeManager.Update();
                }
            }
            else if (cursors.Count > 0 && cursors[0].input.KeyPressed(Controller.Key_Start)) Unpause(cursors[0], pauseMenuUI);
            foreach (GameObject o in miscObjects) if ((camera.target - o.position).Length() < 1920) o.AddLights();
            foreach (Particle p in particles) if ((camera.target - p.position).Length() < 1920) p.AddLights();

            UI.Update();
            UpdateCursors();

            foreach (var se in soundEffects.ToArray())
            {
                if (se.Item1.State == SoundState.Stopped)
                {
                    soundEffects.Remove(se);
                    continue;
                }
                float distanceMultiplier = MathHelper.Clamp(1.5f - (camera.target - se.Item2).Length() / 1000, 0, 1);
                float vol = Game.SoundEffectVolume * se.Item3 * distanceMultiplier;

                float pan = 0;// MathHelper.Clamp((se.Item2.X - camera.target.X) / 1000, -1, 1);
                if (vol > 1) vol = 1;

                se.Item1.Volume = Math.Clamp(vol * (16f / soundEffects.Count + 0.25f), 0, se.Item3);
                se.Item1.Pan = pan;
            }
        }

        public virtual void AdvanceFrame()
        {
            if (PlayIntro && introTimer > -30) introTimer--;
            if (!InfiniteTime && versusTimer > 0) versusTimer--;
            if (winScreenTimer > 0) winScreenTimer++;
            UpdateObjects();
            if (hitpause > 0) hitpause--;

            if (prepareHitpause > 0)
            {
                hitpause = Math.Max(hitpause, prepareHitpause);
                prepareHitpause = 0;
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //Generate light map
            Texture2D lightSampler = new Texture2D(Game.Instance.GraphicsDevice, 4, Math.Max(lights.Count, 1), false, SurfaceFormat.Vector4);
            ApplyLights(lightSampler);
            Effect effect = Game.LoadAsset<Effect>("Shaders/LightMap");
            effect.Parameters["LightCount"].SetValue(lights.Count);
            effect.Parameters["ambientLight"].SetValue(ambientLight.ToVector4());
            effect.Parameters["screenSize"].SetValue(Vector2.One / camera.zoom * lightScale);
            effect.Parameters["camPos"].SetValue(camera.position);

            Game.Instance.GraphicsDevice.SetRenderTarget(lightMap);
            Game.Instance.GraphicsDevice.Clear(Color.Black);
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, effect);
            spritebatch.Draw(lightSampler, new Rectangle(0, 0, 1920, 1080), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
            spritebatch.End();

            //Generate Bloom
            Game.Instance.GraphicsDevice.SetRenderTarget(bloomMap);
            Game.Instance.GraphicsDevice.Clear(Color.Black);
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, null, null, null, camera.viewMatrix);
            foreach (GameObject o in miscObjects.ToArray()) if ((o.position - camera.target).Length() <= 1920) o.DrawBloom(spritebatch);
            spritebatch.End();

            //Draw objects to scene
            Game.Instance.GraphicsDevice.SetRenderTarget(sceneRender);
            Game.Instance.GraphicsDevice.Clear(Color.SkyBlue);
            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, camera.viewMatrix);
            if (room != null) room.Draw(spritebatch);
            DrawObjects(spritebatch);
            spritebatch.End();

            //Draw scene with light map
            Game.Instance.GraphicsDevice.SetRenderTarget(Game.Instance.mainRender);
            effect = Game.LoadAsset<Effect>("Shaders/LightShader");
            effect.Parameters["LightMap"].SetValue(lightMap);
            effect.Parameters["BloomMap"].SetValue(bloomMap);
            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, effect);
            spritebatch.Draw(sceneRender, Vector2.Zero, Color.White);
            spritebatch.End();

            Texture2D gray = new Texture2D(Game.Instance.GraphicsDevice, 1, 1);
            gray.SetData(new Color[] { new Color(Color.DarkGray, 0.6f) });

            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp);
            if (paused) spritebatch.Draw(gray, new Rectangle(0, 0, 1920, 1080), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.1f);
            UI.Draw(spritebatch);
            //Countdown
            if (PlayIntro && introTimer > -30)
            {
                string text = introTimer > 15 ? ((introTimer - 15) / 45 + 1).ToString() : "Fight!";
                SpriteFont font = Fonts.exampleFont;
                spritebatch.DrawString(font, text, new Vector2(960, 480), Color.White, 0, font.MeasureString(text) / 2, 8, SpriteEffects.None, 0.05f);
            }
            foreach (MenuCursor c in cursors) c.Draw(spritebatch);
            spritebatch.End();

            lightSampler.Dispose();
        }

        void ApplyLights(Texture2D lightSampler)
        {
            if (lights.Count == 0)
            {
                lightSampler.SetData(new Vector4[]
                {
                    Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero
                });
            }
            else
            {
                Vector4[] c = new Vector4[lights.Count * 4];
                for (int i = 0; i < lights.Count; i++)
                {
                    c[i * 4] = new Vector4(lights[i].position, lights[i].radius, lights[i].type);
                    c[i * 4 + 1] = lights[i].color.ToVector4();
                    c[i * 4 + 2] = new Vector4(lights[i].decay, 0);
                    c[i * 4 + 3] = lights[i].extra;
                }
                lightSampler.SetData(c);
            }
        }

        public void AddPointLight(Vector2 position, float radius, Color color, float redDecay = 1, float greenDecay = 1, float blueDecay = 1)
        {
            if (lights.Count < 10000) lights.Add(new Light(0, position, radius, color, new Vector3(redDecay, greenDecay, blueDecay), Vector4.Zero));
        }
        public void AddLineLight(Vector2 start, Vector2 end, float width, Color color, float redDecay = 1, float greenDecay = 1, float blueDecay = 1)
        {
            if (lights.Count < 10000) lights.Add(new Light(1, start, width, color, new Vector3(redDecay, greenDecay, blueDecay), new Vector4(end, 0, 0)));
        }
        public void AddConeLight(Vector2 start, float angle, float spread, Color color, float redDecay = 1, float greenDecay = 1, float blueDecay = 1)
        {
            if (lights.Count < 10000) lights.Add(new Light(2, start, MathHelper.ToRadians(spread % 360), color, new Vector3(redDecay, greenDecay, blueDecay), new Vector4(MathHelper.ToRadians(angle % 360), 0, 0, 0)));
        }

        //Gets all objects in loadedObjects that are of type T and its derivitives
        public virtual List<T> GetObjectsOfType<T>()
        {
            List<T> list = new List<T>();

            foreach (GameObject o in miscObjects) if (o is T t) list.Add(t);

            return list;
        }

        protected virtual void UpdateControllers()
        {
            if (Game.Instance.screenTransition == null && introTimer <= 0)
                foreach (Fighter p in players) p.input.UpdateKeys(p.GetMotionInputDirection);
        }

        protected virtual void UpdateObjects()
        {
            if (UI.elements.Exists(e => e is PopupMessage)) return;

            camera.Update();

            foreach (Fighter p in players.ToArray()) if (hitpause <= 0) p.Update();
            foreach (GameObject o in miscObjects.ToArray()) if (hitpause <= 0) o.Update();
            foreach (Hitbox h in hitboxes.ToArray()) if (hitpause <= 0) h.Update();
            foreach (Particle p in particles.ToArray()) if (hitpause <= 0 || p.updateDuringHitpause) p.Update();

            //Corner handling
            if (players.Count == 2 && Math.Abs(players[0].position.X - players[1].position.X) <= 32)
            {
                bool[] inCorner = new bool[] { false, false };
                foreach (Fighter p in players)
                {
                    if ((p.TouchingTile(p.collider.Right, 4, 0) || p.TouchingTile(p.collider.Left, -4, 0)) && players.IndexOf(p) <= 1) inCorner[players.IndexOf(p)] = true;
                }
                if (!inCorner[0] && !inCorner[1]) playerInCorner = null;
                if (inCorner[0] && !inCorner[1]) playerInCorner = players[0];
                if (!inCorner[0] && inCorner[1]) playerInCorner = players[1];
                if (inCorner[0] && inCorner[1])
                {
                    foreach (Fighter p in players)
                    {
                        if (playerInCorner == p)
                        {
                            if (p.TouchingTile(p.collider.Right, 4, 0) && p.Grounded && !p.CommitedState) p.direction = -1;
                            if (p.TouchingTile(p.collider.Left, -4, 0) && p.Grounded && !p.CommitedState) p.direction = 1;
                        }
                        if (playerInCorner != p)
                        {
                            if (p.dontPushPlayer && !playerInCorner.dontPushPlayer) playerInCorner = p;
                            if (p.TouchingTile(p.collider.Right, 4, 0))
                            {
                                p.position.X -= 1;
                                //p.direction = 1;
                            }
                            if (p.TouchingTile(p.collider.Left, -4, 0))
                            {
                                p.position.X += 1;
                                //p.direction = -1;
                            }
                        }
                    }
                }
            }
            //Keep players in the camera
            if ((camera.position - camera.target).Length() <= 400) foreach (Fighter p in players)
            {
                p.position.X = Math.Clamp(p.position.X, camera.position.X + 48, camera.position.X + camera.CamWidth - 48);
            }
            //Handle player sprite layers
            if (!supressPlayerLayering)
            {
                if (players.Count > 1 && players[0].state == Fighter.States.Attack && players[1].state != Fighter.States.Attack)
                {
                    players[0].drawLayer = 0.51f;
                    players[1].drawLayer = 0.5f;
                }
                if (players.Count > 1 && players[1].state == Fighter.States.Attack && players[0].state != Fighter.States.Attack)
                {
                    players[0].drawLayer = 0.5f;
                    players[1].drawLayer = 0.51f;
                }
            }
            supressPlayerLayering = false;
        }

        protected virtual void DrawObjects(SpriteBatch spriteBatch)
        {
            foreach (Fighter p in players) p.Draw(spriteBatch);
            foreach (Hitbox h in hitboxes) h.Draw(spriteBatch);
            foreach (GameObject o in miscObjects.ToArray()) if ((o.position - camera.target).Length() <= 1920) o.Draw(spriteBatch);
            for (int x = (int)(camera.target.X / TileMap.Tile_Size) - camera.CamWidth / TileMap.Tile_Size - 2; x < (int)(camera.target.X / TileMap.Tile_Size) + camera.CamWidth / TileMap.Tile_Size + 2; x++)
            {
                for (int y = (int)(camera.target.Y / TileMap.Tile_Size) - camera.CamHeight / TileMap.Tile_Size - 2; y < (int)(camera.target.Y / TileMap.Tile_Size) + camera.CamHeight / TileMap.Tile_Size + 2; y++)
                {
                    if (x >= 0 && x < tiles.GetLength(1) && y >= 0 && y < tiles.GetLength(0) && tiles[y, x] != null && !miscObjects.Contains(tiles[y, x])) tiles[y, x].Draw(spriteBatch);
                }
            }
            foreach (Particle p in particles.ToArray()) p.Draw(spriteBatch);
        }

        public virtual void ApplyHitpause(int frames)
        {
            prepareHitpause = Math.Max(prepareHitpause, frames);
        }

        public virtual void PlayWorldSound(SoundEffect sound, Vector2 position, float volumeMultiplier = 1)
        {
            if (netcodeManager != null && netcodeManager.rollingBack)
            {
                return;
            }
            float distanceMultiplier = MathHelper.Clamp(1.5f - (camera.target - position).Length() / 1000, 0, 1);
            float vol = Game.SoundEffectVolume * volumeMultiplier * distanceMultiplier;

            float pan = 0;// MathHelper.Clamp((position.X - camera.target.X) / 1000, -1, 1);

            if (vol > 1) vol = 1;

            SoundEffectInstance se = sound.CreateInstance();
            soundEffects.Add((se, position, volumeMultiplier));
            se.Volume = Math.Clamp(vol * (16f / soundEffects.Count + 0.25f), 0, volumeMultiplier);
            se.Pan = pan;
            se.Play();
        }

        public void SetupPauseMenu()
        {
            List<UIElement> list = GetPauseMenuButtons();

            pauseMenuUI = new ButtonMatrix(new UIElement[1, list.Count], Unpause, true) { cursorOffset = new Vector2(Game.LoadAsset<Texture2D>("ExampleContent/UIButton").Width * -2, 0), multiCursorOffset = new Vector2(0, 32) };
            for (int i = 0; i < list.Count; i++)
            {
                list[i].position = new Vector2(960, 480 + 480 / (list.Count - 1) * i);
                list[i].layer = 0.951f;
                pauseMenuUI.buttons[0, i] = list[i];
            }
        }

        protected virtual List<UIElement> GetPauseMenuButtons()
        {
            return new List<UIElement>()
            {
                new TextBlock("Resume", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = (c, m, b) => Unpause(c, m),
                    layer = 0.7f
                },
                new TextBlock("Character Select", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = ReturnToCSS,
                    layer = 0.7f
                },
                new TextBlock("Main Menu", "ExampleContent/UIFont", Color.White, "UI/Menu/MenuButton")
                {
                    onClick = (c, m, b) => Game.Instance.ChangeScene(new MainMenu()),
                    layer = 0.7f
                }
            };
        }

        public virtual void Pause(int controllerID)
        {
            if (!paused)
            {
                paused = true;
                UI.buttonMatricies.Add(pauseMenuUI);
                cursors.Add(new MenuCursor(controllerID, pauseMenuUI));
                cursors[0].input.UpdateKeys(1);
                cursors[0].input.ClearAllBuffers();
                foreach (var se in soundEffects) se.Item1.Pause();
            }
        }

        public virtual void Unpause(MenuCursor c, ButtonMatrix m)
        {
            if (paused)
            {
                paused = false;
                foreach (UIElement e in UI.elements) e.hoverScale = 1;
                foreach (ButtonMatrix b in UI.buttonMatricies) foreach (UIElement e in b.buttons) e.hoverScale = 1;
                UI.buttonMatricies.Clear();
                cursors.Clear();

                //Stops the menu input from carrying over into the game
                foreach (Fighter p in players)
                {
                    p.input.UpdateKeys(p.direction);
                    p.input.ClearAllBuffers();
                }
                foreach (var se in soundEffects) se.Item1.Resume();
            }
        }

        protected virtual void ReturnToCSS(MenuCursor c, ButtonMatrix m, UIElement button)
        {
            Game.Instance.ChangeScene(new CharacterSelect(new int[] { players[0].input.id, players[1].input.id }) { gamemode = Gamemodes.Versus });
        }

        public void StealCorner(Fighter player) => playerInCorner = player;
    }

    public enum Gamemodes
    {
        Versus,
        Training,
        Netplay
    }
}