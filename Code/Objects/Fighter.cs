using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ChromaCore.Code.Effects;
using ChromaCore.Code.Scenes;
using static System.Formats.Asn1.AsnWriter;

namespace ChromaCore.Code.Objects
{
    public abstract class Fighter : GameObject
    {
        public enum States
        {
            Idle,
            Walk,
            Run,
            Backdash,
            Crouch,
            JumpSquat,
            AirIdle,
            AirStall,
            Hitstun,
            KnockDown,
            Attack,
            LandingLag,
            Guard,
            LowGuard
        }

        public enum HitstunProperties
        {
            Restand,
            Knockdown,
            Launcher,
            GroundBounce,
            ForceGroundBounce,
            WallBounce
        }

        public int ID;
        public Fighter nearestPlayer;

        public States state = States.Idle;
        public Attack attack;
        public int attackTimer = 0;
        public int jumpSquatTimer;
        public int jumpSquatType = 0;
        public int jumpSquatDir = 0;
        public int dashTimer = 0;
        public int dashCooldownTimer = 0;
        public int hitstunTimer;
        public int knockdownTimer;
        public int knockdownDuration;
        public int rollTimer;
        public int landingLag;
        public int blockStun;
        public int unblockableProtection;
        public HitTypes unblockableType = HitTypes.Normal;
        public int crossUpProtection;
        public int grabProtection;
        public bool hasHitPlayer = false;
        public bool hasHitBlock = false;
        public Fighter lastPlayerHit;
        public int Iframes;
        public int projImmuneFrames;
        public int airImmuneFrames;
        public int armorFrames;
        public int armorFlash;
        public bool hasDoubleJump = false;

        /// <summary>
        /// Right pressed minus left pressed
        /// </summary>
        public int inputDir
        {
            get
            {
                int d = 0;
                if (input.KeyDown(Controller.Key_Right)) d++;
                if (input.KeyDown(Controller.Key_Left)) d--;
                return d;
            }
        }
        public int GetMotionInputDirection => (nearestPlayer != null && position.X != nearestPlayer.position.X && Grounded) ? Math.Sign(nearestPlayer.position.X - position.X) : direction;

        public int comboCounter;
        public float comboScaling;
        public List<HitstunProperties> hitstunProperties = new List<HitstunProperties>();
        public bool airCombo;
        public bool launched;
        public bool wallBounced;
        public bool groundBounced;
        public List<Buff> buffs = new List<Buff>();

        public List<Collider> hurtboxes = new List<Collider>();

        public Collider idleHurtbox;
        public Collider crouchHurtbox;

        public Animation idleAnim;
        public Animation crouchAnim;
        public Animation unCrouchAnim;
        public Animation walkAnim;
        public Animation walkBackAnim;
        public Animation runAnim;
        public Animation backdashAnim;
        public Animation jumpSquatAnim;
        public Animation airAnim;
        public Animation jumpForwardAnim;
        public Animation jumpBackAnim;
        public Animation dashJumpAnim;
        public Animation airHurtAnim;
        public Animation groundHurtAnim;
        public Animation crouchHurtAnim;
        public Animation overheadHurtAnim;
        public Animation knockdownAnim;
        public Animation guardHighAnim;
        public Animation guardLowAnim;
        public Animation techForwardAnim;
        public Animation techBackAnim;
        public Animation introAnimation;

        public List<Animation> AirHurtAnims => new List<Animation>() { airHurtAnim }.Where(a => a != null).ToList();
        public List<Animation> GroundHurtAnims => new List<Animation>() { groundHurtAnim, crouchHurtAnim, overheadHurtAnim }.Where(a => a != null).ToList();
        public List<Animation> TechAnims => new List<Animation>() { techForwardAnim, techBackAnim }.Where(a => a != null).ToList();

        private List<Animation> basicAnims = new List<Animation>();
        private List<Animation> AttackAnims
        {
            get
            {
                List<Animation> l = new List<Animation>();
                foreach (Attack a in allAttacks) l.Add(a.anim);
                return l;
            }
        }
        protected List<Animation> Anims => basicAnims.Union(AttackAnims).ToList();

        /// <summary>
        /// Should contain all attacks the character will ever use
        /// </summary>
        public List<Attack> allAttacks;
        protected List<Attack> attacks = new List<Attack>();
        public List<Attack> Attacks => attacks;

        public Controller input;

        public float walkAccel;
        public float airAccel;
        public float walkSpeed;
        public float jumpSpeed;
        public float runSpeed;
        public int dashLength;
        public bool holdDash;
        public int dashCooldown;
        public float weight;

        public int health = 400;
        public int healthMax = 400;

        public bool dontDraw = false;
        public bool dontPushPlayer = false;

        public List<int> overrideHitboxGroups = new List<int>();

        public virtual Texture2D healthBarBack => Game.LoadAsset<Texture2D>("UI/BattleHUD/HealthBar_Back");
        public virtual Texture2D healthBarBar => Game.LoadAsset<Texture2D>("UI/BattleHUD/HealthBar_Bar");
        public virtual Texture2D healthBarFrame => null;

        public List<string> colorPalettes;
        public int currentPalette = 0;

        protected bool HoldingForward => (direction == 1 && input.KeyDown(Controller.Key_Right) && !input.KeyDown(Controller.Key_Left)) || (direction == -1 && input.KeyDown(Controller.Key_Left) && !input.KeyDown(Controller.Key_Right));
        protected bool HoldingBackward => (direction == -1 && input.KeyDown(Controller.Key_Right) && !input.KeyDown(Controller.Key_Left)) || (direction == 1 && input.KeyDown(Controller.Key_Left) && !input.KeyDown(Controller.Key_Right));

        public Fighter() : base() { }

