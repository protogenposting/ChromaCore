using RCArena.Code.Effects;
using RCArena.Code.Objects;

namespace RCArena.Code.Utils.Combat
{
    public class Attack
    {
        /// <summary>
        /// <para>The animation played during the attack</para>
        /// </summary>
        public Animation anim;
        /// <summary>
        /// The button combination used to access the move
        /// </summary>
        public AttackInput input;
        /// <summary>
        /// <para>The amount of frames before the attack ends</para>
        /// <para>By default, it is the animation frames times the frame rate</para>
        /// </summary>
        public int duration;
        /// <summary>
        /// <para>Unique identifier for any given attack, to be used with AttackUpdate()</para>
        /// <para>Empty by default, which has no effect on anything</para>
        /// </summary>
        public string name = "";
        /// <summary>
        /// <para>If 0, the attack works in the air and on the ground.</para>
        /// <para>If 1, only works on the ground.</para>
        /// <para>If 2, only works in the air.</para>
        /// <para>0 by default</para>
        /// </summary>
        public int groundedness = 0;
        /// <summary>
        /// <para>Whether the move is forced to end if groundedness isn't satisfied</para>
        /// <para>true by default</para>
        /// </summary>
        public bool forceGroundedness = true;
        /// <summary>
        /// <para>Whether the attack leaves you in air stall when it ends</para>
        /// <para>false by default</para>
        /// </summary>
        public bool airStall = false;
        /// <summary>
        /// <para>An extra check for whether this move will be available in the usual attack check</para>
        /// <para>null by default, meaning the check is ignored</para>
        /// </summary>
        public Predicate<Attack> canUse = null;
        /// <summary>
        /// <para>Whether the move can be canceled into other moves</para>
        /// <para>true by default</para>
        /// </summary>
        public bool canCancel = true;
        /// <summary>
        /// <para>What the move can be canceled into on hit</para>
        /// <para>1 = light level</para>
        /// <para>2 = medium level</para>
        /// <para>3 = heavy / command normal level</para>
        /// <para>4 = input special level</para>
        /// <para>5 = super level</para>
        /// <para>1 by default</para>
        /// </summary>
        public int cancelLevel = 1;
        /// <summary>
        /// <para>Whether the move can be jump canceled on hit</para>
        /// <para>false by default</para>
        /// </summary>
        public bool jumpCancelable = false;
        /// <summary>
        /// <para>How many frames you are left in landing lag from the attack</para>
        /// <para>Only applies if groundedness is 2 or aairstall is true</para>
        /// <para>0 by default</para>
        /// </summary>
        public int landingLag = 0;
        /// <summary>
        /// <para>Data structure to spawn hitboxes during the move</para>
        /// <para>null by default</para>
        /// </summary>
        public HitboxSpawner[] hitboxes;
        /// <summary>
        /// <para>Data structure to apply velocity during the move</para>
        /// <para>null by default</para>
        /// </summary>
        public Pusher[] pushers;
        /// <summary>
        /// <para>Replaces the normal hurtbox with this one for the duration of the attack</para>
        /// <para>null by default, meaning it won't replace it</para>
        /// </summary>
        public Collider primaryHurtbox;
        /// <summary>
        /// <para>Whether to skip the crouch animation when transitioning into a crouching state</para>
        /// <para>false by default</para>
        /// </summary>
        public bool holdCrouch = false;
        /// <summary>
        /// <para>A method to run alongside the player's AttackUpdate method</para>
        /// <para>null</para>
        /// </summary>
        public Action<Attack> attackUpdate = null;

        /// <summary>
        /// Calculates roughly how far away the initial hitbox reaches
        /// </summary>
        public int EffectiveRange
        {
            get
            {
                if (overrideEffectiveRange != -1) return overrideEffectiveRange;
                if (hitboxes == null || hitboxes.Length == 0) return -1;
                List<HitboxSpawner> list = hitboxes.Where(h => h.creationFrame <= hitboxes[0].creationFrame).ToList();

                int range = 0;
                foreach (HitboxSpawner h in list)
                {
                    int baseRange = (int)h.offset.X + (int)h.size.X / 2;
                    if (h is ProjectileSpawner p) baseRange += (int)(p.velocity.X * p.lifetime);
                    if (baseRange > range) range = baseRange;
                }
                if (pushers != null) foreach (Pusher p in pushers)
                    {
                        if (p.frame < hitboxes[0].creationFrame) range += (int)(p.vel.X * (hitboxes[0].creationFrame - p.frame) * 0.75f);
                    }

                return range;
            }
        }

