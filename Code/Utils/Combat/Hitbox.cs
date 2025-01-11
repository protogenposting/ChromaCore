using RCArena.Code.Effects;
using RCArena.Code.Objects;
using RCArena.Code.Scenes;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection.Emit;
using static RCArena.Code.Objects.Fighter;

namespace RCArena.Code.Utils.Combat
{
    public class Hitbox : GameObject
    {
        public Fighter owner;
        public int dmg;
        public int chipDamage;
        public Knockback kb;
        public float DIFactor;
        public int stun;
        public int pause;
        public int life;
        public int timer = 0;
        public Vector2 offset;
        public List<GameObject> hitObjects;
        public HitTypes hitType;
        public GroundHurtAnimationSelection groundHurtAnimation = GroundHurtAnimationSelection.Auto;
        public AirHurtAnimationSelection airHurtAnimation = AirHurtAnimationSelection.Auto;
        public int blockstun;
        public int blockPush;
        public int group;
        public List<HitstunProperties> hitstunProperties;
        public int knockdownDuration;
        public SoundEffect hitSound = null;
        public ParticleSpawner hitParticle;
        public bool active = true;
        public Attack parentAttack;
        public bool allowCancel = true;
        public bool flipCrossUp = false;
        public bool canOTG = false;
        public bool affectComboCounter = true;

        public Action<Hitbox, Fighter> preHitEffect;
        public Action<Hitbox, Fighter> postHitEffect;
        public Action<Hitbox, Fighter> onBlockHitEffect;

        public Hitbox(Fighter player, int damage, Knockback knockback, float DIFactor, Vector2 size, Vector2 positionOffset, int hitstun, int lifetime = 3, int hitpause = 4, HitTypes hitType = 0, int blockstun = 10, int blockPush = 4, string hitSfx = "Sounds/Hit", int hitboxGroup = -1, List<HitstunProperties> hitstunProperties = null, Action<Hitbox, Fighter> hitEffect = null, bool active = true) : base()
        {
            owner = player;
            dmg = damage;
            kb = knockback;
            this.DIFactor = DIFactor;
            offset = positionOffset;
            stun = hitstun;
            life = lifetime;
            pause = hitpause;
            collider = new Collider(this, (int)size.X, (int)size.Y, new Vector2(-size.X / 2, -size.Y / 2));
            this.hitType = hitType;
            this.blockstun = blockstun;
            this.blockPush = blockPush;
            group = hitboxGroup;
            this.hitstunProperties = hitstunProperties;
            preHitEffect = hitEffect;
            if (hitSfx != "") hitSound = Game.LoadAsset<SoundEffect>(hitSfx);
            else hitSound = null;
            this.active = active;
            direction = owner.direction;
            parentAttack = owner.attack;

            position = owner.position + offset;
            if (owner.direction == -1) position.X -= offset.X * 2;

            scene.hitboxes.Add(this);

            hitObjects = new List<GameObject>();
        }

        public override void Update()
        {
            if (destroyed) return;
            position = owner.position + offset;
            if (owner.direction == -1) position.X -= offset.X * 2;

            if (owner.state == States.Attack)
            {
                foreach (Hitbox h in scene.hitboxes.ToArray())
                {
                    if (this != h && h.owner != owner && !owner.overrideHitboxGroups.Contains(group) && !h.owner.overrideHitboxGroups.Contains(h.group) && TouchingObject(collider.Box, h) && !hitType.ToString().Contains("Grab") && !h.hitType.ToString().Contains("Grab") && !(this is Projectile proj && proj.transcendent) && !(h is Projectile p && p.transcendent))
                    {
                        if (h.dmg > dmg / 2f)
                        {
                            Destroy();
                            h.owner.overrideHitboxGroups.Add(h.group);
                        }
                        h.Destroy();
                        owner.overrideHitboxGroups.Add(group);
                        new Particle(new Animation("Particles/Common/prt_clash", 4, 4, new Vector2(25), false), (position + h.position) / 2, 16, Vector2.Zero);
                        return;
                    }
                }
                foreach (Fighter player in scene.players.ToArray())
                {
                    foreach (Collider hurtbox in player.hurtboxes)
                    {
                        if (player != owner && collider.Box.Intersects(hurtbox.Box) && !hitObjects.Contains(player) && (!owner.overrideHitboxGroups.Contains(group) || group == -1) && active)
                        {
                            if (player.GotHit(this, out int counter))
                            {
                                ApplyHit(player, counter);
                                hitObjects.Add(player);
                                owner.overrideHitboxGroups.Add(group);
                            }
                        }
                    }
                }
            }

            if (owner.attack != parentAttack || owner.state != States.Attack) allowCancel = false;

            timer++;
            if (timer > life) Destroy();
        }