        public void Spawn(int playerID, int controllerPort, ControlProfile controllerProfile, int colorPalette = 0)
        {
            BaseStats();
            health = healthMax;

            basicAnims.Add(idleAnim);
            basicAnims.Add(walkAnim);
            basicAnims.Add(walkBackAnim);
            basicAnims.Add(runAnim);
            basicAnims.Add(backdashAnim);
            basicAnims.Add(crouchAnim);
            basicAnims.Add(unCrouchAnim);
            basicAnims.Add(jumpSquatAnim);
            basicAnims.Add(airAnim);
            basicAnims.Add(jumpForwardAnim);
            basicAnims.Add(jumpBackAnim);
            basicAnims.Add(dashJumpAnim);
            basicAnims.Add(airHurtAnim);
            basicAnims.Add(groundHurtAnim);
            basicAnims.Add(crouchHurtAnim);
            basicAnims.Add(overheadHurtAnim);
            basicAnims.Add(knockdownAnim);
            basicAnims.Add(guardHighAnim);
            basicAnims.Add(guardLowAnim);
            basicAnims.Add(techForwardAnim);
            basicAnims.Add(techBackAnim);
            basicAnims.Add(introAnimation);

            if (allAttacks == null) allAttacks = attacks;

            animation = idleAnim;

            hurtboxes = new List<Collider>();
            hurtboxes.Add(idleHurtbox);

            PostInit();

            ID = playerID;
            input = new Controller(controllerPort);
            input.UpdateKeys(1);
            input.ClearAllBuffers();
            position = scene.room.spawn[ID % 2];
            CollisionCorrection();
            if (attacks.Count != 0) attack = attacks[0];

            if (controllerProfile != null)
            {
                for (int i = 0; i < controllerProfile.keyBinds.Length; i++) input.RebindKeyboard(DataManager.keyBindNames.ElementAt(i).Value, controllerProfile.keyBinds[i]);
                for (int i = 0; i < controllerProfile.padBinds.Length; i++) input.RebindController(DataManager.keyBindNames.ElementAt(i + 4).Value, controllerProfile.padBinds[i]);
            }

            if (colorPalettes != null && colorPalette >= colorPalettes.Count) colorPalette = 0;
            currentPalette = colorPalette;
            if (colorPalettes != null) LoadPalette(colorPalettes[0], colorPalettes[colorPalette]);
        }

        public virtual void BaseStats() { }
        public virtual void PostInit() { }

        public virtual void ResetStats()
        {
            walkAccel = 1;
            airAccel = 1;
            walkSpeed = 4;
            jumpSpeed = -16;
            runSpeed = 8;
            dashLength = 12;
            holdDash = true;
            dashCooldown = 12;
            weight = 1;
        }