        /// <summary>
        /// Overrides the return value for "EffectiveRange" as it is used by the AI
        /// </summary>
        public int overrideEffectiveRange = -1;

        public int FirstActiveFrame => hitboxes.Length > 0 ? hitboxes[0].creationFrame : -1;

        public Attack() { }


        /// <summary>
        /// <para>Programmable base data for an attack. Takes an animation and generic data to start, any data can be modified from its default. Attacks can be further manipulated with AttackUpdate()</para>
        /// <para>Available variables:</para>
        /// <para>duration</para>
        /// <para>groundedness</para>
        /// <para>airStall</para>
        /// <para>cancelLevel</para>
        /// <para>jumpCancelable</para>
        /// <para>hitboxes</para>
        /// <para>Pushers</para>
        /// <para>holdCrouch</para>
        /// </summary>
        /// <param name="animation">Animation to use during the attack</param>
        public Attack(Animation animation, AttackInput inputCommand = null)
        {
            anim = animation;
            duration = animation.frames * animation.frameRate;
            input = inputCommand;
        }
    }

    public enum InputMotions
    {
        None,
        Down,
        Forward,
        Back,
        DownForward,
        DownBack,
        QuarterForward,
        QuarterBack,
        HalfForward,
        HalfBack,
        DPForward,
        DPBackward,
        ChargeBack,
        ChargeDown,
        DownDown
    }

    public class AttackInput
    {
        public int priority;
        private int button;
        private InputMotions motion;

        public int Button { get => button; }
        public InputMotions Motion { get => motion; }

        public AttackInput() { }

        /// <summary>
        /// Used to determine what controller inputs call the attack
        /// </summary>
        /// <param name="button">The Button to be pressed</param>
        /// <param name="motion">The direction of the stick or motion it must perform</param>
        /// <param name="priority"><para>Whether this input will override other inputs</para>
        /// <para>By it's default of -1:</para>
        /// <para>DP motions have a priority of 7</para>
        /// <para>Half circles have a priority of 6</para>
        /// <para>Quarter circles have a priority of 5</para>
        /// <para>Charge motions have a priority of 4</para>
        /// <para>Diagonal directions have a priority of 3</para>
        /// <para>Vertical and horizontal directions have a priority of 2</para>
        /// <para>No direction has a priority of 1</para></param>
        public AttackInput(int button, InputMotions motion = InputMotions.None, int priority = -1)
        {
            this.button = button;
            this.motion = motion;
            if (priority == -1)
            {
                if (motion == InputMotions.DPForward || motion == InputMotions.DPBackward) this.priority = 7;
                if (motion == InputMotions.HalfForward || motion == InputMotions.HalfBack) this.priority = 6;
                if (motion == InputMotions.QuarterForward || motion == InputMotions.QuarterBack || motion == InputMotions.DownDown) this.priority = 5;
                if (motion == InputMotions.ChargeBack || motion == InputMotions.ChargeDown) this.priority = 4;
                if (motion == InputMotions.DownForward || motion == InputMotions.DownBack) this.priority = 3;
                if (motion == InputMotions.Down || motion == InputMotions.Forward || motion == InputMotions.Back) this.priority = 2;
                if (motion == InputMotions.None) this.priority = 1;
            }
            else this.priority = priority;
        }

