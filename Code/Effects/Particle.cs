using RCArena.Code.Objects;

namespace RCArena.Code.Effects
{
    public class Particle : GameObject
    {
        public int duration;
        public float acceleration;
        public bool updateDuringHitpause = true;
        public GameObject stayWithOwner = null;

        public Particle(Animation animation, Vector2 position, int lifetime, Vector2 velocity, float acceleration = 1, float rotation = 0, int direction = 0) : base()
        {
            drawLayer = 0.9f + game.randomLayerOffset;
            this.animation = animation;
            this.position = position;
            duration = lifetime;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.rotation = rotation;
            this.direction = direction;

            scene.particles.Add(this);
        }

        public override void Update()
        {
            base.Update();
            duration--;

            if (stayWithOwner != null)
            {
                position = stayWithOwner.position;
            }
            else
            {
                velocity *= acceleration;
                position += velocity;
            }

            if (duration <= 0)
            {
                Destroy();
            }
        }
    }

    public class ParticleSpawner
    {
        public Animation animation;
        public int duration;
        public Vector2 velocity;
        public float acceleration;
        public float rotation = 0;
        public float layer = 0.6f;
        public GameObject stayWithOwner;
        public bool additiveBlend = false;
        public Color drawColor = Color.White;
        public bool dynamicRotation = false;

        public ParticleSpawner(Animation animation, int duration, Vector2 velocity, float acceleration = 1, float rotation = 0)
        {
            this.animation = animation;
            this.duration = duration;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.rotation = rotation;
        }

        public virtual Particle Spawn(Vector2 position, int direction, int rotation = 0)
        {
            return new Particle(new Animation(animation.spriteSheet, animation.frames, animation.frameRate, animation.loopAnim), position, duration, velocity, acceleration, (dynamicRotation ? rotation : this.rotation) * -direction, direction) { stayWithOwner = stayWithOwner, drawColor = drawColor, drawLayer = layer };
        }
    }

    public class MultiParticleSpawner : ParticleSpawner
    {
        public List<ParticleSpawner> particles;

        public MultiParticleSpawner(List<ParticleSpawner> particles) : base(null, 0, Vector2.Zero)
        {
            this.particles = particles;
        }

        public override Particle Spawn(Vector2 position, int direction, int rotation = 0)
        {
            if (particles.Count == 0) return null;
            for (int i = 1; i < particles.Count; i++) particles[i].Spawn(position, direction, rotation);
            return particles[0].Spawn(position, direction, rotation);
        }
    }
}