        public void ApplyHit(Fighter target, int counterHitType)
        {
            int tempDmg = dmg;
            if (counterHitType == 0) tempDmg *= 2;

            //Hit Effects
            if (affectComboCounter && !owner.hasHitPlayer && hitType != HitTypes.Grab && hitType != HitTypes.AirGrab) target.comboCounter++;
            if (preHitEffect != null) preHitEffect(this, target);
            owner.HitPlayer(this, target, ref tempDmg);

            //Apply damage
            target.health -= tempDmg;
            if (scene is InGameTraining t) t.comboDamage += tempDmg;
            bool toAir = kb.groundLaunch || target.knockdownTimer > 20;

            //Handle Knockback
            if (target.armorFrames <= 0 || hitType.ToString().Contains("Grab"))
            {
                bool wasGrounded = target.Grounded;
                target.velocity = CalculateKnockBack(kb, target);
                if (wasGrounded && !toAir)
                {
                    target.velocity.Y = 0;
                }
                if (wasGrounded && kb.groundLaunch && target.velocity.Y > 0)
                {
                    target.position.Y -= target.velocity.Y * 1.05f;
                }
                target.hitstunProperties.Clear();
                target.hitstunProperties.AddRange(hitstunProperties);
                if (hitstunProperties.Contains(HitstunProperties.Knockdown)) target.knockdownDuration = knockdownDuration;
            }

            //More Effects
            if (hitSound != null) scene.PlayWorldSound(hitSound, position, 0.25f);
            scene.ApplyHitpause(counterHitType != -1 ? pause * 2 : pause);
            if (pause > 0) scene.camera.ShakeCamera(Math.Min(tempDmg, 20) * 0.6f, pause);
            if (hitParticle != null)
            {
                Particle p = hitParticle.Spawn(target.position, this is Projectile ? direction : target.position.X > owner.position.X ? 1 : -1);
                if (counterHitType == 0) p.drawColor = new Color(255, 60, 60);
            }

            //Apply hitstun and combo counting
            if (target.armorFrames <= 0 || hitType.ToString().Contains("Grab"))
            {
                target.hitstunTimer = Math.Min(stun + 4 - (int)target.comboCounter, stun);
                if (target.hitstunTimer < 4) target.hitstunTimer = 4;
                target.direction = Math.Sign(owner.position.X - target.position.X);

                //Set Animation
                if (target.Grounded && !toAir)
                {
                    if (groundHurtAnimation == GroundHurtAnimationSelection.Auto)
                    {
                        if (target.overheadHurtAnim != null && hitType == HitTypes.Overhead && kb.angle < 0) target.animation = target.overheadHurtAnim;
                        else if (target.crouchHurtAnim != null && (target.state == States.Crouch || target.state == States.Attack && target.attack.holdCrouch || target.animation == target.crouchHurtAnim || target.animation == target.overheadHurtAnim))
                            target.animation = target.crouchHurtAnim;
                        else target.animation = target.groundHurtAnim;
                    }
                    else if (groundHurtAnimation == GroundHurtAnimationSelection.Normal) target.animation = target.groundHurtAnim;
                    else if (groundHurtAnimation == GroundHurtAnimationSelection.Crouch && target.crouchHurtAnim != null) target.animation = target.crouchAnim;
                    else if (groundHurtAnimation == GroundHurtAnimationSelection.Overhead && target.overheadHurtAnim != null) target.animation = target.overheadHurtAnim;

                    target.animation.Reset();
                }
                else
                {
                    if (airHurtAnimation == AirHurtAnimationSelection.Auto)
                    {
                        target.animation = target.airHurtAnim;
                    }
                    else if (airHurtAnimation == AirHurtAnimationSelection.Normal) target.animation = target.airHurtAnim;

                    if (target.hitstunProperties.Contains(HitstunProperties.Restand) || target.velocity.Y < 1)
                    {
                        target.animation.Reset();
                    }
                    else if (target.velocity.Y >= target.fallSpeed)
                    {
                        target.animation.currentFrame = target.animation.frames;
                        target.animation.timer = 0;
                    }
                }

                if (target.direction == 0) target.direction = 1;

                target.OnHurt(this, owner);
            }
            else
            {
                target.armorFlash = 15;
            }

            if (postHitEffect != null) postHitEffect(this, target);
        }