        public bool CheckButton(Controller input)
        {
            if (input.KeyPressed(button)) return true;
            return false;
        }
        public bool CheckMotion(Controller input, int direction)
        {
            switch (motion)
            {
                case InputMotions.None: return true;
                case InputMotions.Down: if (input.KeyDown(Controller.Key_Down)) return true; break;
                case InputMotions.Forward: if ((direction == 1 && input.KeyDown(Controller.Key_Right) || direction == -1 && input.KeyDown(Controller.Key_Left)) && !input.KeyDown(Controller.Key_Down)) return true; break;
                case InputMotions.Back: if (direction == 1 && input.KeyDown(Controller.Key_Left) || direction == -1 && input.KeyDown(Controller.Key_Right)) return true; break;
                case InputMotions.DownForward: if (input.KeyDown(Controller.Key_Down) && (direction == 1 && input.KeyDown(Controller.Key_Right) || direction == -1 && input.KeyDown(Controller.Key_Left))) return true; break;
                case InputMotions.DownBack: if (input.KeyDown(Controller.Key_Down) && (direction == 1 && input.KeyDown(Controller.Key_Left) || direction == -1 && input.KeyDown(Controller.Key_Right))) return true; break;
                case InputMotions.QuarterForward: if (input.MotionQCF) return true; break;
                case InputMotions.QuarterBack: if (input.MotionQCB) return true; break;
                case InputMotions.HalfForward: if (input.MotionHCF) return true; break;
                case InputMotions.HalfBack: if (input.MotionHCB) return true; break;
                case InputMotions.DPForward: if (input.MotionDPF) return true; break;
                case InputMotions.DPBackward: if (input.MotionDPB) return true; break;
                case InputMotions.ChargeBack: if (input.MotionCharegBack) return true; break;
                case InputMotions.ChargeDown: if (input.MotionCharegDown) return true; break;
                case InputMotions.DownDown: if (input.MotionDoubleDown) return true; break;
            }
            return false;
        }

        public bool CheckInput(Controller input, int direction)
        {
            if (CheckButton(input) && CheckMotion(input, direction)) return true;
            else return false;
        }
    }

