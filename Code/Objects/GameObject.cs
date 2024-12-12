using ChromaCore.Code.Effects;
using ChromaCore.Code.Scenes;
using ChromaCore.Code.Utils.Map;
using ChromaCore.Code.Utils.Visual;
using System;

namespace ChromaCore.Code.Objects
{
    /// <summary>
    /// <para>Basic class for interacting with the InGame environment</para>
    /// <para>Should be derived to perform more specific roles</para>
    /// </summary>
    public class GameObject : IDisposable
    {
        protected Game game = Game.Instance;
        protected InGame scene => Game.Instance.Scene is InGame g ? g : null;

        public Animation animation;
        public Texture2D sprite => animation != null ? animation.spriteSheet : null;
        public int direction = 1;
        public float rotation = 0;
        public float drawLayer = 0.5f + Game.Instance.randomLayerOffset;
        public float drawScale = 1;
        public Color drawColor = Color.White;

        public Vector2 position = Vector2.Zero;
        public Vector2 velocity = Vector2.Zero;
        public float gravity = 0;
        public float fallSpeed = 0;

        public Collider collider;
        public bool ignoreTiles = false;

        public bool pushEntities = false;
        public float pushWeight = 1;

        public bool updateOffscreen = false;
        public bool destroyed = false;

        public GameObject() { }