        /// <summary>
        /// The main script run every frame for the player object. Be sure to include base.Update() when modifying
        /// </summary>
        public override void Update()
        {
            if (animation != null && animation.frames > 1)
            {
                animation.Update();
            }

            hurtboxes.Clear();
            hurtboxes.Add(idleHurtbox);

            foreach (Buff b in buffs.ToArray()) b.Update(this);

            bool unCrouch = unCrouchAnim != null && (state == States.AirIdle || state == States.Crouch || (state == States.Attack && attack.holdCrouch) || state == States.LandingLag) || animation == crouchHurtAnim || animation == overheadHurtAnim;

            if (!CommitedState && dashTimer == 0)
            {
                if (Grounded)
                {
                    if (input.KeyDown(Controller.Key_Down)) state = States.Crouch;
                    else state = States.Idle;
                }
                else state = States.AirIdle;
            }

            //Landing Lag
            if (state == States.AirStall && TouchingTile(collider.Bottom, 0, (int)velocity.Y + 1))
            {
                if (attack.landingLag == 0) state = States.Idle;
                else landingLag = attack.landingLag;
            }
            if (landingLag > 0)
            {
                landingLag--;
                state = States.LandingLag;
                animation = jumpSquatAnim;
                if (landingLag == 0)
                {
                    if (input.KeyDown(Controller.Key_Down)) state = States.Crouch;
                    else state = States.Idle;
                }
            }

            //Guard State
            if (blockStun > 0)
            {
                state = input.KeyDown(Controller.Key_Down) ? States.LowGuard : States.Guard;
                blockStun--;
                if (blockStun == 0)
                {
                    state = input.KeyDown(Controller.Key_Down) ? States.Crouch : States.Idle;
                    if (input.KeyDown(Controller.Key_Down)) crouchAnim.currentFrame = crouchAnim.frames;
                }
            }

            //Movement
            int moveDir = 0;
            if (!CommitedState && state != States.Crouch && state != States.Run)
            {
                if (input.KeyDown(Controller.Key_Right)) moveDir++;
                if (input.KeyDown(Controller.Key_Left)) moveDir--;
                if (moveDir != 0)
                {
                    if (Grounded && nearestPlayer == null) direction = moveDir;
                    if (state != States.Attack && Grounded) state = States.Walk;
                }
            }

            //Dashing
            int targetDir = 0;
            targetDir = nearestPlayer != null ? Math.Sign(nearestPlayer.position.X - position.X) : direction;
            if (dashCooldownTimer > 0 && state != States.Run && state != States.Backdash) dashCooldownTimer--;
            if (Grounded && !CommitedState && state != States.Crouch && input.KeyPressed(Controller.Key_Dash) && direction == targetDir && dashCooldownTimer <= 0 && dashLength != 0)
            {
                if (moveDir != 0 && moveDir == -direction) state = States.Backdash;
                else state = States.Run;
                dashTimer = state == States.Backdash ? (int)(dashLength * 1.75f) : dashLength;
                velocity.X = runSpeed * moveDir;
                dashCooldownTimer = dashCooldown;
                backdashAnim.Reset();
            }
            if (state == States.Run && dashTimer > 0)
            {
                dashTimer--;
                velocity.X = runSpeed * direction;
            }
            if (state == States.Run && dashTimer == 0 && holdDash)
            {
                if (input.KeyDown(Controller.Key_Dash) || (direction == -1 && input.KeyDown(Controller.Key_Left)) || (direction == 1 && input.KeyDown(Controller.Key_Right)))
                {
                    dashTimer = 1;
                }
            }
            if (state != States.Run && state != States.Backdash) dashTimer = 0;
            if (state == States.Backdash && dashTimer > 0)
            {
                dashTimer--;
                if (dashTimer > dashLength / 2) velocity.X = runSpeed * 2f * -direction * (dashTimer / (dashLength * 2f));
                else velocity.X = 0;
                if (dashTimer <= 0) state = States.Idle;
            }
            if (!Grounded) dashTimer = 0;

            //X friction
            if (!Grounded)
            {
                velocity.X = Approach(velocity.X, walkSpeed * moveDir, airAccel);
            }
            else if (state != States.Run)
            {
                velocity.X = Approach(velocity.X, walkSpeed * (moveDir != 0 && moveDir == -direction ? moveDir * 0.5f : moveDir), walkAccel);
            }

            //Jumping
            if (input.KeyPressed(Controller.Key_Up) && (Grounded || hasDoubleJump) && !(CommitedState && state != States.Attack && state != States.Run) && !(state == States.Attack && (!attack.jumpCancelable || !hasHitPlayer)))
            {
                if (state == States.Run) jumpSquatType = 1;
                else jumpSquatType = 0;
                jumpSquatDir = 0;
                if (input.KeyDown(Controller.Key_Right)) jumpSquatDir++;
                if (input.KeyDown(Controller.Key_Left)) jumpSquatDir--;
                state = States.JumpSquat;
                if (!Grounded)
                {
                    jumpSquatTimer = 6;
                    hasDoubleJump = false;
                }
            }
            if (state == States.JumpSquat)
            {
                jumpSquatTimer++;
                velocity.X *= 0.8f;
                if (jumpSquatDir == 0)
                {
                    if (input.KeyDown(Controller.Key_Right)) jumpSquatDir++;
                    if (input.KeyDown(Controller.Key_Left)) jumpSquatDir--;
                }
                if (jumpSquatTimer >= 6)
                {
                    state = States.AirIdle;
                    velocity.Y = input.KeyDown(Controller.Key_Up) ? jumpSpeed : jumpSpeed * 0.75f;
                    if (jumpSquatType == 1) velocity.X = direction * runSpeed;
                    else velocity.X = jumpSquatDir * walkSpeed;
                    if (jumpSquatType == 1 && dashJumpAnim != null) animation = dashJumpAnim;
                }
            }
            else jumpSquatTimer = 0;

            //Get nearest player
            nearestPlayer = null;
            foreach (Fighter p in scene.players)
            {
                if (p != this)
                {
                    if (nearestPlayer == null) nearestPlayer = p;
                    else if (Math.Abs(p.position.X - position.X) < Math.Abs(nearestPlayer.position.X - position.X)) nearestPlayer = p;
                }
            }
            if (!CommitedState && Grounded && state != States.Run && nearestPlayer != null) direction = Math.Sign(nearestPlayer.position.X - position.X);
            if (direction == 0) direction = 1;

            //Attack inputs
            if (input.KeyPressed(Controller.Key_Light) || input.KeyPressed(Controller.Key_Medium) || input.KeyPressed(Controller.Key_Heavy) || input.KeyPressed(Controller.Key_Grab))
            {
                Attack attackFound = null;
                foreach (Attack a in attacks)
                {
                    int dir = (nearestPlayer != null && position.X != nearestPlayer.position.X && Grounded) ? Math.Sign(nearestPlayer.position.X - position.X) : direction;
                    if ((a.canUse == null || a.canUse(a)) && a.input != null && (!CommitedState || (state == States.Attack && (hasHitPlayer || hasHitBlock || (attackTimer <= 3 && attack?.input != null && attack.input.Button != a.input.Button)) && attack.canCancel && a.cancelLevel > attack.cancelLevel)) && a.input.CheckInput(input, dir))
                    {
                        if (a.groundedness == 0 || (a.groundedness == 1 && Grounded && velocity.Y >= 0) || (a.groundedness == 2 && !Grounded))
                        {
                            if (attackFound == null) attackFound = a;
                            else if (a.input.priority > attackFound.input.priority) attackFound = a;
                        }
                    }
                }

                if (attackFound != null)
                {
                    SetAttack(attackFound);
                }
            }

            if ((state == States.Crouch || (state == States.LowGuard)) && crouchHurtbox != null || unCrouch) hurtboxes[0] = crouchHurtbox;
            if (state == States.Attack && attack.primaryHurtbox != null) hurtboxes[0] = attack.primaryHurtbox;

            //Attacks
            if (state == States.Attack)
            {
                attack.attackUpdate?.Invoke(attack);
                AttackUpdate();
            }

            if (state == States.Attack && hitstunTimer <= 0)
            {
                animation = attack.anim;
                attackTimer++;
                if (attackTimer > attack.duration && attack.duration != -1)
                {
                    if (attack.airStall && !Grounded) state = States.AirStall;
                    else
                    {
                        if (Grounded)
                        {
                            EndState();
                        }
                        else state = States.AirIdle;
                    }
                }
                if (attack.forceGroundedness && attack.groundedness == 2 && Grounded)
                {
                    if (attack.landingLag == 0) state = States.Idle;
                    else
                    {
                        landingLag = attack.landingLag;
                        state = States.LandingLag;
                    }
                    DestroyHitboxes();
                }
                if (attack.forceGroundedness && attack.groundedness == 1 && !Grounded)
                {
                    state = States.AirIdle;
                    DestroyHitboxes();
                }
                if (attack.hitboxes != null)
                {
                    foreach (HitboxSpawner h in attack.hitboxes)
                    {
                        if (attackTimer == h.creationFrame)
                        {
                            if (h.parent >= 0 && h.parent < attack.hitboxes.Length)
                            {
                                attack.hitboxes[h.parent].SpawnHitBox(h.size, h.offset);
                            }
                            else h.SpawnHitBox();
                        }
                    }
                }

                if (attack.pushers != null)
                {
                    foreach (Pusher p in attack.pushers)
                    {
                        if (attackTimer == p.frame)
                        {
                            p.Push(this);
                        }
                    }
                }
            }
            else
            {
                attackTimer = 0;
                hasHitPlayer = false;
                hasHitBlock = false;
            }

            //Hitstun and combos
            if (hitstunTimer > 0)
            {
                hitstunTimer--;
                state = States.Hitstun;
                if (!Grounded)
                {
                    if (!hitstunProperties.Contains(HitstunProperties.Restand)) hitstunTimer = 1;
                    airCombo = true;
                    if (hitstunProperties.Contains(HitstunProperties.Launcher))
                    {
                        if (velocity.Y < -4) velocity.Y += gravity * 0.5f;
                        if (velocity.Y >= -4 && velocity.Y < 4)
                        {
                            velocity.Y -= gravity * 0.5f;
                            velocity.X = Approach(velocity.X, 0, 0.2f);
                        }
                    }
                }
                knockdownTimer = 0;
                if (hitstunProperties.Contains(HitstunProperties.WallBounce))
                {
                    if (!wallBounced &&
                        ((TouchingTile(collider.Box, (int)(velocity.X * 1.1f), 0) || TouchingTile(collider.Right, 1, 0) || TouchingTile(collider.Left, -1, 0) ||
                        (position.X + velocity.X <= scene.camera.position.X + 24 || position.X + velocity.X > scene.camera.position.X + scene.camera.CamWidth - 24))))
                    {
                        velocity.X *= -1;
                        position.X += velocity.X * 0.5f;
                        velocity.X = Math.Clamp(Math.Abs(velocity.X * 0.25f), 1, 5) * (velocity.X != 0 ? Math.Sign(velocity.X) : direction * -1);
                        velocity.Y = Math.Min(-14, velocity.Y);
                        wallBounced = true;
                        hitstunProperties.Remove(HitstunProperties.WallBounce);
                    }
                }

                if (airCombo && TouchingTile(collider.Bottom, 0, (int)(velocity.Y + gravity + 1)))
                {
                    if ((!groundBounced && hitstunProperties.Contains(HitstunProperties.GroundBounce)) || hitstunProperties.Contains(HitstunProperties.ForceGroundBounce))
                    {
                        //Visual stuff
                        int y = 0;
                        while (!TouchingTile(collider.Bottom, 0, y)) y++;
                        //new Particle(new Animation("Characters/Common/GroundBounceParticle", 4, 4, new Vector2(39), false), position + new Vector2(velocity.X, y), 16, Vector2.Zero, 1, 0, direction);
                        scene.camera.ShakeCamera(velocity.Y / 2, 4);

                        position.Y += velocity.Y;
                        CollisionCorrection();
                        velocity.Y *= -0.5f;

                        if (hitstunProperties.Contains(HitstunProperties.GroundBounce)) groundBounced = true;
                        hitstunProperties.Remove(HitstunProperties.ForceGroundBounce);
                        hitstunProperties.Remove(HitstunProperties.GroundBounce);
                    }
                    else
                    if (TouchingTile(collider.Bottom, 0, (int)(velocity.Y + gravity + 1)))
                    {
                        hitstunTimer = 0;
                        if (hitstunProperties.Contains(HitstunProperties.Restand)) airCombo = false;
                        else if (hitstunProperties.Contains(HitstunProperties.Knockdown))
                        {
                            knockdownTimer = knockdownDuration;
                            animation = knockdownAnim;
                        }
                        else
                        {
                            knockdownTimer = 20;

                            int xDir = 0;
                            if (input.KeyDown(Controller.Key_Right)) xDir++;
                            if (input.KeyDown(Controller.Key_Left)) xDir--;
                            velocity.X = runSpeed * 2 * xDir;

                            if (velocity.X != 0)
                            {
                                if (Math.Sign(velocity.X) == direction)
                                {
                                    animation = techForwardAnim;
                                }
                                else
                                {
                                    animation = techBackAnim;
                                }
                                int totalRollTime = animation.frames * animation.frameRate;
                                if (totalRollTime > knockdownTimer)
                                {
                                    animation.currentFrame = animation.frames - (knockdownTimer / animation.frameRate);
                                }
                                rollTimer = knockdownTimer;
                            }
                            else animation = knockdownAnim;
                        }
                    }
                }
            }
            else if (knockdownTimer < 10)
            {
                comboCounter = 0;
                comboScaling = 0;
                airCombo = false;
                launched = false;
                wallBounced = false;
                groundBounced = false;
                if (scene is InGameTraining && knockdownTimer < 5) health = healthMax;
            }
            if (Grounded) airCombo = false;

            //Knockdown
            if (knockdownTimer > 0)
            {
                knockdownTimer--;
                state = States.KnockDown;
                if (velocity.X == 0 && rollTimer <= 0) animation = knockdownAnim;
                if (knockdownTimer == 0)
                {
                    if (nearestPlayer != null) direction = Math.Sign(nearestPlayer.position.X - position.X);
                    if (direction == 0) direction = 1;
                    state = input.KeyDown(Controller.Key_Down) ? States.Crouch : States.Idle;
                    rollTimer = 0;
                }
            }
            if (state != States.KnockDown) knockdownTimer = 0;

            //Physics
            UpdateGravity();

            if (!dontPushPlayer) PlayerCollision();
            dontPushPlayer = false;

            UpdatePosition();

            bool landed = false;
            if (state == States.AirIdle && Grounded)
            {
                state = States.Idle;
                landed = true;
            }

            if (state == States.Idle)
            {
                if (landed) animation = jumpSquatAnim;
                else if (unCrouchAnim != null && (unCrouch || (animation == unCrouchAnim && unCrouchAnim.currentFrame < unCrouchAnim.frames) || animation == jumpSquatAnim))
                {
                    animation = unCrouchAnim;
                }
                else animation = idleAnim;
            }
            if (state == States.Walk)
            {
                if (direction == -moveDir) animation = walkBackAnim;
                else animation = walkAnim;
            }
            if (state == States.Run) animation = runAnim;
            if (state == States.Backdash) animation = backdashAnim;
            if (state == States.Crouch)
            {
                if (unCrouch && crouchAnim != null && animation != crouchAnim)
                {
                    crouchAnim.timer = crouchAnim.currentFrame = crouchAnim.frames;
                }
                animation = crouchAnim;
            }
            if (state == States.JumpSquat) animation = jumpSquatAnim;
            if (state == States.AirIdle || state == States.AirStall)
            {
                if (animation != dashJumpAnim || animation.IsDone)
                {
                    animation = airAnim;
                    if (Math.Abs(velocity.X) > 1)
                    {
                        if (Math.Sign(velocity.X) == direction && jumpForwardAnim != null) animation = jumpForwardAnim;
                        if (Math.Sign(velocity.X) == -direction && jumpBackAnim != null) animation = jumpBackAnim;
                    }

                    animation.currentFrame = (int)Math.Ceiling((Math.Clamp(velocity.Y, jumpSpeed / 1.5f, fallSpeed / 1.5f) - jumpSpeed / 1.5f) / ((fallSpeed - jumpSpeed) / 1.5f) * animation.frames);
                    if (animation.currentFrame < 1) animation.currentFrame = 1;
                    if (animation.currentFrame > animation.frames) animation.currentFrame = animation.frames;
                }
                if (Grounded) animation = idleAnim;
            }
            if (state == States.Guard)
            {
                animation = guardHighAnim;
                if (animation.frames > 1 && hitstunTimer >= animation.frameRate * animation.frames)
                {
                    animation.Reset();
                }
            }
            if (state == States.LowGuard)
            {
                animation = guardLowAnim;
                if (animation.frames > 1 && hitstunTimer >= animation.frameRate * animation.frames)
                {
                    animation.Reset();
                }
            }
            if (state == States.Hitstun)
            {
                if (!Grounded)
                {
                    if (!AirHurtAnims.Contains(animation)) animation = airHurtAnim;

                    else if (hitstunProperties.Contains(HitstunProperties.Restand) || velocity.Y < 1)
                    {
                        animation.Reset();
                    }
                    else if (velocity.Y >= fallSpeed)
                    {
                        animation.currentFrame = animation.frames;
                        animation.timer = 0;
                    }
                }
                else
                {
                    if (!GroundHurtAnims.Contains(animation)) animation = groundHurtAnim;
                    if (animation.frames > 1 && hitstunTimer >= animation.frameRate * animation.frames)
                    {
                        animation.Reset();
                    }
                }
            }
            if (state == States.KnockDown)
            {
                int getUpTime = knockdownAnim.frameRate * knockdownAnim.frames;
                if (rollTimer <= 0)
                {
                    animation = knockdownAnim;
                    if (knockdownTimer > getUpTime)
                    {
                        animation.timer = 0;
                        animation.currentFrame = 1;
                    }
                }
                else if (velocity.X != 0 && !TechAnims.Contains(animation)) animation = Math.Sign(velocity.X) == direction ? techForwardAnim : techBackAnim;
            }

            if (state != States.Attack) DestroyHitboxes(false);

            if (hitstunTimer > 0)
            {
                armorFrames = 0;
                armorFlash = 0;
            }
            if (Iframes > 0) Iframes--;
            if (projImmuneFrames > 0) projImmuneFrames--;
            if (armorFrames > 0) armorFrames--;
            if (armorFlash > 0) armorFlash--;
            if (unblockableProtection > 0) unblockableProtection--;
            if (crossUpProtection > 0) crossUpProtection--;
            if (blockStun > 0) grabProtection = 8;
            if (grabProtection > 0 && hitstunTimer <= 0) grabProtection--;

            if (knockdownTimer > 0 || blockStun > 0) grabProtection = 10;

            UpdateHurtboxes();

            ResetStats();

            if (animation != null && animation.frames > 1)
            {
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

            foreach (Animation a in Anims) if (a != animation) a?.Reset();
        }

        public virtual void SetAttack(Attack att, bool updateDirection = true, bool resetHitCheck = true)
        {
            if (att != null)
            {
                if (attack != null) Animation.Reset(attack.anim);
                animation = att.anim;
                Animation.Reset(animation);
                state = States.Attack;
                attack = att;
                attackTimer = 0;
                DestroyHitboxes();
                overrideHitboxGroups.Clear();
                if (resetHitCheck) hasHitPlayer = false;
                if (resetHitCheck) hasHitBlock = false;
                if (att.input != null) input.ClearBuffer(att.input.Button);
                if (Grounded && scene.players.Count > 1 && updateDirection)
                {
                    if (nearestPlayer != null) direction = Math.Sign(nearestPlayer.position.X - position.X);
                    if (direction == 0) direction = 1;
                }
            }
        }

        public virtual void SetAttack(string att, bool updateDirection = true, bool resetHitCheck = true) => SetAttack(GetAttackFromName(att), updateDirection, resetHitCheck);

        public virtual void EndState()
        {
            if (!Grounded)
            {
                state = States.AirIdle;
            }
            else if (input.KeyDown(Controller.Key_Down)) state = States.Crouch;
            else if (input.KeyDown(Controller.Key_Left) ^ input.KeyDown(Controller.Key_Right)) state = States.Walk;
            else state = States.Idle;

            if (Grounded && state != States.Run && nearestPlayer != null) direction = Math.Sign(nearestPlayer.position.X - position.X);
            if (direction == 0) direction = 1;
        }

        public bool CommitedState => state == States.JumpSquat || state == States.Attack || hitstunTimer > 0 || state == States.AirStall || state == States.KnockDown || state == States.LandingLag || state == States.Guard || dashTimer > 1 || rollTimer > 0 || blockStun > 0;

        public virtual void AttackUpdate() { }

        public virtual void HitPlayer(Hitbox hitbox, Fighter target, ref int damage)
        {
            if (hitbox.allowCancel) hasHitPlayer = true;
        }

        public virtual void OnHurt(Hitbox hitbox, Fighter enemy) { }

        /// <summary>
        /// Whether or not you got hit
        /// </summary>
        /// <param name="hitbox">the hitbox that hit you</param>
        /// <param name="counterHitType"><para>The type of counter hit that is applied</para>
        /// <para>-1 = no counter hit</para>
        /// <para>0 = normal counter hit</para>
        /// <para>1 = photon shift counter hit</para>
        /// <para></para></param>
        /// <returns></returns>
        public virtual bool GotHit(Hitbox hitbox, out int counterHitType)
        {
            counterHitType = -1;
            if (hitbox.hitType.ToString().Contains("Grab") && grabProtection > 0) return false;
            if (Grounded && hitbox.hitType == HitTypes.AirGrab) return false;
            if ((state == States.KnockDown && !(knockdownTimer > 20 && hitbox.canOTG)) || Iframes > 0) return false;
            if (hitbox.hitType == HitTypes.Grab && ((!Grounded || jumpSquatTimer > 0) && hitstunTimer <= 0)) return false;
            if (hitbox is Projectile && projImmuneFrames > 0) return false;

            //Blocking
            if (Grounded && (!CommitedState || blockStun > 0) && ((hitbox.flipCrossUp ? HoldingForward : HoldingBackward) || crossUpProtection > 0) && !hitbox.hitType.ToString().Contains("Grab"))
            {
                bool blocked = true;
                if (hitbox.hitType == HitTypes.Low && !input.KeyDown(Controller.Key_Down)) blocked = false;
                if (hitbox.hitType == HitTypes.Overhead && input.KeyDown(Controller.Key_Down)) blocked = false;

                if (blocked || (unblockableProtection > 0 && unblockableType != hitbox.hitType))
                {
                    velocity.X = 0;
                    health = Math.Max(health - hitbox.chipDamage, 1);
                    if (hitbox.hitSound != null) scene.PlayWorldSound(Game.LoadAsset<SoundEffect>("Sounds/Characters/Common/sfx_hitBlocked"), position, 0.25f);
                    if (hitbox.hitParticle != null)
                    {
                        Vector2 prtPos = new Vector2(direction == 1 ? (collider.Box.Right - 8) : (collider.Box.Left + 8), position.Y + (hitbox.hitType == HitTypes.Overhead ? -40 : hitbox.hitType == HitTypes.Low ? 48 : -8));
                        GenericParticles.blockHit.Spawn(prtPos, direction, 0);
                    }

                    scene.ApplyHitpause(hitbox.pause + 1);
                    scene.camera.ShakeCamera(hitbox.dmg / 2, 4);
                    blockStun = hitbox.blockstun;
                    velocity.X += hitbox.blockPush * hitbox.direction;
                    if (!(hitbox is Projectile)) hitbox.owner.velocity.X += hitbox.blockPush * Math.Sign(hitbox.owner.position.X - position.X) / 2;
                    hitbox.owner.hasHitBlock = true;
                    if (blocked && hitbox.hitType != HitTypes.Normal)
                    {
                        unblockableProtection = 16;
                        unblockableType = hitbox.hitType;
                    }
                    if (direction == Math.Sign(hitbox.owner.position.X - position.X)) crossUpProtection = 20;
                    if (hitbox.onBlockHitEffect != null) hitbox.onBlockHitEffect(hitbox, this);

                    hitbox.hitObjects.Add(this);
                    hitbox.owner.overrideHitboxGroups.Add(hitbox.group);

                    animation = input.KeyDown(Controller.Key_Down) ? guardLowAnim : guardHighAnim;

                    return false;
                }
            }

            //Throw teching
            if (hitbox.hitType == HitTypes.Grab && state == States.Attack && attack.name == "Grab" && hitbox.owner.state == States.Attack && hitbox.owner.attack.name == "Grab")
            {
                scene.PlayWorldSound(Game.LoadAsset<SoundEffect>("Sounds/Characters/Common/sfx_hitBlocked"), position, 0.25f);
                Vector2 prtPos = (position + hitbox.owner.position) / 2 + new Vector2(0, -16);
                GenericParticles.throwTech.Spawn(prtPos, direction);
                state = States.Backdash;
                dashTimer = 16;
                hitstunTimer = 0;
                DestroyHitboxes();
                hitbox.owner.state = States.Backdash;
                hitbox.owner.dashTimer = 16;
                hitbox.owner.hitstunTimer = 0;
                hitbox.owner.DestroyHitboxes();
                return false;
            }

            //Prevent multiple grabs in a combo
            if (hitbox.hitType.ToString().Contains("Grab")) grabProtection = 2;

            //Cleaning up variables
            if ((state == States.Attack || state == States.AirStall || state == States.LandingLag || (scene is InGameTraining t2 && t2.forceCounterHit == InGameTraining.CounterHitSetting.On && state != States.Hitstun)) && !hitbox.hitType.ToString().Contains("Grab"))
            {
                counterHitType = 0;
                comboScaling = -4;
            }
            dashTimer = 0;
            attackTimer = 0;
            jumpSquatTimer = 0;
            rollTimer = 0;
            blockStun = 0;

            hitbox.owner.lastPlayerHit = this;

            //Extra Stuff
            if (scene is InGameTraining tr) tr.startupDisplay = hitbox.owner.attackTimer;
            return true;
        }

        public void PlayerCollision()
        {
            foreach (GameObject obj in scene.players)
            {
                if (obj != this && TouchingObject(collider.Box, obj))
                {
                    if (Math.Sign(obj.position.X - position.X) == Math.Sign(velocity.X) && position.X != obj.position.X)
                    {
                        if (Math.Sign(obj.position.X - position.X) == -1 && obj.TouchingTile(obj.collider.Left, -2, 0))
                        {
                            position.X -= velocity.X;
                        }
                        else if (Math.Sign(obj.position.X - position.X) == 1 && obj.TouchingTile(obj.collider.Right, 2, 0))
                        {
                            position.X -= velocity.X;
                        }
                        else
                        {
                            float pushVal = (Math.Abs(velocity.X) / 2) * (weight / ((Fighter)obj).weight);
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
                    if (velocity.X == 0 && obj.velocity.X == 0)
                    {
                        position.X -= 4 * Math.Sign(obj.position.X - position.X);
                    }

                    CollisionCorrection();
                    obj.CollisionCorrection();
                }
            }
        }

        protected float Approach(float start, float end, float interval)
        {
            if (Math.Abs(start - end) <= interval) return end;
            if (start < end)
                return MathF.Min(start + interval, end);
            else
                return MathF.Max(start - interval, end);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!dontDraw)
            {
                Color baseColor = ID == 1 && (scene.players[0].GetType() == GetType() && scene.players[0].currentPalette == currentPalette) ? new Color(160, 140, 80) : Color.White;
                animation?.Draw(spriteBatch, position, direction, baseColor, drawLayer, rotation, drawScale);

                if (scene is InGameTraining t && t.displayHitboxes && collider != null)
                {
                    Texture2D rect = Game.LoadTexture("ExampleContent/Pixel");
                    spriteBatch.Draw(rect, new Rectangle(new Point((int)collider.Box.X, (int)collider.Box.Y), new Point((int)collider.Box.Width, (int)collider.Box.Height)), null,
                        new Color(Color.Lime, 0.5f), 0, Vector2.Zero, SpriteEffects.None, Math.Max(0.81f, drawLayer + 0.01f));
                }

                if (armorFlash > 0) animation.DrawSilhouette(spriteBatch, position, drawLayer + 0.000001f, direction, new Color(Color.Red, 0.25f), rotation, drawScale);
                foreach (Buff b in buffs.ToArray()) b.onDraw?.Invoke(spriteBatch, b, this);
            }
            dontDraw = false;

            foreach (Collider hurtbox in hurtboxes)
            {
                if (scene != null && scene is InGameTraining t && t.displayHitboxes && hurtbox != null)
                {
                    Texture2D rect = Game.LoadTexture("ExampleContent/Pixel");
                    spriteBatch.Draw(rect, new Rectangle(new Point((int)hurtbox.Box.X, (int)hurtbox.Box.Y), new Point((int)hurtbox.Box.Width, (int)hurtbox.Box.Height)), null,
                        new Color(Color.Blue, 0.5f), 0, Vector2.Zero, SpriteEffects.None, Math.Max(0.82f, drawLayer + 0.02f));
                }
            }
        }

        public bool HasBuff(string tag) => buffs.Exists(b => b.identifier == tag);

        public void RemoveBuff(string tag)
        {
            foreach (Buff b in buffs.ToArray()) if (b.identifier == tag) buffs.Remove(b);
        }

        /// <summary>
        /// Populates the colorPalettes list based on a root directory and index numbers
        /// </summary>
        /// <param name="baseDirectory">Directory to the palettes without their ID</param>
        /// <param name="count">Number of palettes</param>
        protected void AutoGeneratePalettes(string baseDirectory, int count)
        {
            colorPalettes = new List<string>();
            for (int i = 0; i < count; i++) colorPalettes.Add(baseDirectory + i.ToString());
        }

        protected virtual void LoadPalette(string basePaletteFile, string newPaletteFile)
        {
            Texture2D basePalette = Game.LoadAsset<Texture2D>(basePaletteFile);
            Texture2D newPalette = Game.LoadAsset<Texture2D>(newPaletteFile);

            Color[] baseColors = new Color[basePalette.Width];
            Color[] newColors = new Color[newPalette.Width];
            basePalette.GetData(baseColors);
            newPalette.GetData(newColors);

            List<Animation> allAnims = new List<Animation>(Anims);
            foreach (Animation a in Anims) if (a != null && a.particles != null) foreach (AnimationParticle prt in a.particles) allAnims.Add(prt.ParticleAnimation);

            foreach (Animation a in allAnims.Where(anim => anim != null))
            {
                Texture2D spriteSheet = new Texture2D(game.GraphicsDevice, a.spriteSheet.Width, a.spriteSheet.Height);

                Color[] c = new Color[spriteSheet.Width * spriteSheet.Height];
                a.spriteSheet.GetData(c);

                for (int i = 0; i < c.Length; i++)
                {
                    if (baseColors.Contains(c[i])) c[i] = newColors[baseColors.IndexWhere(color => color == c[i])];
                }

                spriteSheet.SetData(c);
                a.spriteSheet = spriteSheet;
            }
        }

        protected void MoveToBack()
        {
            drawLayer = 0.499f;
            scene.supressPlayerLayering = true;
        }
        protected void MoveToFront()
        {
            drawLayer = 0.511f;
            scene.supressPlayerLayering = true;
        }

        public Attack GetAttackFromName(string name)
        {
            foreach (Attack a in allAttacks)
            {
                if (a.name == name) return a;
            }
            return null;
        }

        /// <summary>
        /// <para>Contains a template for how grabs should work</para>
        /// <para>Relies on having the attack names: "Grab", "Grab Miss", "Grab Release", "Forward Throw", and "Back Throw"</para>
        /// </summary>
        /// <param name="grabbedCheckTime">The frame when the attack will transition into Grab Miss if it hasn't hit</param>
        /// <param name="holdDuration">The number of frames to hold the target before transitioning to Grab Release</param>
        protected void BasicGrabUpdate(int grabbedCheckTime, int holdDuration, Vector2 holdPosition = default)
        {
            if (attack.name == "Grab")
            {
                if (!hasHitPlayer && attackTimer >= grabbedCheckTime) SetAttack(GetAttackFromName("Grab Miss"), false);

                if (hasHitPlayer && attackTimer >= grabbedCheckTime)
                {
                    if (lastPlayerHit.TouchingTile(lastPlayerHit.collider.Box, direction * 2, 0)) position.X -= 8 * direction;
                    dontPushPlayer = true;
                    //Subtracting ID to fix update order
                    lastPlayerHit.hitstunTimer = (int)Math.Clamp(32 - lastPlayerHit.comboScaling * 1.5f, 18, 24) - ID;
                    lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, position + (holdPosition != default ? holdPosition : new Vector2(64 * direction, -24)), 0.75f);
                    lastPlayerHit.velocity.X = 0;
                    lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                    lastPlayerHit.direction = -direction;
                    lastPlayerHit.CollisionCorrection();

                    int inputDir = 0;
                    if (input.KeyPressed(Controller.Key_Left)) inputDir--;
                    if (input.KeyPressed(Controller.Key_Right)) inputDir++;
                    if (inputDir == direction) SetAttack(GetAttackFromName("Forward Throw"));
                    if (inputDir == -direction)
                    {
                        SetAttack(GetAttackFromName("Back Throw"));
                        direction *= -1;
                    }

                    if (attackTimer >= grabbedCheckTime + holdDuration - lastPlayerHit.comboScaling * 2)
                    {
                        lastPlayerHit.velocity = new Vector2(6 * direction, -6);
                        SetAttack(GetAttackFromName("Grab Release"));
                        lastPlayerHit.hitstunProperties.Add(HitstunProperties.Restand);
                    }
                }
            }
        }

        public virtual void UpdateHurtboxes() { }

        protected void AddHurtbox(int width, int height, int xOffset, int yOffset)
        {
            hurtboxes.Add(new Collider(this, width, height, new Vector2(width / -2, height / -2 + yOffset), xOffset));
        }

        public override Dictionary<string, object> GetState()
        {
            var state = base.GetState();
            state.Add("nearestPlayer", nearestPlayer);
            state.Add("state", this.state);
            state.Add("attack", attack);
            state.Add("attackTimer", attackTimer);
            state.Add("jumpSquatTimer", jumpSquatTimer);
            state.Add("jumpSquatType", jumpSquatType);
            state.Add("jumpSquatDir", jumpSquatDir);
            state.Add("dashTimer", dashTimer);
            state.Add("dashCooldownTimer", dashCooldownTimer);
            state.Add("hitstunTimer", hitstunTimer);
            state.Add("knockdownTimer", knockdownTimer);
            state.Add("knockdownDuration", knockdownDuration);
            state.Add("rollTimer", rollTimer);
            state.Add("landingLag", landingLag);
            state.Add("blockStun", blockStun);
            state.Add("unblockableProtection", unblockableProtection);
            state.Add("unblockableType", unblockableType);
            state.Add("crossUpProtection", crossUpProtection);
            state.Add("grabProtection", grabProtection);
            state.Add("hasHitPlayer", hasHitPlayer);
            state.Add("hasHitBlock", hasHitBlock);
            state.Add("lastPlayerHit", lastPlayerHit);
            state.Add("iFrames", Iframes);
            state.Add("projImmuneFrames", projImmuneFrames);
            state.Add("airImmuneFrames", airImmuneFrames);
            state.Add("armorFrames", armorFrames);
            state.Add("hasDoubleJump", hasDoubleJump);
            state.Add("comboCounter", comboCounter);
            state.Add("comboScaling", comboScaling);
            state.Add("hitstunProperties", new List<HitstunProperties>(hitstunProperties));
            state.Add("airCombo", airCombo);
            state.Add("launched", launched);
            state.Add("wallBounced", wallBounced);
            state.Add("groundBounced", groundBounced);
            state.Add("buffs", new List<Buff>(buffs));
            List<int> buffTimes = new List<int>();
            foreach (Buff buff in buffs) buffTimes.Add(buff.duration);
            state.Add("buffTimes", buffTimes);
            state.Add("hurtboxes", new List<Collider>(hurtboxes));
            state.Add("health", health);
            state.Add("dontDraw", dontDraw);
            state.Add("dontPushPlayer", dontPushPlayer);
            state.Add("overrideHitboxGroups", new List<int>(overrideHitboxGroups));
            state.Add("input", input.ToBytes());
            return state;
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            base.LoadState(state);
            nearestPlayer = (Fighter)state["nearestPlayer"];
            this.state = (States)state["state"];
            attack = (Attack)state["attack"];
            attackTimer = (int)state["attackTimer"];
            jumpSquatTimer = (int)state["jumpSquatTimer"];
            jumpSquatType = (int)state["jumpSquatType"];
            jumpSquatDir = (int)state["jumpSquatDir"];
            dashTimer = (int)state["dashTimer"];
            dashCooldownTimer = (int)state["dashCooldownTimer"];
            hitstunTimer = (int)state["hitstunTimer"];
            knockdownTimer = (int)state["knockdownTimer"];
            knockdownDuration = (int)state["knockdownDuration"];
            rollTimer = (int)state["rollTimer"];
            landingLag = (int)state["landingLag"];
            blockStun = (int)state["blockStun"];
            unblockableProtection = (int)state["unblockableProtection"];
            unblockableType = (HitTypes)state["unblockableType"];
            crossUpProtection = (int)state["crossUpProtection"];
            grabProtection = (int)state["grabProtection"];
            hasHitPlayer = (bool)state["hasHitPlayer"];
            hasHitBlock = (bool)state["hasHitBlock"];
            lastPlayerHit = (Fighter)state["lastPlayerHit"];
            Iframes = (int)state["iFrames"];
            projImmuneFrames = (int)state["projImmuneFrames"];
            airImmuneFrames = (int)state["airImmuneFrames"];
            armorFrames = (int)state["armorFrames"];
            hasDoubleJump = (bool)state["hasDoubleJump"];
            comboCounter = (int)state["comboCounter"];
            comboScaling = (float)state["comboScaling"];
            hitstunProperties = new List<HitstunProperties>((List<HitstunProperties>)state["hitstunProperties"]);
            airCombo = (bool)state["airCombo"];
            launched = (bool)state["launched"];
            wallBounced = (bool)state["wallBounced"];
            groundBounced = (bool)state["groundBounced"];
            buffs = new List<Buff>((List<Buff>)state["buffs"]);
            for (int i = 0; i < ((List<int>)state["buffTimes"]).Count; i++)
                buffs[i].duration = ((List<int>)state["buffTimes"])[i];
            hurtboxes = new List<Collider>((List<Collider>)state["hurtboxes"]);
            health = (int)state["health"];
            dontDraw = (bool)state["dontDraw"];
            dontPushPlayer = (bool)state["dontPushPlayer"];
            overrideHitboxGroups = new List<int>((List<int>)state["overrideHitboxGroups"]);
            input.ReadBytes((byte[])state["input"]);
        }

        protected void RestandOnHitIfGrounded(Hitbox hitbox, Fighter target)
        {
            if (target.Grounded) target.hitstunProperties.Add(HitstunProperties.Restand);
        }
    }
}