    public class HitboxSpawner
    {
        protected Fighter owner;
        /// <summary>
        /// <para>Action run for the hitbox when it is created</para>
        /// <para>does nothing by default</para>
        /// </summary>
        public Action<Hitbox> modifiers = null;
        /// <summary>
        /// <para>Redirects to a different spawner within the attack</para>
        /// <para>-1 by default, meaning it won't redirect</para>
        /// </summary>
        public int parent = -1;
        /// <summary>
        /// <para>The frame of the attack when the hitbox spawns</para>
        /// <para>-1 by default, meaning it won't spawn</para>
        /// </summary>
        public int creationFrame = -1;
        /// <summary>
        /// <para>The damage that the hitbox deals</para>
        /// <para>1 by default</para>
        /// </summary>
        public int damage = 1;
        /// <summary>
        /// <para>The damage dealt when the opponent blocks the hitbox</para>
        /// <para>0 by default</para>
        /// </summary>
        public int chipDamage = 0;
        /// <summary>
        /// <para>The knockback that the hitbox deals</para>
        /// <para>No knockback by default</para>
        /// </summary>
        public Knockback knockback = new Knockback(0, 0, 0, false);
        /// <summary>
        /// <para>How much the knockback can be changed by Directional Influence in degrees</para>
        /// <para>0 by default</para>
        /// </summary>
        public float DIFactor = 0;
        /// <summary>
        /// <para>The amount of frames the hitbox lasts</para>
        /// <para>3 by default</para>
        /// </summary>
        public int lifetime = 3;
        /// <summary>
        /// <para>The position where the hitbox spawns relative to the player</para>
        /// <para>0 by default</para>
        /// </summary>
        public Vector2 offset = Vector2.Zero;
        /// <summary>
        /// <para>The size of the hitbox</para>
        /// <para>0 by default</para>
        /// </summary>
        public Vector2 size = Vector2.Zero;
        /// <summary>
        /// <para>The duration that the game pauses on hit</para>
        /// <para>4 by default</para>
        /// </summary>
        public int hitpause = 4;
        /// <summary>
        /// <para>The duration the opponent stays in hitstun</para>
        /// <para>20 by default</para>
        /// </summary>
        public int hitstun = 20;
        /// <summary>
        /// <para>The way the opponent is put in hitstun</para>
        /// <para>PreKnockDown: opponent will be forced into a knockdown when they land</para>
        /// <para>Launcher: target deccelerates horizontally, knockback is reduced if used more than once in a combo</para>
        /// <para>PreWallBounce: opponent will bounce off of the wall if possible once per combo</para>
        /// <para>PreGroundBounce: opponent will bounce off of the ground instead of landing once per combo</para>
        /// <para>PreForceGroundBounce: opponent will bounce off of the ground instead of landing</para>
        /// </summary>
        public List<Fighter.HitstunProperties> hitstunProperties = new List<Fighter.HitstunProperties>();
        /// <summary>
        /// <para>Whether or not the hitbox will hit an opponent in a hard knockdown state</para>
        /// <para>false by default</para>
        /// </summary>
        public bool canOTG = false;
        /// <summary>
        /// <para>If hitstunProperties contains Knockdown, the number of frames the target will remain in the knockdown state</para>
        /// <para>40 by default</para>
        /// </summary>
        public int knockdownDuration = 40;
        /// <summary>
        /// <para>The duration the opponent stays locked in their shield</para>
        /// <para>16 by default</para>
        /// </summary>
        public int blockstun = 16;
        /// <summary>
        /// <para>The distance players are pushed apart when the hitbox hits on block</para>
        /// <para>4 by default</para>
        /// </summary>
        public int blockPush = 4;
        /// <summary>
        /// <para>How the hitbox behaves on block</para>
        /// <para>0 by default</para>
        /// </summary>
        public HitTypes hitType = HitTypes.Normal;
        /// <summary>
        /// <para>Controls what hurt animation, if available, will be played when hit by this hitbox on the ground</para>
        /// <para>Auto by default, meaning it is calculated by the hit type</para>
        /// </summary>
        public GroundHurtAnimationSelection groundHurtAnimation = GroundHurtAnimationSelection.Auto;
        /// <summary>
        /// <para>Controls what hurt animation, if available, will be played when hit by this hitbox in the air</para>
        /// <para>Auto by default, meaning it is calculated by the hit trajectory</para>
        /// </summary>
        public AirHurtAnimationSelection airHurtAnimation = AirHurtAnimationSelection.Auto;
        /// <summary>
        /// <para>If true, this will be treated as hitting from the opposite direction that it actually hit</para>
        /// <para>false by default</para>
        /// </summary>
        public bool flipCrossUp = false;
        /// <summary>
        /// <para>Hitboxes with the same group value will only have one hit</para>
        /// <para>A value of -1 means it will always hit</para>
        /// <para>-1 by default</para>
        /// </summary>
        public int hitboxGroup = -1;
        /// <summary>
        /// <para>URL for the sound that plays when the hitbox hits</para>
        /// <para>Has a default hit sound</para>
        /// </summary>
        public string hitSound = "Sounds/Characters/Common/sfx_hit";
        /// <summary>
        /// <para>Particle effect to create when the hitbox connects</para>
        /// <para>Has a default hit particle</para>
        /// </summary>
        public ParticleSpawner hitParticle = null;
        /// <summary>
        /// <para>Methods to run when the hitbox hits an opponent</para>
        /// <para>Hit effects are run before damage is applied, so it can be used to modify properties like damage and knockback</para>
        /// <para>null by default</para>
        /// </summary>
        public Action<Hitbox, Fighter> preHitEffect = null;
        /// <summary>
        /// <para>Like preHitEffects, but it runs after damage and knockback are applied</para>
        /// <para>null by default</para>
        /// </summary>
        public Action<Hitbox, Fighter> postHitEffect = null;
        /// <summary>
        /// <para>Like preHitEffects, applying when the hitbox is blocked</para>
        /// <para>null by default</para>
        /// </summary>
        public Action<Hitbox, Fighter> onBlockHitEffect = null;
        /// <summary>
        /// <para>Whether the hitbox can hit the enemy</para>
        /// <para>true by default</para>
        /// </summary>
        public bool active = true;
        /// <summary>
        /// <para>If false, this hitbox will never increase the combo counter.</para>
        /// <para>true by default</para>
        /// </summary>
        public bool affectComboCounter = true;

        public HitboxSpawner() { }

        /// <summary>
        /// <para>Creates a hitbox on the specified frame of the attack.</para>
        /// </summary>
        /// <param name="player">The owner of the hitbox. Should generally be "this."</param>
        public HitboxSpawner(Fighter player)
        {
            owner = player;
        }