        public virtual void Update()
        {
            if (animation != null && animation.frames > 1)
            {
                animation.Update();

                if (animation.timer == 0)
                {
                    if (animation.sounds != null) foreach (AnimationSound s in animation.sounds)
                        {
                            if (s.AnimationFrame == animation.currentFrame) scene.PlayWorldSound(s.Sound, position, s.Volume);
                        }

                    if (animation.particles != null) foreach (AnimationParticle p in animation.particles)
                        {
                            if (p.AnimationFrame == animation.currentFrame) new Particle(new Animation(p.ParticleAnimation.spriteSheet, p.ParticleAnimation.frames, p.ParticleAnimation.frameRate, p.ParticleAnimation.cellSize, p.ParticleAnimation.loopAnim), position, p.Lifetime, new Vector2(p.velocity.X * direction, p.velocity.Y), p.acceleration, 0, direction) { stayWithOwner = p.stayWithOwner ? this : null };
                        }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            animation?.Draw(spriteBatch, position, direction, drawColor, drawLayer, rotation, drawScale);

            if ((game.DebugMode || (scene is InGameTraining igt && igt.displayHitboxes)) && !(this is Tile) && collider != null && collider.Box.Width > 0)
            {
                Texture2D rect = Game.LoadTexture("ExampleContent/Pixel");
                spriteBatch.Draw(rect, new Rectangle(new Point((int)collider.Box.X, (int)collider.Box.Y), new Point((int)collider.Box.Width, (int)collider.Box.Height)), null,
                    new Color(this is Hitbox ? Color.Red : Color.Green, 0.25f), 0, Vector2.Zero, SpriteEffects.None, Math.Max(0.81f, drawLayer + 0.01f));
            }
        }

        public virtual void DrawBloom(SpriteBatch spriteBatch) { }

        public void DestroyHitboxes(bool includeProjectiles = false)
        {
            foreach (Hitbox o in scene.hitboxes.ToArray())
            {
                if (o.owner == this && (includeProjectiles || !(o is Projectile)))
                {
                    o.Destroy();
                }
            }
        }

        public void UpdateGravity()
        {
            if (velocity.Y < fallSpeed && !Grounded)
            {
                velocity.Y += gravity;
            }
        }

        public void UpdatePosition()
        {
            if (pushEntities) EntityPush();

            if (collider != null) CollisionX();
            position.X += velocity.X;
            if (collider != null) CollisionY();
            position.Y += velocity.Y;
            if (collider != null) CollisionCorrection();
        }

        //Called after Update regardless of pause
        public virtual void AddLights() { }

        protected virtual void EntityPush()
        {
            foreach (GameObject obj in scene.GetObjectsOfType<GameObject>())
            {
                if (obj != this && TouchingObject(collider.Box, obj) && obj.pushEntities)
                {
                    if (Math.Sign(obj.position.X - position.X) == Math.Sign(velocity.X) && position.X != obj.position.X)
                    {
                        if (Math.Sign(obj.position.X - position.X) == -1 && obj.TouchingTile(obj.collider.Left, -2, 0) || Math.Sign(obj.position.X - position.X) == 1 && obj.TouchingTile(obj.collider.Right, 2, 0))
                        {
                            position.X -= velocity.X;
                        }
                        else
                        {
                            float pushVal = Math.Abs(velocity.X) / 2 * (pushWeight / obj.pushWeight);
                            if (pushVal > Math.Abs(velocity.X)) pushVal = Math.Abs(velocity.X);
                            position.X -= pushVal * Math.Sign(velocity.X);
                            obj.position.X += pushVal * Math.Sign(velocity.X);
                        }
                    }
                    if (position.X == obj.position.X)
                    {
                        if (position.Y < obj.position.Y)
                        {
                            if (TouchingTile(collider.Right, 8, 0)) position.X -= 1;
                            if (TouchingTile(collider.Left, -8, 0)) position.X += 1;
                        }
                    }
                    if (velocity.X == 0 && obj.velocity.X == 0 || Math.Sign(velocity.X) == Math.Sign(obj.velocity.X))
                    {
                        position.X -= 4 * Math.Sign(obj.position.X - position.X);
                    }

                    CollisionCorrection();
                    obj.CollisionCorrection();
                }
            }
        }

        public void CollisionY()
        {
            if (!ignoreTiles && velocity.Y < 0 && TouchingTile(collider.Top, 0, velocity.Y))
            {
                velocity.Y = 0;
                while (!TouchingTile(collider.Box, 0, -0.1f))
                {
                    position.Y -= 0.1f;
                }
            }
            if (!ignoreTiles && velocity.Y > 0 && TouchingTile(collider.Bottom, 0, velocity.Y))
            {
                velocity.Y = 0;
                while (!TouchingTile(collider.Box, 0, 0.1f))
                {
                    position.Y += 0.1f;
                }
            }
        }

        public void CollisionX()
        {
            if (!ignoreTiles && velocity.X < 0 && TouchingTile(collider.Left, velocity.X, 0))
            {
                if (Grounded && !TouchingTile(collider.Left, velocity.X, -16))
                {
                    while (TouchingTile(collider.Bottom, velocity.X, -1)) position.Y--;
                }
                else
                {
                    velocity.X = 0;
                    while (!TouchingTile(collider.Box, -1, 0))
                    {
                        position.X -= 0.1f;
                    }
                }
            }
            if (!ignoreTiles && velocity.X > 0 && TouchingTile(collider.Right, velocity.X, 0))
            {
                if (Grounded && !TouchingTile(collider.Right, velocity.X, -16))
                {
                    while (TouchingTile(collider.Bottom, velocity.X, -1)) position.Y--;
                }
                else
                {
                    velocity.X = 0;
                    while (!TouchingTile(collider.Box, 1, 0))
                    {
                        position.X += 0.1f;
                    }
                }
            }
            if (Grounded && TouchingTile(collider.Bottom, velocity.X, 8))
            {
                while (Grounded && !TouchingTile(collider.Bottom, velocity.X, 0))
                {
                    position.Y++;
                }
            }
        }

        public void CollisionCorrection()
        {
            if (!ignoreTiles && TouchingTile(collider.Box, 0, 0))
            {
                if (TouchingTile(collider.Left, 0, 0) && TouchingTile(collider.Right, 0, 0))
                {
                    while (TouchingTile(collider.Bottom, 0, 0))
                    {
                        position.Y -= 0.1f;
                    }
                }
                else
                {
                    while (TouchingTile(collider.Left, 0, 0))
                    {
                        position.X += 0.1f;
                    }

                    while (TouchingTile(collider.Right, 0, 0))
                    {
                        position.X -= 0.1f;
                    }
                }
            }
        }

        public bool TouchingTile(RectangleF collide, float offsetX, float offsetY)
        {
            collide.X += offsetX;
            collide.Y += offsetY;
            List<GameObject> list = scene.solidObjects.ToList();
            list.Remove(this);
            foreach (GameObject obj in list)
            {
                if (obj.collider != null && collide.Intersects(obj.collider.Box) && collider.CollisionCondition(collide, obj.collider.Box) && obj.collider.CollisionCondition(obj.collider.Box, collide))
                {
                    return true;
                }
            }

            int w = (int)Math.Ceiling(collider.Box.Width / TileMap.Tile_Size) + 1;
            int h = (int)Math.Ceiling(collider.Box.Height / TileMap.Tile_Size) + 1;

            for (int x = (int)((position.X + offsetX) / TileMap.Tile_Size - 3); x <= (int)((position.X + offsetX) / TileMap.Tile_Size) + 3; x++)
            {
                for (int y = (int)((position.Y + offsetY) / TileMap.Tile_Size - 3); y <= (int)((position.Y + offsetY) / TileMap.Tile_Size) + 3; y++)
                {
                    if (x < 0 || x >= scene.tiles.GetLength(1) || y < 0 || y >= scene.tiles.GetLength(0)) continue;
                    Tile t = scene.tiles[y, x];
                    if (t != null && collide.Intersects(t.collider.Box) && collider.CollisionCondition(collide, t.collider.Box) && t.collider.CollisionCondition(t.collider.Box, collide) && !(t.platform && (t.collider.Box.Top < collider.Box.Bottom || FallThroughPlatforms))) return true;
                }
            }
            return false;
        }
        public virtual bool FallThroughPlatforms => false;

        public GameObject[] TouchingObjects(RectangleF collide)
        {
            List<GameObject> objects = new List<GameObject>();
            if (game.Scene is InGame scene)
            {
                List<GameObject> list = scene.miscObjects.Where(o => (o.position - position).Length() < 320).ToList();
                list.Remove(this);
                foreach (GameObject obj in list)
                {
                    if (obj.collider != null)
                    {
                        if (collide.Intersects(obj.collider.Box) && collider.CollisionCondition(collide, obj.collider.Box) && obj.collider.CollisionCondition(obj.collider.Box, collide))
                        {
                            objects.Add(obj);
                        }
                    }
                }
            }
            return objects.ToArray();
        }

        public bool TouchingObject(RectangleF collide, GameObject target)
        {
            if (target.collider != null && collide.Intersects(target.collider.Box) && collider.CollisionCondition(collide, target.collider.Box) && target.collider.CollisionCondition(target.collider.Box, collide))
            {
                return true;
            }
            return false;
        }

        public bool TouchingObjectOfType<T>(RectangleF collide, out T obj)
        {
            if (game.Scene is InGame scene)
            {
                List<GameObject> list = scene.miscObjects.Where(o => (o.position - position).Length() < 320).ToList();
                list.Remove(this);
                foreach (GameObject o in list)
                {
                    if (o is T t && o.collider != null && collide.Intersects(o.collider.Box) && collider.CollisionCondition(collide, o.collider.Box) && o.collider.CollisionCondition(o.collider.Box, collide))
                    {
                        obj = t;
                        return true;
                    }
                }
            }
            obj = default;
            return false;
        }

        public virtual void Destroy()
        {
            if (game.Scene is InGame scene)
            {
                scene.miscObjects.Remove(this);
                scene.solidObjects.Remove(this);
                if (this is Hitbox h) scene.hitboxes.Remove(h);
                if (this is Particle p) scene.particles.Remove(p);
                destroyed = true;
            }
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool Grounded => TouchingTile(collider.Bottom, 0, 0) && velocity.Y >= 0;

        public virtual Dictionary<string, object> GetState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            if (animation != null)
            {
                state.Add("animation", animation);
                state.Add("animationFrame", animation.currentFrame);
                state.Add("animationTimer", animation.timer);
            }
            state.Add("direction", direction);
            state.Add("rotation", rotation);
            state.Add("position", position);
            state.Add("velocity", velocity);
            state.Add("destroyed", destroyed);

            return state;
        }

        public virtual void LoadState(Dictionary<string, object> state)
        {
            if (state.ContainsKey("animation"))
            {
                animation = (Animation)state["animation"];
                animation.currentFrame = (int)state["animationFrame"];
                animation.timer = (int)state["animationTimer"];
            }
            direction = (int)state["direction"];
            rotation = (float)state["rotation"];
            position = (Vector2)state["position"];
            velocity = (Vector2)state["velocity"];
            destroyed = (bool)state["destroyed"];
        }
    }
}