        public Vector2 CalculateKnockBack(Knockback kbValue, Fighter target)
        {
            int dir = target.inputDir * direction;
            if (target.input.id == -2 && scene is InGameTraining tr)
            {
                if (tr.diSetting == InGameTraining.DISetting.In) dir = -1;
                if (tr.diSetting == InGameTraining.DISetting.Out) dir = 1;
            }
            float ang = kbValue.angle;
            if (dir == 1) ang.Approach(25 * Math.Sign(kb.angle), DIFactor);
            if (dir == -1) ang.Approach(180 * Math.Sign(kb.angle), DIFactor);
            Vector2 v = new Vector2(kbValue.baseKB * (this is Projectile ? direction : owner.direction) * (float)Math.Cos(MathHelper.ToRadians(ang)), kbValue.baseKB * (float)Math.Sin(MathHelper.ToRadians(-ang)));
            if (v.Y < 0) v.Y = Math.Min(v.Y, -2);
            v.Y *= 1 + (0.01f * DIFactor * dir);
            return v;
        }

        public override Dictionary<string, object> GetState()
        {
            var state = base.GetState();
            state.Add("owner", owner);
            state.Add("dmg", dmg);
            state.Add("chipDamage", chipDamage);
            state.Add("kb", kb);
            state.Add("DIFactor", DIFactor);
            state.Add("stun", stun);
            state.Add("pause", pause);
            state.Add("life", life);
            state.Add("timer", timer);
            state.Add("hitObjects", new List<GameObject>(hitObjects));
            state.Add("blockstun", blockstun);
            state.Add("blockPush", blockPush);
            state.Add("hitstunProperties", new List<HitstunProperties>(hitstunProperties));
            state.Add("knockdownDuration", knockdownDuration);
            state.Add("active", active);
            return state;
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            base.LoadState(state);

            owner = (Fighter)state["owner"];
            dmg = (int)state["dmg"];
            chipDamage = (int)state["chipDamage"];
            kb = (Knockback)state["kb"];
            DIFactor = (float)state["DIFactor"];
            stun = (int)state["stun"];
            pause = (int)state["pause"];
            life = (int)state["life"];
            timer = (int)state["timer"];
            hitObjects = new List<GameObject>((List<GameObject>)state["hitObjects"]);
            blockstun = (int)state["blockstun"];
            blockPush = (int)state["blockPush"];
            hitstunProperties = new List<HitstunProperties>((List<HitstunProperties>)state["hitstunProperties"]);
            knockdownDuration = (int)state["knockdownDuration"];
            active = (bool)state["active"];
        }
    }

    public class Projectile : Hitbox
    {
        public Action<Projectile> projectileUpdate;

        public int aiTimer;
        public bool transcendent = false;

        public Projectile(Fighter player, int damage, Knockback knockback, float comboScaling, Vector2 size, Vector2 positionOffset, string sprite, Vector2 ejectVelocity, float gravity, float fallSpeed, int hitstun, int lifetime = 3, int hitpause = 4, HitTypes hitType = 0, int blockstun = 10, int blockPush = 4, string hitSfx = "Sounds/Hit", float scale = 1, Action<Projectile> updateAI = null, int hitboxGroup = -1, List<HitstunProperties> hitstunProperties = null, Action<Hitbox, Fighter> hitEffect = null, bool spawnInBounds = true, bool active = true) : base(player, damage, knockback, comboScaling, size, positionOffset, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSfx, hitboxGroup, hitstunProperties, hitEffect, active)
        {
            if (sprite != "") animation = new Animation(sprite);
            drawScale = scale;
            direction = player.direction;
            ejectVelocity.X *= direction;
            velocity = ejectVelocity;
            this.gravity = gravity;
            this.fallSpeed = fallSpeed;
            projectileUpdate = updateAI;
            drawLayer = 0.65f;

            ignoreTiles = true;

            if (spawnInBounds && Game.Instance.Scene is InGame scene)
            {
                int i = 0;
                while (position.X < 0 && i < 1000)
                {
                    position.X++;
                    i++;
                }
                i = 0;
                while (position.X > scene.room.bounds.Width && i < 1000)
                {
                    position.X--;
                    i++;
                }
            }
        }