        public virtual Hitbox SpawnHitBox()
        {
            Hitbox h = new Hitbox(owner, damage, knockback, DIFactor, size, offset, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSound, hitboxGroup, hitstunProperties, preHitEffect, active)
            {
                hitParticle = hitParticle,
                postHitEffect = postHitEffect,
                onBlockHitEffect = onBlockHitEffect,
                knockdownDuration = knockdownDuration,
                chipDamage = chipDamage,
                flipCrossUp = flipCrossUp,
                canOTG = canOTG,
                affectComboCounter = affectComboCounter,
                groundHurtAnimation = groundHurtAnimation,
                airHurtAnimation = airHurtAnimation
            };

            modifiers?.Invoke(h);
            return h;
        }

        public virtual Hitbox SpawnHitBox(Vector2 overrideSize = default, Vector2 overrideOffset = default)
        {
            Hitbox h = new Hitbox(owner, damage, knockback, DIFactor, overrideSize == default ? size : overrideSize, overrideOffset == default ? offset : overrideOffset, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSound, hitboxGroup, hitstunProperties, preHitEffect, active)
            {
                hitParticle = hitParticle,
                postHitEffect = postHitEffect,
                onBlockHitEffect = onBlockHitEffect,
                knockdownDuration = knockdownDuration,
                chipDamage = chipDamage,
                flipCrossUp = flipCrossUp,
                canOTG = canOTG,
                affectComboCounter = affectComboCounter,
                groundHurtAnimation = groundHurtAnimation,
                airHurtAnimation = airHurtAnimation
            };

            modifiers?.Invoke(h);
            return h;
        }
    }

    public class ProjectileSpawner : HitboxSpawner
    {
        /// <summary>
        /// <para>Delegate run for the projectile when it is created</para>
        /// <para>does nothing by default</para>
        /// </summary>
        new public Action<Projectile> modifiers;
        /// <summary>
        /// Sprite for the projectile
        /// </summary>
        public string spr;
        /// <summary>
        /// Animation for the projectile
        /// </summary>
        public Animation anim = null;
        /// <summary>
        /// <para>A multiplier for the sprite to be drawn</para>
        /// <para>1 by default</para>
        /// </summary>
        public float spriteScale = 1;
        /// <summary>
        /// <para>Velocity for the projectile when it spawns</para>
        /// <para>0 by default</para>
        /// </summary>
        public Vector2 velocity = Vector2.Zero;
        /// <summary>
        /// <para>Gravity for the projectile when it spawns</para>
        /// <para>0 by default</para>
        /// </summary>
        public float gravity = 0;
        /// <summary>
        /// <para>Max fall speed for the projectile when it spawns</para>
        /// <para>0 by default</para>
        /// </summary>
        public float fallSpeed = 0;
        /// <summary>
        /// <para>If the projectile is projected to spawn outside the room's bounds, it will move into them before appearing</para>
        /// <para>true by default</para>
        /// </summary>
        public bool spawnInBounds = true;
        /// <summary>
        /// <para>Transcendent projectiles will not be destroyed by other hitboxes</para>
        /// <para>false by default</para>
        /// </summary>
        public bool transcendent = false;
        /// <summary>
        /// <para>If true, the projectile will be rotated to match its initial velocity when it spawns.</para>
        /// <para>false by default</para>
        /// </summary>
        public bool rotateToVelocity = false;
        /// <summary>
        /// <para>Update method that will be attached to the projectile</para>
        /// </summary>
        public Action<Projectile> projectileUpdate = null;

        /// <summary>
        /// <para>HitboxSpawner but for projectiles, contains an extra argument for a sprite or animation</para>
        /// </summary>
        /// <param name="player">The owner of the hitbox. Should generally be "this."</param>
        /// <param name="sprite">URL for the sprite of the projectile</param>
        public ProjectileSpawner(Fighter player, string sprite) : base(player)
        {
            spr = sprite;
        }

        /// <summary>
        /// <para>HitboxSpawner but for projectiles, contains an extra argument for a sprite or animation</para>
        /// </summary>
        /// <param name="player">The owner of the hitbox. Should generally be "this."</param>
        /// <param name="sprite">URL for the sprite of the projectile</param>
        public ProjectileSpawner(Fighter player, Animation animation) : base(player)
        {
            anim = animation;
        }

        public override Projectile SpawnHitBox()
        {
            Projectile p = null;
            if (anim != null) p = new Projectile(owner, damage, knockback, DIFactor, size, offset, anim, velocity, gravity, fallSpeed, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSound, spriteScale, projectileUpdate, hitboxGroup, hitstunProperties, preHitEffect, spawnInBounds, active)
            {
                hitParticle = hitParticle,
                transcendent = transcendent,
                postHitEffect = postHitEffect,
                onBlockHitEffect = onBlockHitEffect,
                knockdownDuration = knockdownDuration,
                chipDamage = chipDamage,
                flipCrossUp = flipCrossUp,
                canOTG = canOTG,
                affectComboCounter = affectComboCounter,
                groundHurtAnimation = groundHurtAnimation,
                airHurtAnimation = airHurtAnimation
            };
            else p = new Projectile(owner, damage, knockback, DIFactor, size, offset, spr, velocity, gravity, fallSpeed, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSound, spriteScale, projectileUpdate, hitboxGroup, hitstunProperties, preHitEffect, spawnInBounds, active)
            {
                hitParticle = hitParticle,
                transcendent = transcendent,
                postHitEffect = postHitEffect,
                onBlockHitEffect = onBlockHitEffect,
                knockdownDuration = knockdownDuration,
                chipDamage = chipDamage,
                flipCrossUp = flipCrossUp,
                canOTG = canOTG,
                affectComboCounter = affectComboCounter,
                groundHurtAnimation = groundHurtAnimation,
                airHurtAnimation = airHurtAnimation
            };
            if (rotateToVelocity) p.rotation = MathHelper.ToDegrees((float)Math.Atan2(velocity.Y, velocity.X)) * p.direction;

            modifiers?.Invoke(p);
            return p;
        }

        public override Projectile SpawnHitBox(Vector2 overrideSize = default, Vector2 overrideOffset = default)
        {
            Projectile p = null;
            if (anim != null) p = new Projectile(owner, damage, knockback, DIFactor, overrideSize == default ? size : overrideSize, overrideOffset == default ? offset : overrideOffset, anim, velocity, gravity, fallSpeed, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSound, spriteScale, projectileUpdate, hitboxGroup, hitstunProperties, preHitEffect, spawnInBounds, active)
            {
                hitParticle = hitParticle,
                transcendent = transcendent,
                postHitEffect = postHitEffect,
                onBlockHitEffect = onBlockHitEffect,
                knockdownDuration = knockdownDuration,
                chipDamage = chipDamage,
                flipCrossUp = flipCrossUp,
                canOTG = canOTG,
                affectComboCounter = affectComboCounter
            };
            else p = new Projectile(owner, damage, knockback, DIFactor, overrideSize == default ? size : overrideSize, overrideOffset == default ? offset : overrideOffset, spr, velocity, gravity, fallSpeed, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSound, spriteScale, projectileUpdate, hitboxGroup, hitstunProperties, preHitEffect, spawnInBounds, active)
            {
                hitParticle = hitParticle,
                transcendent = transcendent,
                postHitEffect = postHitEffect,
                onBlockHitEffect = onBlockHitEffect,
                knockdownDuration = knockdownDuration,
                chipDamage = chipDamage,
                flipCrossUp = flipCrossUp,
                canOTG = canOTG,
                affectComboCounter = affectComboCounter
            };
            modifiers?.Invoke(p);
            return p;
        }
    }

    public struct Pusher
    {
        public int frame;
        public Vector2 vel;

        public Pusher(int pushFrame, Vector2 velocity)
        {
            frame = pushFrame;
            vel = velocity;
        }

        public void Push(Fighter player)
        {
            player.velocity = vel;
            player.velocity.X *= player.direction;
        }
    }

    public struct Knockback
    {
        public float baseKB;
        public float scaling;
        public float angle;
        public bool groundLaunch;

        public Knockback(float baseKnockBack, float knockBackScalingFactor, float knockBackAngle, bool launchFromGround = false)
        {
            baseKB = baseKnockBack;
            scaling = knockBackScalingFactor;
            angle = knockBackAngle;
            groundLaunch = launchFromGround;
        }
    }

    public enum HitTypes
    {
        Normal,
        Overhead,
        Low,
        Grab,
        AirGrab
    }

    public enum GroundHurtAnimationSelection
    {
        Auto,
        Normal,
        Crouch,
        Overhead
    }
    public enum AirHurtAnimationSelection
    {
        Auto,
        Normal
    }
}