        public Projectile(Fighter player, int damage, Knockback knockback, float comboScaling, Vector2 size, Vector2 positionOffset, Animation animation, Vector2 ejectVelocity, float gravity, float fallSpeed, int hitstun, int lifetime = 3, int hitpause = 4, HitTypes hitType = 0, int blockstun = 10, int blockPush = 4, string hitSfx = "Sounds/Hit", float scale = 1, Action<Projectile> updateAI = null, int hitboxGroup = -1, List<HitstunProperties> hitstunProperties = null, Action<Hitbox, Fighter> hitEffect = null, bool spawnInBounds = true, bool active = true) : base(player, damage, knockback, comboScaling, size, positionOffset, hitstun, lifetime, hitpause, hitType, blockstun, blockPush, hitSfx, hitboxGroup, hitstunProperties, hitEffect, active)
        {
            this.animation = new Animation(animation.spriteSheet, animation.frames, animation.frameRate, animation.cellSize, animation.loopAnim);
            drawScale = scale;
            direction = player.direction;
            ejectVelocity.X *= direction;
            velocity = ejectVelocity;
            this.gravity = gravity;
            this.fallSpeed = fallSpeed;
            projectileUpdate = updateAI;
            drawLayer = 0.65f;

            ignoreTiles = true;

            if (spawnInBounds && Game.Instance.Scene is InGame scene)
            {
                int i = 0;
                while (position.X < 32 && i < 1000)
                {
                    position.X++;
                    i++;
                }
                i = 0;
                while (position.X > scene.room.bounds.Width - 32 && i < 1000)
                {
                    position.X--;
                    i++;
                }
            }
        }

        public override void Update()
        {
            if (animation != null) animation.Update();
            if (projectileUpdate != null) projectileUpdate(this);

            UpdateGravity();
            UpdatePosition();

            foreach (Hitbox h in scene.hitboxes.ToArray())
            {
                if (this != h && h.owner != owner && !owner.overrideHitboxGroups.Contains(group) && !h.owner.overrideHitboxGroups.Contains(h.group) && TouchingObject(collider.Box, h) && !hitType.ToString().Contains("Grab") && !h.hitType.ToString().Contains("Grab") && !(this is Projectile proj && proj.transcendent) && !(h is Projectile p && p.transcendent))
                {
                    if (h.dmg > dmg / 2f) Destroy();
                    h.Destroy();
                    owner.overrideHitboxGroups.Add(group);
                    return;
                }
            }
            foreach (Fighter player in scene.players.ToArray())
            {
                foreach (Collider hurtbox in player.hurtboxes)
                {
                    if (player != owner && collider.Box.Intersects(hurtbox.Box) && !hitObjects.Contains(player) && !(owner.overrideHitboxGroups.Contains(group) && group != -1) && active)
                    {
                        if (player.GotHit(this, out int counter))
                        {
                            ApplyHit(player, counter);
                            owner.overrideHitboxGroups.Add(group);
                            hitObjects.Add(player);
                        }
                    }
                }
            }

            if (owner.attack != parentAttack || owner.state != States.Attack) allowCancel = false;

            timer++;
            if (timer > life) Destroy();
        }

        public override Dictionary<string, object> GetState()
        {
            var state = base.GetState();
            state.Add("transcendent", transcendent);
            state.Add("aiTimer", aiTimer);
            return state;
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            base.LoadState(state);

            transcendent = (bool)state["transcendent"];
            aiTimer = (int)state["aiTimer"];
        }
    }
}
