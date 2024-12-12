using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using ChromaCore.Code.Scenes;
using ChromaCore.Code.Effects;

namespace ChromaCore.Code.Objects.Players.Characters
{
    public class Jet : Fighter
    {
        public int uppercutCharge = 0;
        public int heatMaxCharges = 3;
        public int heatCharges = 3;
        public int heatCooldown = 0;

        int fireParticleTimer = 0;

        public static List<string> heatSettings = new List<string>()
        {
            "Normal",
            "Infinite",
            "No Cooldown",
            "Burnt Out"
        };

        public override void ResetStats()
        {
            base.ResetStats();

            walkAccel = 1f;
            airAccel = 0f;
            walkSpeed = 4f;
            jumpSpeed = -16;
            gravity = 0.75f;
            fallSpeed = 12;

            runSpeed = 8;
            dashLength = 10;
            holdDash = true;
            dashCooldown = 8;
        }

        public override void BaseStats()
        {
            ResetStats();

            AutoGeneratePalettes("Characters/Jet/Palette", 6);

            healthMax = 350;

            collider = new Collider(this, 48, 80, new Vector2(-24, -12));
            idleHurtbox = new Collider(this, 80, 112, new Vector2(-40, -44));
            crouchHurtbox = new Collider(this, 72, 88, new Vector2(-36, -20));

            idleAnim = new Animation("Characters/Jet/Idle", 5, 10, new Vector2(48));
            crouchAnim = new CompoundAnimation("Characters/Jet/Crouch", 1, 4, 12, new Vector2(64), 3);
            unCrouchAnim = new Animation("Characters/Jet/UnCrouch", 2, 4, new Vector2(64), false);
            walkAnim = new CompoundAnimation("Characters/Jet/Walk", 1, 8, 5, new Vector2(48));
            walkBackAnim = new CompoundAnimation("Characters/Jet/WalkBackward", 1, 8, 8, new Vector2(48));
            runAnim = new CompoundAnimation("Characters/Jet/Run", 2, 6, 6, new Vector2(48), 4);
            backdashAnim = new Animation("Characters/Jet/Backdash", 4, 4, new Vector2(48), false);
            jumpSquatAnim = new Animation("Characters/Jet/JumpSquat", 1, 0, new Vector2(64));
            airAnim = new Animation("Characters/Jet/Jump", 4, 0, new Vector2(64));
            airHurtAnim = new Animation("Characters/Jet/HurtAir", 5, 4, new Vector2(64), false);
            groundHurtAnim = new Animation("Characters/Jet/HurtStand", 2, 6, new Vector2(64), false);
            crouchHurtAnim = new Animation("Characters/Jet/HurtCrouch", 2, 6, new Vector2(64), false);
            overheadHurtAnim = new Animation("Characters/Jet/HurtOverhead", 2, 6, new Vector2(64), false);
            knockdownAnim = new Animation("Characters/Jet/Knockdown", 4, 3, new Vector2(64), false);
            guardHighAnim = new Animation("Characters/Jet/Guard_High", 3, 6, new Vector2(64));
            guardLowAnim = new Animation("Characters/Jet/Guard_Low", 3, 6, new Vector2(64));
            techForwardAnim = new Animation("Characters/Jet/Tech_Forward", 6, 5, new Vector2(48), false);
            techBackAnim = new Animation("Characters/Jet/Tech_Backward", 6, 5, new Vector2(48), false);

            if (scene.SpawnHealthBars) scene.UI.AddElement(new NewHeatMeter(this));

            attacks.Clear();

            //Light
            attacks.Add(new Attack(new Animation("Characters/Jet/att_StandLight", 8, 3, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishshort", 1)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.None, -2))
            {
                name = "Light",
                groundedness = 1,
                cancelLevel = 1,
                jumpCancelable = true,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = 7,
                        lifetime = 3,
                        size = new Vector2(80, 32),
                        offset = new Vector2(60, -16),
                        knockback = new Knockback(6, 0.5f, 65, false),
                        hitpause = 2,
                        hitstun = 18,
                        blockstun = 14,
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                }
            });

            //Light 2
            attacks.Add(new Attack(new Animation("Characters/Jet/att_StandLight2", 7, 5, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 3)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.None))
            {
                name = "Light 2",
                groundedness = 1,
                cancelLevel = 2,
                canUse = a => state == States.Attack && attack.name == "Light",
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        creationFrame = 11,
                        lifetime = 3,
                        size = new Vector2(64, 40),
                        offset = new Vector2(40, -8),
                        knockback = new Knockback(4, 0.5f, 70, false),
                        hitstun = 20,
                        hitpause = 2,
                        blockstun = 16,
                        blockPush = -3,
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(9, new Vector2(8, 0))
                }
            });

            //Light 3
            attacks.Add(new Attack(new Animation("Characters/Jet/att_StandLight3", 8, 5, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 3)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.None))
            {
                name = "Light 3",
                groundedness = 1,
                cancelLevel = 3,
                jumpCancelable = true,
                canUse = a => state == States.Attack && attack.name == "Light 2",
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 11,
                        lifetime = 3,
                        size = new Vector2(72, 64),
                        offset = new Vector2(64, 12),
                        knockback = new Knockback(13, 0.5f, 75, true),
                        hitstun = 24,
                        hitpause = 6,
                        blockstun = 18,
                        hitParticle = GenericParticles.HitLightDirectional(45)
                    }
                }
            });

            //Light 4
            attacks.Add(new Attack(new Animation("Characters/Jet/att_StandLight4", 7, 5, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 5)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.None))
            {
                name = "Light 4",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 4,
                airStall = true,
                landingLag = 16,
                canUse = a => state == States.Attack && attack.name == "Light 3"/* && CanUseFireMoves(a)*/,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 12,
                        creationFrame = 26,
                        lifetime = 3,
                        size = new Vector2(64, 80),
                        offset = new Vector2(56, 16),
                        knockback = new Knockback(16, 0, -70, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.ForceGroundBounce },
                        hitstun = 24,
                        hitpause = 8,
                        blockstun = 24,
                        knockdownDuration = 48,
                        hitParticle = GenericParticles.HitLightDirectional(-60),
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(6, new Vector2(5, -12f))
                }
            });

            //Crouch Light
            attacks.Add(new Attack(new Animation("Characters/Jet/att_CrouchLight", 5, 5, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishshort", 1)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.Down))
            {
                name = "Crouch Light",
                groundedness = 1,
                cancelLevel = 1,
                holdCrouch = true,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = 6,
                        lifetime = 4,
                        size = new Vector2(72, 24),
                        offset = new Vector2(48, 56),
                        knockback = new Knockback(5, 0.5f, 70, false),
                        comboScaling = 2,
                        hitstun = 18,
                        hitType = HitTypes.Low,
                        blockstun = 16,
                        hitboxGroup = 1,
                        hitParticle = GenericParticles.HitLightNeutral()
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = 6,
                        lifetime = 4,
                        size = new Vector2(48, 40),
                        offset = new Vector2(32, 48),
                        knockback = new Knockback(5, 0.5f, 70, false),
                        comboScaling = 2,
                        hitstun = 18,
                        hitType = HitTypes.Low,
                        blockstun = 16,
                        hitboxGroup = 1,
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                }
            });

            //Forward Light
            attacks.Add(new Attack(new Animation("Characters/Jet/att_ForwardLight", 10, 4, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 4)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.Forward))
            {
                name = "Forward Light",
                groundedness = 1,
                cancelLevel = 3,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 12,
                        creationFrame = 17,
                        lifetime = 1,
                        size = new Vector2(48, 32),
                        offset = new Vector2(40, -16),
                        knockback = new Knockback(10, 1, 45, false),
                        hitstun = 26,
                        blockstun = 20,
                        hitboxGroup = 1,
                        hitParticle = GenericParticles.HitLightDirectional(0)
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 18,
                        lifetime = 4,
                        size = new Vector2(40, 28),
                        offset = new Vector2(40, -16),
                        knockback = new Knockback(10, 1, 45, false),
                        hitstun = 22,
                        blockstun = 20,
                        hitboxGroup = 1,
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(13, new Vector2(16, 0))
                }
            });

            //Medium
            attacks.Add(new Attack(new Animation("Characters/Jet/att_StandMedium", 9, 4, new Vector2(64))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 3)
                }
            }, new AttackInput(Controller.Key_Medium))
            {
                name = "Medium",
                groundedness = 1,
                cancelLevel = 2,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 13,
                        lifetime = 3,
                        size = new Vector2(96, 48),
                        offset = new Vector2(68, 8),
                        knockback = new Knockback(11, 0.75f, 65, false),
                        hitstun = 21,
                        blockstun = 18,
                        hitParticle = GenericParticles.HitLightDirectional(30)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(10, new Vector2(8, 0))
                }
            });

            //Crouch Medium
            attacks.Add(new Attack(new Animation("Characters/Jet/att_CrouchMedium", 9, 4, new Vector2(80))
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 3)
                }
            }, new AttackInput(Controller.Key_Medium, InputMotions.Down))
            {
                name = "Crouch Medium",
                groundedness = 1,
                cancelLevel = 2,
                primaryHurtbox = crouchHurtbox,
                holdCrouch = true,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 9,
                        creationFrame = 13,
                        lifetime = 3,
                        size = new Vector2(84, 40),
                        offset = new Vector2(86, 48),
                        knockback = new Knockback(11, 0.5f, 40f, true),
                        hitstun = 18,
                        hitType = HitTypes.Low,
                        blockstun = 18,
                        hitParticle = GenericParticles.HitLightDirectional(-15)
                    }
                }
            });

            //Heavy
            attacks.Add(new Attack(new Animation("Characters/Jet/att_Heavy", 8, 5, new Vector2(80))
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_Special_Particle", 7, 4, new Vector2(80), false), 4, 28) {stayWithOwner = false}
                }
            }, new AttackInput(Controller.Key_Heavy))
            {
                name = "Heavy",
                groundedness = 1,
                cancelLevel = 3,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 10,
                        chipDamage = 2,
                        creationFrame = 16,
                        lifetime = 4,
                        size = new Vector2(96, 56),
                        offset = new Vector2(64, 4),
                        knockback = new Knockback(14, 0.5f, 75, true),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.Launcher },
                        hitpause = 2,
                        blockstun = 16,
                        postHitEffect = (h, p) => { ApplyBurn(p, 1f); ConsumeHeatCharge(); } ,
                        hitParticle = GenericParticles.HitFireDirectional(45),
                        hitboxGroup = 1
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        chipDamage = 1,
                        creationFrame = 20,
                        lifetime = 4,
                        size = new Vector2(96, 64),
                        offset = new Vector2(88, -16),
                        knockback = new Knockback(8, 0.5f, 80, false),
                        hitstun = 24,
                        hitpause = 2,
                        blockstun = 16,
                        postHitEffect = (h, p) => { ApplyBurn(p, 0.5f); ConsumeHeatCharge(); } ,
                        hitParticle = GenericParticles.HitFireNeutral(),
                        hitboxGroup = 1
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        chipDamage = 1,
                        creationFrame = 24,
                        lifetime = 2,
                        size = new Vector2(80, 64),
                        offset = new Vector2(96, -24),
                        knockback = new Knockback(8, 0.5f, 80, false),
                        hitstun = 24,
                        hitpause = 2,
                        blockstun = 16,
                        postHitEffect = (h, p) => { ApplyBurn(p, 0.5f); ConsumeHeatCharge(); } ,
                        hitParticle = GenericParticles.HitFireNeutral(),
                        hitboxGroup = 1
                    }
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_Heavy_NoFire", 8, 5, new Vector2(80)), new AttackInput(Controller.Key_Heavy))
            {
                name = "Heavy No Fire",
                groundedness = 1,
                cancelLevel = 3,
                canUse = a => !CanUseFireMoves(a) && heatCharges != 0,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        creationFrame = 16,
                        lifetime = 3,
                        size = new Vector2(64, 48),
                        offset = new Vector2(48, 0),
                        knockback = new Knockback(8, 0.5f, 70, false),
                        hitpause = 2,
                        blockstun = 12,
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                }
            });

            //Crouch Heavy
            attacks.Add(new Attack(new Animation("Characters/Jet/att_CrouchHeavy", 15, 3, new Vector2(64))
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_CrouchSpecial_Particle", 5, 4, new Vector2(64), false), 11, 20, 0, 0, 1, false)
                }
            }, new AttackInput(Controller.Key_Heavy, InputMotions.Down))
            {
                name = "Crouch Heavy",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 3,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        chipDamage = 1,
                        creationFrame = 13,
                        lifetime = 3,
                        size = new Vector2(80, 112),
                        offset = new Vector2(48, -16),
                        knockback = new Knockback(10, 0f, 90, true),
                        postHitEffect = (h, p) =>
                        {
                            if (p.position.Y <= position.Y - 32) p.velocity.Y /= 2;
                            ConsumeHeatCharge();
                            ApplyBurn(p, 2f);
                        },
                        hitstun = 24,
                        blockPush = 0,
                        comboScaling = 0.5f,
                        hitParticle = GenericParticles.HitFireNeutral()
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        chipDamage = 1,
                        creationFrame = 31,
                        lifetime = 3,
                        size = new Vector2(112, 64),
                        offset = new Vector2(48, 24),
                        knockback = new Knockback(16, 0.75f, 45, true),
                        hitstun = 24,
                        comboScaling = 0.5f,
                        hitParticle = GenericParticles.HitFireDirectional(0)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(10, new Vector2(0, -9))
                },
                attackUpdate = a =>
                {
                    if (velocity.Y < 0) velocity.Y -= gravity / 2;
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_CrouchHeavy_NoFire", 15, 3, new Vector2(64)), new AttackInput(Controller.Key_Heavy, InputMotions.Down))
            {
                name = "Crouch Heavy No Fire",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 3,
                canUse = a => !CanUseFireMoves(a) && heatCharges != 0,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 3,
                        creationFrame = 13,
                        lifetime = 3,
                        size = new Vector2(80, 112),
                        offset = new Vector2(48, -16),
                        knockback = new Knockback(9.5f, 0f, 90, true),
                        postHitEffect = (h, p) =>
                        {
                            if (p.position.Y <= position.Y - 32) p.velocity.Y /= 2;
                        },
                        hitstun = 24,
                        blockstun = 12,
                        blockPush = 0,
                        comboScaling = 0.5f,
                        hitParticle = GenericParticles.HitLightNeutral()
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = 31,
                        lifetime = 3,
                        size = new Vector2(112, 64),
                        offset = new Vector2(48, 24),
                        knockback = new Knockback(12, 0.75f, 30, true),
                        hitstun = 24,
                        blockstun = 12,
                        comboScaling = 0.5f,
                        hitParticle = GenericParticles.HitLightDirectional(0)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(10, new Vector2(0, -9))
                },
                attackUpdate = a =>
                {
                    if (velocity.Y < 0) velocity.Y -= gravity / 2;
                }
            });

            //Air Light
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirLight", 9, 3, new Vector2(64)), new AttackInput(Controller.Key_Light))
            {
                name = "Air Light",
                groundedness = 2,
                cancelLevel = 1,
                landingLag = 4,
                jumpCancelable = true,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 5,
                        creationFrame = 7,
                        lifetime = 3,
                        size = new Vector2(56, 56),
                        offset = new Vector2(36, 12),
                        knockback = new Knockback(8, 0.5f, 60, false),
                        hitstun = 16,
                        hitType = HitTypes.Overhead,
                        blockstun = 16,
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                }
            });

            //Air Medium
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirMedium", 12, 3, new Vector2(64), false), new AttackInput(Controller.Key_Medium))
            {
                name = "Air Medium",
                groundedness = 2,
                cancelLevel = 2,
                landingLag = 8,
                jumpCancelable = true,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = 10,
                        lifetime = 3,
                        size = new Vector2(64, 48),
                        offset = new Vector2(52, 16),
                        knockback = new Knockback(5.5f, 0, 70, false),
                        comboScaling = 0.25f,
                        hitstun = 22,
                        hitType = HitTypes.Overhead,
                        blockstun = 20,
                        hitParticle = GenericParticles.HitLightNeutral()
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 5,
                        creationFrame = 22,
                        lifetime = 3,
                        size = new Vector2(80, 48),
                        offset = new Vector2(64, 16),
                        knockback = new Knockback(12, 0.75f, 50, false),
                        comboScaling =  0.75f,
                        hitstun = 24,
                        hitType = HitTypes.Overhead,
                        blockstun = 20,
                        hitParticle = GenericParticles.HitLightDirectional(15)
                    }
                }
            });

            //Air Down Medium
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirDownMedium", 9, 4, new Vector2(64), false), new AttackInput(Controller.Key_Medium, InputMotions.Down))
            {
                name = "Air Down Medium",
                groundedness = 2,
                cancelLevel = 3,
                landingLag = 12,
                jumpCancelable = true,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 10,
                        creationFrame = 9,
                        lifetime = 3,
                        size = new Vector2(64, 96),
                        offset = new Vector2(48, -32),
                        knockback = new Knockback(14, 1.5f, 70, true),
                        blockstun = 16,
                        hitParticle = GenericParticles.HitLightDirectional(75),
                        hitboxGroup = 1
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 13,
                        lifetime = 3,
                        size = new Vector2(80, 48),
                        offset = new Vector2(-8, -72),
                        knockback = new Knockback(12, 1f, 100, false),
                        blockstun = 16,
                        hitParticle = GenericParticles.HitLightDirectional(115),
                        hitboxGroup = 1
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        creationFrame = 17,
                        lifetime = 3,
                        size = new Vector2(48, 80),
                        offset = new Vector2(-52, -16),
                        knockback = new Knockback(8, 0f, 150, false),
                        hitType = HitTypes.Overhead,
                        hitstun = 26,
                        blockstun = 16,
                        hitParticle = GenericParticles.HitLightNeutral(),
                        hitboxGroup = 1,
                        blockPush = -4
                    }
                }
            });

            //Air Heavy
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirHeavy", 10, 4, new Vector2(64))
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_AirHeavy_Particle", 7, 4, new Vector2(64), false), 6, 28)
                }
            }, new AttackInput(Controller.Key_Heavy))
            {
                name = "Air Heavy",
                groundedness = 2,
                cancelLevel = 3,
                landingLag = 12,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 12,
                        chipDamage = 2,
                        creationFrame = 21,
                        lifetime = 3,
                        size = new Vector2(80, 112),
                        offset = new Vector2(64, 0),
                        knockback = new Knockback(28, 0, -80, true),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.GroundBounce },
                        hitstun = 24,
                        hitpause = 6,
                        hitType = HitTypes.Overhead,
                        blockstun = 16,
                        postHitEffect = (h, p)  =>
                        {
                            if (p.groundBounced)
                            {
                                p.hitstunProperties.Add(HitstunProperties.Knockdown);
                                p.knockdownDuration = 24;
                            }
                            ApplyBurn(p, 1.5f);
                            ConsumeHeatCharge();
                        },
                        hitParticle = GenericParticles.HitFireDirectional(-60)
                    }
                },
                attackUpdate = a =>
                {
                    if (attackTimer == 0 && hitstunTimer <= 0)
                    {
                        velocity.X = Math.Clamp(velocity.X + 2 * direction, -6, 6);
                        velocity.Y = Math.Min(velocity.Y, -8);
                    }
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirHeavy_NoFire", 10, 3, new Vector2(64)), new AttackInput(Controller.Key_Heavy))
            {
                name = "Air Heavy No Fire",
                groundedness = 2,
                cancelLevel = 3,
                landingLag = 12,
                canUse = a => !CanUseFireMoves(a) && heatCharges != 0,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        creationFrame = 16,
                        lifetime = 3,
                        size = new Vector2(64, 96),
                        offset = new Vector2(60, 0),
                        knockback = new Knockback(16, 0, -75, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.GroundBounce },
                        hitstun = 20,
                        hitpause = 4,
                        hitType = HitTypes.Overhead,
                        hitParticle = GenericParticles.HitLightDirectional(-60)
                    }
                }
            });

            //Jet Flurry
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfLight", 15, 3, new Vector2(64), false)
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_qcfLight_Particle", 6, 5, new Vector2(64), false), 10, 30)
                }
            }, new AttackInput(Controller.Key_Light, InputMotions.ChargeBack))
            {
                name = "Jet Flurry",
                groundedness = 1,
                cancelLevel = 4,
                canUse = CanUseFireMoves,
                overrideEffectiveRange = 256,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 2,
                        chipDamage = 1,
                        creationFrame = 10,
                        lifetime = 2,
                        size = new Vector2(64, 64),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(5, 0, 60, false),
                        blockstun = 12,
                        postHitEffect = (h, p) => { ApplyBurn(p, 1f); },
                        hitstun = 24,
                        hitpause = 2,
                        comboScaling = 0,
                        hitParticle = null
                    },
                    new HitboxSpawner(this)
                    {
                        parent = 0,
                        creationFrame = 13
                    },
                    new HitboxSpawner(this)
                    {
                        parent = 0,
                        creationFrame = 16
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        chipDamage = 1,
                        creationFrame = 28,
                        lifetime = 3,
                        size = new Vector2(56, 96),
                        offset = new Vector2(48, -16),
                        knockback = new Knockback(13f, 4f, 73f, true),
                        blockstun = 13,
                        postHitEffect = (h, p) => { ApplyBurn(p, 1.5f); ConsumeHeatCharge(); },
                        hitParticle = GenericParticles.HitFireDirectional(45)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(7, new Vector2(24, 0))
                },
                attackUpdate = a =>
                {
                    if (attackTimer <= 16) velocity.X *= 0.95f;
                    if (attackTimer == 6 || attackTimer == 10 || attackTimer == 12) PaletteBasedAfterImage();
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfLight_NoFire", 9, 3, new Vector2(64), false), new AttackInput(Controller.Key_Light, InputMotions.ChargeBack))
            {
                name = "Jet Flurry No Fire",
                duration = 30,
                holdCrouch = true,
                groundedness = 1,
                cancelLevel = 4,
                canUse = a => !CanUseFireMoves(a),
                overrideEffectiveRange = 212,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 2,
                        creationFrame = 10,
                        lifetime = 2,
                        size = new Vector2(64, 64),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(4, 1, 50, false),
                        blockstun = 5,
                        hitstun = 14,
                        hitpause = 2,
                        comboScaling = 0.25f,
                        hitParticle = null
                    },
                    new HitboxSpawner(this)
                    {
                        parent = 0,
                        creationFrame = 13
                    },
                    new HitboxSpawner(this)
                    {
                        parent = 0,
                        creationFrame = 16
                    },
                    new HitboxSpawner(this)
                    {
                        parent = 0,
                        creationFrame = 19
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(7, new Vector2(20, 0))
                },
                attackUpdate = a =>
                {
                    if (attackTimer <= 16) velocity.X *= 0.96f;
                }
            });

            //Flip Kick back
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium", 14, 4, new Vector2(64), false)
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_qcbMedium_ParticleHit", 5, 5, new Vector2(64), false), -1, 25),
                    new AnimationParticle(new Animation("Characters/Jet/att_qcbMedium_ParticleWhiff", 4, 5, new Vector2(64), false), 9, 25)
                }
            }, new AttackInput(Controller.Key_Medium, InputMotions.QuarterBack))
            {
                name = "Flip Kick",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 4,
                canCancel = false,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        chipDamage = 2,
                        creationFrame = 29,
                        lifetime = 3,
                        size = new Vector2(56, 48),
                        offset = new Vector2(-48, 48),
                        knockback = new Knockback(4, 0, 90, false),
                        postHitEffect = (h, p) => { ApplyBurn(p, p.Grounded ? 1f : 1.5f); ConsumeHeatCharge(); } ,
                        hitType = HitTypes.Overhead,
                        hitstun = 22,
                        hitpause = 0,
                        blockstun = 20,
                        blockPush = -4,
                        hitParticle = null,
                        comboScaling = 0
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(1, new Vector2(5, -13))
                },
                attackUpdate = a =>
                {
                    if (attackTimer < a.hitboxes[0].creationFrame)
                    {
                        dontPushPlayer = true;
                        scene.StealCorner(this);

                        if (nearestPlayer != null && nearestPlayer.Grounded && Math.Sign(position.X - nearestPlayer.position.X) == direction) velocity.X *= 0.95f;
                        if (attackTimer % 6 == 0) PaletteBasedAfterImage();
                    }
                    if (hasHitPlayer || hasHitBlock)
                    {
                        a.anim.particles[0].Spawn(this);
                        direction *= -1;
                    }
                    if (hasHitPlayer) SetAttack(GetAttackFromName("Flip Kick Hold"), true, false);
                    else if (hasHitBlock) SetAttack(GetAttackFromName("Flip Kick Hold On Block"), true, false);
                }
            });
            //Hold
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium_Hold", 1, 0, new Vector2(64), false))
            {
                name = "Flip Kick Hold",
                duration = 10,
                groundedness = 0,
                cancelLevel = 4,
                attackUpdate = a =>
                {
                    velocity = new Vector2(0, -gravity);
                    if (!lastPlayerHit.Grounded) lastPlayerHit.velocity = new Vector2(0, -lastPlayerHit.gravity);

                    if (attackTimer == a.duration)
                    {
                        if (HoldingForward) SetAttack(GetAttackFromName("Flip Kick Forward"), false, false);
                        else SetAttack(GetAttackFromName("Flip Kick Backward"), false, false);
                    }
                }
            });
            //Hold on Block
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium_Hold", 1, 0, new Vector2(64), false))
            {
                name = "Flip Kick Hold On Block",
                duration = 10,
                groundedness = 0,
                cancelLevel = 4,
                attackUpdate = a =>
                {
                    velocity = new Vector2(0, -gravity);

                    if (attackTimer == a.duration)
                    {
                        if (HoldingForward) SetAttack(GetAttackFromName("Flip Kick Forward"), false, false);
                        else SetAttack(GetAttackFromName("Flip Kick Backward"), false, false);
                    }
                }
            });
            //Finish Forward
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium_Forward", 4, 4, new Vector2(64), false))
            {
                name = "Flip Kick Forward",
                groundedness = 0,
                cancelLevel = 4,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        chipDamage = 2,
                        creationFrame = 1,
                        lifetime = 3,
                        size = new Vector2(128, 96),
                        offset = new Vector2(0, 32),
                        knockback = new Knockback(24, 0, -70, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.GroundBounce },
                        hitstun = 22,
                        hitpause = 0,
                        blockstun = 4,
                        blockPush = 4,
                        hitSound = "",
                        hitParticle = GenericParticles.HitFireDirectional(-45)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(1, new Vector2(3, -10))
                }
            });
            //Finish Backward
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium_Backward", 4, 4, new Vector2(64), false))
            {
                name = "Flip Kick Backward",
                groundedness = 0,
                cancelLevel = 4,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        chipDamage = 2,
                        creationFrame = 1,
                        lifetime = 3,
                        size = new Vector2(128, 96),
                        offset = new Vector2(0, 32),
                        knockback = new Knockback(24, 0, -80, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.GroundBounce },
                        hitstun = 22,
                        hitpause = 0,
                        blockstun = 4,
                        blockPush = 4,
                        hitSound = "",
                        hitParticle = GenericParticles.HitFireDirectional(-45)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(1, new Vector2(-3, -9))
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium_NoFire", 14, 4, new Vector2(64), false), new AttackInput(Controller.Key_Heavy, InputMotions.QuarterBack))
            {
                name = "Flip Kick No Fire",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 4,
                canCancel = false,
                canUse = a => !CanUseFireMoves(a),
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 29,
                        lifetime = 3,
                        size = new Vector2(56, 48),
                        offset = new Vector2(-48, 48),
                        knockback = new Knockback(20, 0, -110, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.GroundBounce },
                        hitType = HitTypes.Overhead,
                        hitstun = 22,
                        hitpause = 8,
                        blockstun = 18,
                        blockPush = -4,
                        hitParticle = GenericParticles.HitLightDirectional(-45),
                        comboScaling = 1
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(1, new Vector2(5, -13))
                },
                attackUpdate = a =>
                {
                    if (attackTimer < a.hitboxes[0].creationFrame)
                    {
                        dontPushPlayer = true;
                        scene.StealCorner(this);

                        if (nearestPlayer != null && nearestPlayer.Grounded && Math.Sign(position.X - nearestPlayer.position.X) == direction) velocity.X *= 0.95f;
                    }
                    if (hasHitPlayer || hasHitBlock)
                    {
                        direction *= -1;
                        SetAttack(GetAttackFromName("Flip Kick Backward No Fire"), true, false);
                    }
                }
            });
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcbMedium_Backward", 4, 4, new Vector2(64), false))
            {
                name = "Flip Kick Backward No Fire",
                groundedness = 0,
                cancelLevel = 4,
                pushers = new Pusher[]
                {
                    new Pusher(1, new Vector2(-4, -9))
                }
            });

            //Flip Kick front
            attacks.Add(new Attack(new CompoundAnimation("Characters/Jet/att_qcfMedium", 5, 3, 2, new Vector2(96), 4), new AttackInput(Controller.Key_Medium, InputMotions.QuarterForward))
            {
                name = "Front Flip Kick",
                duration = 180,
                groundedness = 0,
                cancelLevel = 4,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        chipDamage = 2,
                        creationFrame = 21,
                        lifetime = 60,
                        size = new Vector2(64, 112),
                        offset = new Vector2(64, -24),
                        knockback = new Knockback(18, 0, -75, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.ForceGroundBounce },
                        hitType = HitTypes.Overhead,
                        hitpause = 0,
                        blockstun = 18,
                        postHitEffect = (h, p) =>
                        {
                            if (!p.Grounded)
                            {
                                p.position.Y = position.Y - 24;
                                p.CollisionCorrection();
                            }
                        },
                        hitParticle = null,
                        hitSound = ""
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(6, new Vector2(12, -13))
                },
                attackUpdate = a =>
                {
                    if (!Grounded && animation.currentFrame == 1)
                    {
                        attackTimer = animation is CompoundAnimation ca ? ca.startupFrameRate : animation.currentFrame;
                        animation.currentFrame = 2;
                    }

                    if (!Grounded && hitstunTimer <= 0)
                    {
                        if (Math.Sign(velocity.X) == direction) velocity.X -= 0.4f * direction;
                        else velocity.X = 0;
                        if (animation.currentFrame >= 4 && velocity.Y < fallSpeed * 1.5f) velocity.Y += 3;
                    }

                    if (attackTimer > 10 && Grounded)
                    {
                        SetAttack(GetAttackFromName("Front Flip Kick Land"), false, false);
                        scene.camera.ShakeCamera(6, 6);
                        attack.hitboxes[0].SpawnHitBox();

                        animation.particles[0].Spawn(this);
                    }
                    if (attackTimer % 6 == 2) PaletteBasedAfterImage();
                }
            });
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfMedium_Land", 6, 4, new Vector2(96), false)
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_qcfMedium_Particle", 7, 4, new Vector2(96), false), -1, 28)
                }
            })
            {
                name = "Front Flip Kick Land",
                groundedness = 1,
                cancelLevel = 4,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 10,
                        chipDamage = 2,
                        creationFrame = -1,
                        lifetime = 3,
                        size = new Vector2(112, 96),
                        offset = new Vector2(96, 24),
                        knockback = new Knockback(18, 0, -60, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.GroundBounce, HitstunProperties.Knockdown },
                        groundHurtAnimation = GroundHurtAnimationSelection.Overhead,
                        hitstun = 28,
                        hitpause = 8,
                        blockstun = 12,
                        postHitEffect = (h, p) =>
                        {
                            if (!p.Grounded)
                            {
                                p.position.Y = position.Y - 12;
                                p.CollisionCorrection();
                            }
                            ApplyBurn(p, p.Grounded ? 0.5f : 1.5f);
                            ConsumeHeatCharge();
                        },
                        hitParticle = GenericParticles.HitFireDirectional(90)
                    }
                }
            });

            attacks.Add(new Attack(new CompoundAnimation("Characters/Jet/att_qcfMedium_NoFire", 5, 3, 2, new Vector2(96), 5), new AttackInput(Controller.Key_Medium, InputMotions.QuarterForward))
            {
                name = "Front Flip Kick No Fire",
                duration = 180,
                groundedness = 0,
                cancelLevel = 4,
                canUse = a => !CanUseFireMoves(a),
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 2,
                        creationFrame = 26,
                        lifetime = 60,
                        size = new Vector2(64, 112),
                        offset = new Vector2(64, -24),
                        knockback = new Knockback(18, 0, -85, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.ForceGroundBounce },
                        hitType = HitTypes.Overhead,
                        hitpause = 0,
                        blockstun = 18,
                        postHitEffect = (h, p) =>
                        {
                            if (!p.Grounded)
                            {
                                p.position.Y = position.Y - 24;
                                p.CollisionCorrection();
                            }
                        },
                        hitParticle = null,
                        hitSound = ""
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(6, new Vector2(10, -10))
                },
                attackUpdate = a =>
                {
                    if (!Grounded && animation.currentFrame == 1)
                    {
                        attackTimer = animation is CompoundAnimation ca ? ca.startupFrameRate : animation.currentFrame;
                        animation.currentFrame = 2;
                    }

                    if (!Grounded && hitstunTimer <= 0)
                    {
                        if (Math.Sign(velocity.X) == direction) velocity.X -= 0.4f * direction;
                        else velocity.X = 0;
                        if (animation.currentFrame >= 4 && velocity.Y < fallSpeed * 1.5f) velocity.Y += 1.5f;
                        else velocity.Y -= gravity / 3;
                    }

                    if (attackTimer > 10 && Grounded)
                    {
                        SetAttack(GetAttackFromName("Front Flip Kick Land No Fire"), false, false);
                        scene.camera.ShakeCamera(4, 6);
                        attack.hitboxes[0].SpawnHitBox();
                    }
                }
            });
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfMedium_Land_NoFire", 6, 4, new Vector2(96), false))
            {
                name = "Front Flip Kick Land No Fire",
                groundedness = 1,
                cancelLevel = 4,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        creationFrame = -1,
                        lifetime = 3,
                        size = new Vector2(80, 64),
                        offset = new Vector2(64, 32),
                        knockback = new Knockback(18, 0, -60, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.Knockdown },
                        hitstun = 26,
                        hitpause = 2,
                        blockstun = 10,
                        postHitEffect = (h, p) =>
                        {
                            if (!p.Grounded)
                            {
                                p.position.Y = position.Y - 12;
                                p.CollisionCorrection();
                            }
                        },
                        hitParticle = GenericParticles.HitLightNeutral()
                    }
                }
            });

            //Jet Uppercut
            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfHeavy", 22, 4, new Vector2(64), false), new AttackInput(Controller.Key_Heavy, InputMotions.DPForward))
            {
                name = "Jet Uppercut",
                duration = 56,
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 4,
                airStall = true,
                landingLag = 20,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 13,
                        lifetime = 3,
                        size = new Vector2(48, 64),
                        offset = new Vector2(24, 4),
                        knockback = new Knockback(16, 0, 90, true),
                        hitpause = 10,
                        blockstun = 20,
                        hitParticle = GenericParticles.HitFireDirectional(75),
                        modifiers = p =>
                        {
                            p.chipDamage = 2 + uppercutCharge / 8;
                        },
                        postHitEffect = (h, p) =>
                        {
                            while (TouchingTile(collider.Box, 48 * direction, 0)) position.X -= direction;
                            CollisionCorrection();
                            p.direction = -direction;
                            p.position = position + new Vector2(64 * direction, -4);
                            p.CollisionCorrection();
                            ApplyBurn(p, 1f);
                        }
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = -1,
                        lifetime = 2,
                        size = new Vector2(80, 96),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(12, 0, 80, true),
                        hitpause = 1,
                        comboScaling = 0,
                        blockstun = 20,
                        hitParticle = GenericParticles.HitFireNeutral(),
                        modifiers = p =>
                        {
                            p.chipDamage = 1 + uppercutCharge / 16;
                        },
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = -1,
                        lifetime = 4,
                        size = new Vector2(80, 112),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(21, 0, 70, true),
                        hitpause = 6,
                        blockstun = 20,
                        hitParticle = GenericParticles.HitFireDirectional(75),
                        modifiers = p =>
                        {
                            p.chipDamage = 2 + uppercutCharge / 8;
                        },
                        postHitEffect = (h, p) =>
                        {
                            if (uppercutCharge == 32 && p.comboScaling <= -1)
                            {
                                p.velocity = p.velocity.RotatedBy(-5 * direction);
                                p.hitstunProperties.Add(HitstunProperties.Launcher);
                            }
                            ApplyBurn(p, 2f);
                            ConsumeHeatCharge();
                        }
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(9, new Vector2(14, 0)),
                    new Pusher(17, new Vector2(4, -18))
                },
                attackUpdate = a =>
                {
                    if (attackTimer == 1) uppercutCharge = 0;

                    if (attackTimer == 8 && uppercutCharge < 32 && input.KeyDown(attack.input.Button))
                    {
                        uppercutCharge++;
                        attackTimer--;

                        if (uppercutCharge > 8 && input.KeyDown(Controller.Key_Light))
                        {
                            input.ClearBuffer(Controller.Key_Light);
                            uppercutCharge = 0;
                            SetAttack(GetAttackFromName("Uppercut Cancel"), false);
                        }
                    }
                    if (attackTimer == 8)
                    {
                        animation.currentFrame = 11;
                        animation.timer = 0;
                        Iframes = 12;
                    }

                    if (attackTimer >= 16 && attackTimer <= 36 && attackTimer % 4 == 0)
                    {
                        int n = (attackTimer - 16) / 4;
                        if (n < uppercutCharge / 8) attack.hitboxes[1].SpawnHitBox();
                        if (n == uppercutCharge / 8) attack.hitboxes[2].SpawnHitBox();
                    }
                    if (attackTimer == 12 || (attackTimer % 6 == 0 && velocity.Y < 0)) PaletteBasedAfterImage();
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfSpecial_Cancel", 2, 6, new Vector2(64))
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_qcfSpecial_Cancel_Particle", 3, 4, new Vector2(64), false), 1, 12)
                }
            })
            {
                name = "Uppercut Cancel",
                duration = 4,
                groundedness = 1
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_qcfSpecial_NoFire", 13, 4, new Vector2(64), false), new AttackInput(Controller.Key_Heavy, InputMotions.DPForward))
            {
                name = "Jet Uppercut No Fire",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 4,
                airStall = true,
                landingLag = 12,
                canUse = a => !CanUseFireMoves(a),
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 13,
                        lifetime = 6,
                        size = new Vector2(48, 64),
                        offset = new Vector2(28, -32),
                        knockback = new Knockback(17, 0, 70, true),
                        hitpause = 6,
                        blockstun = 16,
                        hitParticle = GenericParticles.HitLightDirectional(75)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(5, new Vector2(13, 0)),
                    new Pusher(13, new Vector2(2.5f, -16))
                }
            });

            //Burnout Grab
            attacks.Add(new Attack(new CompoundAnimation("Characters/Jet/att_Burnout_Air", 9, 2, 4, new Vector2(96)), new AttackInput(Controller.Key_Heavy, InputMotions.None))
            {
                name = "Burnout Grab",
                duration = -1,
                groundedness = 0,
                cancelLevel = 5,
                canUse = a => heatCharges == 0 && heatCooldown <= 0,
                attackUpdate = a =>
                {
                    heatCooldown = 300;
                    heatCharges = 3;

                    if (lastPlayerHit == null) return;
                    if (attackTimer == 0 && lastPlayerHit != null)
                    {
                        scene.ApplyHitpause(4);

                        Vector2 oldPos = position;
                        position = lastPlayerHit.position + new Vector2(-32 * direction, -16);
                        //Fire trail
                        float length = (oldPos - position).Length();
                        for (int i = (int)-length / 4; i < length * 1.25f; i += 8)
                        {
                            int rate = game.random.Next(3, 6);
                            float rot = MathHelper.ToDegrees((float)Math.Atan2(position.Y - oldPos.Y, position.X - oldPos.X)) + 270;
                            Vector2 pos = Vector2.Lerp(oldPos, position, i / length);
                            Vector2 offset = new Vector2((float)game.random.NextDouble() * 16, 0).RotatedBy(game.random.Next(360));
                            new Particle(new Animation("Particles/Red/prt_Ember1", 5, rate, new Vector2(16), false), pos + offset, 5 * rate, Vector2.Zero)
                            { drawLayer = drawLayer + 0.000001f, rotation = rot };
                        }

                        velocity = new Vector2(7 * direction, -8);
                    }
                    if (attackTimer <= 24) velocity.Y -= gravity / 1.5f;
                    else if (velocity.Y < fallSpeed * 2) velocity.Y += gravity;
                    scene.camera.OverridePosition(position + new Vector2(8 * direction, -attackTimer));

                    lastPlayerHit.hitstunTimer = 2;
                    Vector2 enemyTargetPos = lastPlayerHit.position;
                    if (animation.currentFrame == 1 || animation.currentFrame == 2) enemyTargetPos = new Vector2(position.X + (48 * direction), position.Y - 16);
                    if (animation.currentFrame == 3 || animation.currentFrame == 7) enemyTargetPos = new Vector2(position.X + (64 * direction), position.Y - 16);
                    if (animation.currentFrame == 4 || animation.currentFrame == 8) enemyTargetPos = new Vector2(position.X, position.Y - 64);
                    if (animation.currentFrame == 5 || animation.currentFrame >= 9) enemyTargetPos = new Vector2(position.X - (32 * direction), position.Y - 16);
                    if (animation.currentFrame == 6) enemyTargetPos = new Vector2(position.X + (16 * direction), position.Y + 16);
                    if (animation.currentFrame == 5 || animation.currentFrame == 6 || animation.currentFrame >= 9) MoveToBack();
                    lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, enemyTargetPos, 0.4f);
                    lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                    lastPlayerHit.velocity.X = 0;
                    lastPlayerHit.direction = -direction;
                    lastPlayerHit.CollisionCorrection();
                    if (Grounded && attackTimer > 4)
                    {
                        while (TouchingTile(collider.Box, direction * 32, -1) || TouchingTile(collider.Box, direction * 96, -1)) position.X -= direction;
                        lastPlayerHit.position = position + new Vector2(80 * direction, -8);
                        lastPlayerHit.CollisionCorrection();
                        SetAttack(GetAttackFromName("Burnout Slam"), false, false);
                        attack.anim.particles[0].Spawn(this);
                    }
                    if (attackTimer % 4 == 0 && velocity.Y > 0) PaletteBasedAfterImage();
                }
            });

            //Burnout Slam
            attacks.Add(new Attack(new Animation("Characters/Jet/att_Burnout_Land", 8, 5, new Vector2(96))
            {
                particles = new AnimationParticle[]
                {
                    new AnimationParticle(new Animation("Characters/Jet/att_Burnout_Particle", 8, 4, new Vector2(96), false), -1, 32)
                }
            })
            {
                name = "Burnout Slam",
                groundedness = 1,
                cancelLevel = 5,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 40,
                        creationFrame = 1,
                        lifetime = 2,
                        size = new Vector2(64, 80),
                        offset = new Vector2(48, 24),
                        knockback = new Knockback(24, 0f, -80, true),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.ForceGroundBounce, HitstunProperties.Knockdown },
                        knockdownDuration = 50,
                        hitpause = 10,
                        postHitEffect = (h, p) => { ApplyBurn(p, 2.5f); },
                        hitSound = "Sounds/Characters/Jet/sfx_FireHit",
                        hitParticle = GenericParticles.HitFireDirectional(90)
                    }
                }
            });

            //Grab
            attacks.Add(new Attack(new Animation("Characters/Jet/att_Grab", 4, 4, new Vector2(64), false), new AttackInput(Controller.Key_Grab))
            {
                name = "Grab",
                duration = 120,
                groundedness = 1,
                cancelLevel = 2,
                canCancel = false,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 0,
                        creationFrame = 9,
                        lifetime = 2,
                        size = new Vector2(64, 32),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(0, 0f, 0, false),
                        hitType = HitTypes.Grab,
                        hitpause = 0,
                        comboScaling = 2,
                        hitSound = "",
                        hitParticle = null
                    }
                }
            });

            //Grab miss
            attacks.Add(new Attack(new Animation("Characters/Jet/att_GrabMiss", 4, 6, new Vector2(64)))
            {
                name = "Grab Miss",
                groundedness = 1,
                cancelLevel = 1,
                canCancel = false
            });
            //Grab Release
            attacks.Add(new Attack(new Animation("Characters/Jet/att_GrabRelease", 3, 5, new Vector2(64), false))
            {
                name = "Grab Release",
                duration = 15,
                groundedness = 1,
                cancelLevel = 1,
                canCancel = false
            });

            //Forward Throw
            attacks.Add(new Attack(new Animation("Characters/Jet/att_ForwardThrow", 15, 4, new Vector2(64)))
            {
                name = "Forward Throw",
                groundedness = 1,
                forceGroundedness = false,
                cancelLevel = 3,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 4,
                        creationFrame = 5,
                        lifetime = 1,
                        size = new Vector2(32, 48),
                        offset = new Vector2(52, 12),
                        knockback = new Knockback(11, 0f, 85, true),
                        comboScaling = 0.5f,
                        hitParticle = GenericParticles.HitLightNeutral()
                    },
                    new HitboxSpawner(this)
                    {
                        damage = 6,
                        creationFrame = 33,
                        lifetime = 1,
                        size = new Vector2(80, 112),
                        offset = new Vector2(48, -16),
                        knockback = new Knockback(16, 0f, 55, true),
                        comboScaling = 0.5f,
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.Launcher },
                        hitpause = 6,
                        hitParticle = GenericParticles.HitLightDirectional(45)
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(33, new Vector2(4, -7f))
                },
                attackUpdate = a =>
                {
                    if (attackTimer < a.hitboxes[0].creationFrame)
                    {
                        dontPushPlayer = true;
                        lastPlayerHit.hitstunTimer = 2;
                        lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, position + new Vector2(64 * direction, -24), 0.5f);
                        lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                        lastPlayerHit.direction = -direction;
                        lastPlayerHit.CollisionCorrection();
                    }
                }
            });

            //Back Throw
            attacks.Add(new Attack(new Animation("Characters/Jet/att_BackThrow", 7, 4, new Vector2(64), false)
            {
                sounds = new AnimationSound[]
                {
                    new AnimationSound("Sounds/Characters/Common/swishmed", 4)
                }
            })
            {
                name = "Back Throw",
                groundedness = 1,
                cancelLevel = 3,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 15,
                        lifetime = 1,
                        size = new Vector2(48, 48),
                        offset = new Vector2(32, 0),
                        knockback = new Knockback(24, 0f, 15, true),
                        hitpause = 4,
                        hitSound = "",
                        hitParticle = null
                    }
                },
                pushers = new Pusher[]
                {
                    new Pusher(11, new Vector2(8, 0))
                },
                attackUpdate = a =>
                {
                    if (scene.playerInCorner == this) position.X -= 4 * direction;
                    if (attackTimer < attack.hitboxes[0].creationFrame)
                    {
                        dontPushPlayer = true;
                        Vector2 enemyTargetPos = position;
                        if (animation.currentFrame == 1 || animation.currentFrame == 2) enemyTargetPos = new Vector2(position.X + (-80 * direction), position.Y - 32);
                        if (animation.currentFrame == 3) enemyTargetPos = new Vector2(position.X + (-32 * direction), position.Y - 40);
                        if (animation.currentFrame == 4) enemyTargetPos = new Vector2(position.X + (64 * direction), position.Y - 40);
                        if (!hasHitPlayer)
                        {
                            lastPlayerHit.velocity = new Vector2(0, -lastPlayerHit.gravity);
                            lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, enemyTargetPos, 0.5f);
                            lastPlayerHit.direction = direction;
                            lastPlayerHit.CollisionCorrection();
                        }

                        scene.camera.OverridePosition(position + new Vector2(0, -32));
                        if (scene.playerInCorner == this)
                        {
                            position.X -= 48 * direction;
                            scene.playerInCorner = lastPlayerHit;
                        }
                    }
                }
            });

            //Air Grab
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirGrab", 7, 4, new Vector2(64), false), new AttackInput(Controller.Key_Grab))
            {
                name = "Air Grab",
                groundedness = 2,
                cancelLevel = 2,
                landingLag = 12,
                canCancel = false,
                canUse = CanUseFireMoves,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 0,
                        creationFrame = 9,
                        lifetime = 2,
                        size = new Vector2(64, 48),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(0, 0f, 0, false),
                        hitType = HitTypes.AirGrab,
                        postHitEffect = (h, p) => { ApplyBurn(p, 1.5f); },
                        hitpause = 0,
                        hitSound = "",
                        hitParticle = null
                    }
                },
                attackUpdate = a =>
                {
                    if (hasHitPlayer)
                    {
                        while (TouchingTile(collider.Box, direction * 32, -1) || TouchingTile(collider.Box, direction * 96, -1)) position.X -= direction;
                        velocity = new Vector2(0, -gravity);
                        if (TouchingTile(collider.Box, direction * 24, -1)) position.X -= direction;
                        if (TouchingTile(collider.Box, 0, 32)) position.Y -= 2;

                        lastPlayerHit.hitstunTimer = 2;
                        lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, position + new Vector2(80 * direction, -8), 1f);
                        lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                        lastPlayerHit.velocity.X = 0;
                        lastPlayerHit.direction = -direction;
                        lastPlayerHit.CollisionCorrection();

                        if (attackTimer == 12) SetAttack(GetAttackFromName("Air Grab Hit"), false);
                    }
                }
            });
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirGrab_Hit", 10, 3, new Vector2(64), false))
            {
                name = "Air Grab Hit",
                groundedness = 0,
                cancelLevel = 3,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 10,
                        creationFrame = 16,
                        lifetime = 2,
                        size = new Vector2(96, 64),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(32, 0f, 15, false),
                        hitstunProperties = new List<HitstunProperties>() { HitstunProperties.WallBounce },
                        postHitEffect = (h, p) => { ConsumeHeatCharge(); },
                        hitpause = 0,
                        hitParticle = GenericParticles.HitFireDirectional(15)
                    }
                },
                attackUpdate = a =>
                {
                    if (!hasHitPlayer)
                    {
                        velocity = new Vector2(0, -gravity);
                        if (TouchingTile(collider.Box, direction * 24, -1)) position.X -= direction;
                        if (TouchingTile(collider.Box, 0, 32)) position.Y -= 2;

                        lastPlayerHit.hitstunTimer = 2;
                        Vector2 enemyTargetPos = lastPlayerHit.position;
                        if (animation.currentFrame == 1 || animation.currentFrame == 2) enemyTargetPos = new Vector2(position.X + (80 * direction), position.Y - 8);
                        if (animation.currentFrame == 3) enemyTargetPos = new Vector2(position.X + (32 * direction), position.Y - 24);
                        if (animation.currentFrame == 4) enemyTargetPos = new Vector2(position.X - (32 * direction), position.Y - 16);
                        if (animation.currentFrame == 5 || animation.currentFrame == 6) enemyTargetPos = new Vector2(position.X + (64 * direction), position.Y);
                        if (animation.currentFrame == 5 || animation.currentFrame == 6 || animation.currentFrame >= 9) MoveToBack();
                        lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, enemyTargetPos, 0.4f);
                        lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                        lastPlayerHit.velocity.X = 0;
                        lastPlayerHit.direction = -direction;
                        lastPlayerHit.CollisionCorrection();
                    }
                }
            });

            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirGrab_NoFire", 7, 4, new Vector2(64), false), new AttackInput(Controller.Key_Grab))
            {
                name = "Air Grab No Fire",
                groundedness = 2,
                cancelLevel = 2,
                landingLag = 12,
                canCancel = false,
                canUse = a => !CanUseFireMoves(a),
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 0,
                        creationFrame = 9,
                        lifetime = 2,
                        size = new Vector2(64, 48),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(0, 0f, 0, false),
                        hitType = HitTypes.AirGrab,
                        hitpause = 0,
                        hitSound = "",
                        hitParticle = null
                    }
                },
                attackUpdate = a =>
                {
                    if (hasHitPlayer)
                    {
                        while (TouchingTile(collider.Box, direction * 32, -1) || TouchingTile(collider.Box, direction * 96, -1)) position.X -= direction;
                        velocity = new Vector2(0, -gravity);
                        if (TouchingTile(collider.Box, direction * 24, -1)) position.X -= direction;
                        if (TouchingTile(collider.Box, 0, 32)) position.Y -= 2;

                        lastPlayerHit.hitstunTimer = 2;
                        lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, position + new Vector2(80 * direction, -8), 1f);
                        lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                        lastPlayerHit.velocity.X = 0;
                        lastPlayerHit.direction = -direction;
                        lastPlayerHit.CollisionCorrection();

                        if (attackTimer == 12) SetAttack(GetAttackFromName("Air Grab Hit No Fire"), false);
                    }
                }
            });
            attacks.Add(new Attack(new Animation("Characters/Jet/att_AirGrab_Hit_NoFire", 10, 3, new Vector2(64), false))
            {
                name = "Air Grab Hit No Fire",
                groundedness = 0,
                cancelLevel = 3,
                hitboxes = new HitboxSpawner[]
                {
                    new HitboxSpawner(this)
                    {
                        damage = 8,
                        creationFrame = 16,
                        lifetime = 2,
                        size = new Vector2(96, 64),
                        offset = new Vector2(48, -8),
                        knockback = new Knockback(18, 0f, 20, false),
                        hitpause = 0,
                        hitSound = "",
                        hitParticle = null
                    }
                },
                attackUpdate = a =>
                {
                    if (!hasHitPlayer)
                    {
                        velocity = new Vector2(0, -gravity);
                        if (TouchingTile(collider.Box, direction * 24, -1)) position.X -= direction;
                        if (TouchingTile(collider.Box, 0, 32)) position.Y -= 2;

                        lastPlayerHit.hitstunTimer = 2;
                        Vector2 enemyTargetPos = lastPlayerHit.position;
                        if (animation.currentFrame == 1 || animation.currentFrame == 2) enemyTargetPos = new Vector2(position.X + (80 * direction), position.Y - 8);
                        if (animation.currentFrame == 3) enemyTargetPos = new Vector2(position.X + (32 * direction), position.Y - 24);
                        if (animation.currentFrame == 4) enemyTargetPos = new Vector2(position.X - (32 * direction), position.Y - 16);
                        if (animation.currentFrame == 5 || animation.currentFrame == 6) enemyTargetPos = new Vector2(position.X + (64 * direction), position.Y);
                        if (animation.currentFrame == 5 || animation.currentFrame == 6 || animation.currentFrame >= 9) MoveToBack();
                        lastPlayerHit.position = Vector2.Lerp(lastPlayerHit.position, enemyTargetPos, 0.4f);
                        lastPlayerHit.velocity.Y = -lastPlayerHit.gravity;
                        lastPlayerHit.velocity.X = 0;
                        lastPlayerHit.direction = -direction;
                        lastPlayerHit.CollisionCorrection();
                    }
                }
            });
        }

        private bool CanUseFireMoves(Attack a) => heatCharges > 0 && heatCooldown <= 0;

        public override void Update()
        {
            if (scene is InGameTraining t)
            {
                if (heatSettings[t.characterSettings["JetHeatSetting"]] == "No Cooldown") heatCooldown = 0;
                if (heatSettings[t.characterSettings["JetHeatSetting"]] == "Burnt Out" && heatCooldown < 24) heatCooldown += 8;
            }

            if (lastPlayerHit == null || (lastPlayerHit.comboCounter == 0 && lastPlayerHit.state != States.Hitstun) || heatCooldown > 0)
            {
                if (heatCharges == 0) for (int i = 0; i < 20; i++)
                        new Particle(new Animation("Particles/Red/prt_Steam", 5, 4, new Vector2(16), false),
                            position + new Vector2(game.random.Next(-64, 32) * direction, game.random.Next(-80, 32)), 20, Vector2.Zero)
                        { drawLayer = drawLayer - 0.000001f };
                heatCharges = 3;
            }

            if (heatCooldown > 0 && !(attackTimer > 0 && attack.name == "Burnout Grab"))
            {
                heatCooldown--;
                if (heatCooldown % 8 == 0) for (int i = 0; i < 4; i++)
                        new Particle(new Animation("Particles/Red/prt_Steam", 5, 4, new Vector2(16), false),
                            position + new Vector2(game.random.Next(-64, 32) * direction, game.random.Next(-80, 32)), 20, Vector2.Zero)
                        { drawLayer = drawLayer + (game.random.Next(2) == 0 ? 0.000001f : -0.000001f) };
            }

            base.Update();

            fireParticleTimer++;
            if ((heatCharges == 0 || (attackTimer > 0 && attack.name == "Burnout Grab")) && fireParticleTimer % 4 == 0)
            {
                fireParticleTimer = 0;
                RectangleF r = hurtboxes[game.random.Next(hurtboxes.Count)].Box;
                int x = game.random.Next((int)r.Width + 1) + (int)r.X;
                int y = game.random.Next((int)r.Height + 1) + (int)r.Y - 16;
                new Particle(new Animation("Particles/Red/prt_Ember1", 5, 3, new Vector2(16), false), new Vector2(x, y), 15, Vector2.Zero) { drawLayer = drawLayer + 0.00001f };
            }
        }

        void PaletteBasedAfterImage()
        {
            Texture2D tex = Game.LoadAsset<Texture2D>("Characters/Jet/Palette" + currentPalette);
            Color[] c = new Color[tex.Width];
            tex.GetData(c);
            new AfterImage(this, 30, c[c.Length - 2], 0.9f);
        }

        protected override void OverrideHurtboxes()
        {
            if (state == States.Idle)
            {
                hurtboxes.Clear();
                AddHurtbox(40, 16, -16, -36);       //Head
                AddHurtbox(56, 32, -16, -12);       //Chest
                AddHurtbox(72, 32, -16, 20);        //Legs
                AddHurtbox(88, 32, -8, 52);         //Feet
            }

            if (state == States.Crouch || state == States.JumpSquat || state == States.LandingLag)
            {
                hurtboxes.Clear();
                AddHurtbox(40, 16, 0, -4);          //Head
                AddHurtbox(64, 32, 4, 20);          //Chest
                AddHurtbox(96, 32, -4, 52);         //Legs
            }

            if (state == States.Walk)
            {
                hurtboxes.Clear();
                AddHurtbox(40, 20, -4, -38);        //Head
                AddHurtbox(56, 32, -8, -12);        //Chest
                AddHurtbox(64, 32, -8, 20);         //Legs
                AddHurtbox(80, 32, -4, 52);         //Feet
            }

            if (state == States.Run)
            {
                hurtboxes.Clear();
                if (dashTimer > 1)
                {
                    AddHurtbox(40, 16, -4, -28);    //Head
                    AddHurtbox(56, 24, -8, -8);     //Chest
                    AddHurtbox(72, 32, -16, 20);    //Legs
                    AddHurtbox(88, 32, -16, 52);    //Feet
                }
                else
                {
                    AddHurtbox(40, 16, 20, -28);    //Head
                    AddHurtbox(56, 24, 16, -8);     //Chest
                    AddHurtbox(72, 32, 8, 20);      //Legs
                    AddHurtbox(88, 32, 8, 52);      //Feet
                }
            }

            if (state == States.Backdash)
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, -16, -36);       //Head
                    AddHurtbox(56, 32, -16, -12);       //Chest
                    AddHurtbox(56, 32, 0, 20);          //Legs
                    AddHurtbox(72, 32, 12, 52);         //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 16, -12, -36);       //Head
                    AddHurtbox(56, 32, -8, -12);        //Chest
                    AddHurtbox(56, 32, 0, 20);          //Legs
                    AddHurtbox(72, 32, 8, 52);          //Feet
                }
                else if (animation.currentFrame <= 4)
                {
                    AddHurtbox(40, 16, -4, -36);        //Head
                    AddHurtbox(56, 32, -8, -12);        //Chest
                    AddHurtbox(72, 32, -16, 20);        //Legs
                    AddHurtbox(88, 32, -8, 52);         //Feet
                }
            }

            if (state == States.AirIdle || state == States.AirStall)
            {
                hurtboxes.Clear();
                AddHurtbox(40, 16, -4, -48);        //Head
                AddHurtbox(56, 36, -4, -22);        //Chest
                AddHurtbox(56, 40, -4, 16);         //Legs
                AddHurtbox(48, 24, 0, 48);          //Feet
            }

            if (state == States.Guard)
            {
                hurtboxes.Clear();
                AddHurtbox(40, 16, 4, -36);     //Head
                AddHurtbox(52, 32, 2, -12);     //Chest
                AddHurtbox(64, 32, 0, 20);      //Legs
                AddHurtbox(80, 32, -0, 52);     //Feet
            }
            if (state == States.LowGuard)
            {
                hurtboxes.Clear();
                AddHurtbox(40, 16, 0, -4);      //Head
                AddHurtbox(64, 32, 4, 20);      //Chest
                AddHurtbox(96, 32, -4, 52);     //Legs
            }

            if (state == States.Attack && attack.name == "Light")
            {
                hurtboxes.Clear();
                if (animation.currentFrame <= 2)
                {
                    AddHurtbox(40, 16, -8, -36);    //Head
                    AddHurtbox(56, 32, -8, -12);    //Chest
                    AddHurtbox(72, 32, -8, 20);     //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
                else if (animation.currentFrame <= 6)
                {
                    AddHurtbox(40, 16, 12, -36);    //Head
                    AddHurtbox(32, 24, 60, -16);    //Arm
                    AddHurtbox(56, 32, 16, -12);    //Chest
                    AddHurtbox(64, 32, 8, 20);      //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 16, 4, -36);     //Head
                    AddHurtbox(24, 24, 40, -12);    //Arm
                    AddHurtbox(56, 32, 0, -12);     //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(40, 16, -4, -36);    //Head
                    AddHurtbox(56, 32, -4, -12);    //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
            }

            if (state == States.Attack && attack.name == "Light 2")
            {
                hurtboxes.Clear();
                if (animation.currentFrame <= 2)
                {
                    AddHurtbox(40, 16, -16, -36);   //Head
                    AddHurtbox(48, 32, -16, -12);   //Chest
                    AddHurtbox(56, 32, -12, 20);    //Legs
                    AddHurtbox(80, 32, -8, 52);     //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(40, 16, 12, -36);    //Head
                    AddHurtbox(40, 32, 52, -12);    //Arm
                    AddHurtbox(56, 32, 4, -12);     //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(40, 16, 12, -36);    //Head
                    AddHurtbox(24, 32, 44, -12);    //Arm
                    AddHurtbox(56, 32, 4, -12);     //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(88, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame <= 6)
                {
                    AddHurtbox(40, 16, 0, -36);     //Head
                    AddHurtbox(64, 32, 4, -12);     //Chest
                    AddHurtbox(60, 32, -2, 20);     //Legs
                    AddHurtbox(88, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 16, 0, -36);     //Head
                    AddHurtbox(56, 32, 0, -12);     //Chest
                    AddHurtbox(60, 32, -2, 20);     //Legs
                    AddHurtbox(88, 32, -4, 52);     //Feet
                }
            }

            if (state == States.Attack && attack.name == "Light 3")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 16, 24, -28);        //Head
                    AddHurtbox(48, 24, 20, -8);         //Chest
                    AddHurtbox(48, 32, 20, 20);         //Legs
                    AddHurtbox(64, 32, 12, 52);         //Feet
                }
                else if (animation.currentFrame <= 4)
                {
                    AddHurtbox(40, 16, 24, -36);        //Head
                    AddHurtbox(96, 24, 48, -16);        //Chest
                    AddHurtbox(60, 24, 32, 8);          //Legs
                    AddHurtbox(32, 48, 28, 44);         //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(40, 16, 20, -36);        //Head
                    AddHurtbox(88, 24, 40, -16);        //Chest
                    AddHurtbox(60, 24, 32, 8);          //Legs
                    AddHurtbox(32, 48, 28, 44);         //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(40, 16, 16, -36);        //Head
                    AddHurtbox(48, 24, 16, -16);        //Chest
                    AddHurtbox(48, 32, 28, 12);          //Legs
                    AddHurtbox(40, 40, 28, 48);         //Feet
                }
                else if (animation.currentFrame <= 8)
                {
                    AddHurtbox(40, 16, 8, -36);         //Head
                    AddHurtbox(56, 32, 4, -12);         //Chest
                    AddHurtbox(56, 32, 0, 20);         //Legs
                    AddHurtbox(80, 32, 0, 52);          //Feet
                }
            }

            if (state == States.Attack && attack.name == "Light 4")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(24, 32, 60, -8);         //Head
                    AddHurtbox(48, 40, 24, -16);        //Chest
                    AddHurtbox(48, 32, 4, 20);          //Legs
                    AddHurtbox(32, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(28, 36, -54, -12);       //Feet
                    AddHurtbox(48, 48, -16, -32);       //Legs
                    AddHurtbox(32, 48, 24, 0);          //Chest
                    AddHurtbox(32, 32, -8, 20);         //Head
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(12, 36, -46, -12);       //Head
                    AddHurtbox(48, 36, -16, -6);        //Chest
                    AddHurtbox(48, 40, -16, -44);       //Legs
                    AddHurtbox(40, 64, 28, -16);        //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(24, 28, -20, -34);       //Head
                    AddHurtbox(32, 40, -8, -8);         //Chest
                    AddHurtbox(40, 96, 28, -16);        //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(32, 16, 0, -44);         //Head
                    AddHurtbox(56, 32, 8, -20);         //Chest
                    AddHurtbox(56, 32, 16, 12);         //Legs
                    AddHurtbox(40, 40, 48, 32);         //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 16, 8, -48);        //Head
                    AddHurtbox(56, 36, 12, -22);        //Chest
                    AddHurtbox(56, 40, 12, 16);         //Legs
                    AddHurtbox(48, 28, 8, 50);          //Feet
                }
            }

            if (state == States.Attack && attack.name == "Crouch Light")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 8, -8, 0);       //Head
                    AddHurtbox(64, 32, -4, 20);     //Chest
                    AddHurtbox(84, 32, -10, 52);    //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(32, 16, -40, -4);    //Head
                    AddHurtbox(64, 16, -24, 12);    //Chest
                    AddHurtbox(56, 24, 56, 56);     //Front Leg
                    AddHurtbox(24, 12, 40, 38);     //Front Knee
                    AddHurtbox(80, 48, -12, 44);    //Back Leg
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(32, 16, -24, -4);    //Head
                    AddHurtbox(64, 16, -16, 12);    //Chest
                    AddHurtbox(56, 24, 56, 56);     //Front Leg
                    AddHurtbox(24, 12, 40, 38);     //Front Knee
                    AddHurtbox(80, 48, -12, 44);    //Back Leg
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(32, 16, -16, -4);    //Head
                    AddHurtbox(64, 16, -8, 12);     //Chest
                    AddHurtbox(40, 24, 60, 56);     //Front Leg
                    AddHurtbox(16, 12, 48, 38);     //Front Knee
                    AddHurtbox(88, 48, -4, 44);     //Back Leg
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(36, 16, -6, -4);     //Head
                    AddHurtbox(68, 32, -2, 20);     //Chest
                    AddHurtbox(96, 32, 0, 52);      //Legs
                }
            }

            if (state == States.Attack && attack.name == "Forward Light")
            {
                hurtboxes.Clear();
                if (animation.currentFrame <= 3)
                {
                    AddHurtbox(40, 16, -24, -36);   //Head
                    AddHurtbox(52, 32, -22, -12);   //Chest
                    AddHurtbox(72, 32, -20, 20);    //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(40, 16, 0, -36);     //Head
                    AddHurtbox(56, 32, -8, -12);    //Chest
                    AddHurtbox(64, 32, -12, 20);    //Legs
                    AddHurtbox(88, 32, -24, 52);    //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(40, 16, 4, -36);     //Head
                    AddHurtbox(96, 32, 16, -12);    //Chest
                    AddHurtbox(80, 32, -4, 20);     //Legs
                    AddHurtbox(96, 32, -12, 52);    //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(40, 16, 12, -28);    //Head
                    AddHurtbox(96, 24, 16, -8);     //Chest
                    AddHurtbox(80, 32, 0, 20);      //Legs
                    AddHurtbox(96, 32, 4, 52);      //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 16, 4, -20);     //Head
                    AddHurtbox(80, 24, 16, 0);      //Chest
                    AddHurtbox(64, 24, 4, 24);      //Legs
                    AddHurtbox(96, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(40, 16, 8, -20);     //Head
                    AddHurtbox(64, 24, 8, 0);       //Chest
                    AddHurtbox(72, 24, 4, 24);      //Legs
                    AddHurtbox(96, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame <= 10)
                {
                    AddHurtbox(40, 16, 0, -28);     //Head
                    AddHurtbox(72, 24, 0, -8);       //Chest
                    AddHurtbox(72, 32, -4, 20);      //Legs
                    AddHurtbox(96, 32, 0, 52);      //Feet
                }
            }

            if (state == States.Attack && attack.name == "Medium")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1 || animation.currentFrame == 9)
                {
                    AddHurtbox(40, 16, 0, -36);     //Head
                    AddHurtbox(48, 32, 0, -12);     //Chest
                    AddHurtbox(64, 32, -4, 20);     //Legs
                    AddHurtbox(80, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 16, 8, -36);     //Head
                    AddHurtbox(48, 32, 4, -12);     //Chest
                    AddHurtbox(48, 32, 4, 20);      //Legs
                    AddHurtbox(68, 32, 2, 52);      //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(40, 16, 8, -36);     //Head
                    AddHurtbox(48, 32, 12, -12);    //Chest
                    AddHurtbox(56, 32, 24, 20);     //Legs
                    AddHurtbox(40, 32, 16, 52);     //Feet
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(40, 16, 8, -32);     //Head
                    AddHurtbox(56, 28, 24, -10);    //Chest
                    AddHurtbox(56, 32, 80, 0);      //Front Leg
                    AddHurtbox(44, 32, 30, 20);     //Legs
                    AddHurtbox(40, 32, 32, 52);     //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(40, 16, 8, -32);     //Head
                    AddHurtbox(56, 28, 24, -10);    //Chest
                    AddHurtbox(44, 40, 74, -8);     //Front Leg
                    AddHurtbox(44, 32, 30, 20);     //Legs
                    AddHurtbox(40, 32, 32, 52);     //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(40, 16, 12, -32);    //Head
                    AddHurtbox(56, 28, 24, -10);    //Chest
                    AddHurtbox(16, 40, 60, -8);     //Front Leg
                    AddHurtbox(44, 32, 30, 20);     //Legs
                    AddHurtbox(40, 32, 32, 52);     //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 16, 12, -32);    //Head
                    AddHurtbox(48, 28, 16, -10);    //Chest
                    AddHurtbox(44, 32, 22, 20);     //Legs
                    AddHurtbox(40, 32, 24, 52);     //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(40, 16, 8, -36);     //Head
                    AddHurtbox(48, 32, 12, -12);    //Chest
                    AddHurtbox(48, 32, 12, 20);     //Legs
                    AddHurtbox(56, 32, 12, 52);     //Feet
                }
            }

            if (state == States.Attack && attack.name == "Crouch Medium")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 12, 8, -2);      //Head
                    AddHurtbox(56, 32, 12, 20);     //Chest
                    AddHurtbox(88, 32, 0, 52);      //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(36, 8, 22, 0);       //Head
                    AddHurtbox(56, 32, 24, 20);     //Chest
                    AddHurtbox(80, 32, 12, 52);     //Legs
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(32, 8, 28, 8);       //Head
                    AddHurtbox(64, 32, 40, 28);     //Chest
                    AddHurtbox(48, 24, 32, 56);     //Legs
                }
                else if (animation.currentFrame <= 5)
                {
                    AddHurtbox(32, 12, 36, 6);      //Head
                    AddHurtbox(48, 24, 44, 24);     //Chest
                    AddHurtbox(96, 32, 72, 52);     //Legs
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(32, 16, 32, 4);      //Head
                    AddHurtbox(56, 24, 40, 24);     //Chest
                    AddHurtbox(80, 32, 56, 52);     //Legs
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(32, 16, 24, 4);      //Head
                    AddHurtbox(48, 24, 36, 24);     //Chest
                    AddHurtbox(56, 32, 44, 52);     //Legs
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(36, 8, 14, 0);       //Head
                    AddHurtbox(52, 32, 22, 20);     //Chest
                    AddHurtbox(64, 32, 24, 52);     //Legs
                }
                else if (animation.currentFrame == 9)
                {
                    AddHurtbox(40, 12, 12, -2);       //Head
                    AddHurtbox(56, 32, 16, 20);     //Chest
                    AddHurtbox(76, 32, 14, 52);     //Legs
                }
            }

            if (state == States.Attack && (attack.name == "Heavy" || attack.name == "Heavy No Fire"))
            {
                hurtboxes.Clear();
                if (animation.currentFrame <= 2)
                {
                    AddHurtbox(40, 16, -16, -36);   //Head
                    AddHurtbox(56, 32, -16, -12);   //Chest
                    AddHurtbox(64, 32, -12, 20);    //Legs
                    AddHurtbox(88, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(40, 8, -8, -32);     //Head
                    AddHurtbox(48, 32, -4, -12);    //Chest
                    AddHurtbox(64, 32, -4, 20);     //Legs
                    AddHurtbox(88, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(40, 8, 20, -32);     //Head
                    AddHurtbox(32, 24, 60, -4);     //Arm
                    AddHurtbox(56, 32, 16, -12);    //Chest
                    AddHurtbox(72, 32, 4, 20);      //Legs
                    AddHurtbox(96, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(40, 16, 20, -36);    //Head
                    AddHurtbox(32, 24, 60, -24);    //Arm
                    AddHurtbox(56, 32, 16, -12);    //Chest
                    AddHurtbox(64, 32, 4, 20);      //Legs
                    AddHurtbox(96, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(40, 16, 12, -36);    //Head
                    AddHurtbox(56, 32, 12, -12);    //Chest
                    AddHurtbox(64, 32, 4, 20);      //Legs
                    AddHurtbox(96, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 16, 4, -36);     //Head
                    AddHurtbox(48, 32, 8, -12);     //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(96, 32, -4, 52);     //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(40, 16, -4, -36);    //Head
                    AddHurtbox(56, 32, -4, -12);    //Chest
                    AddHurtbox(64, 32, -8, 20);      //Legs
                    AddHurtbox(88, 32, -8, 52);     //Feet
                }
            }

            if (state == States.Attack && (attack.name == "Crouch Heavy" || attack.name == "Crouch Heavy No Fire"))
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(72, 32, 4, 52);          //Legs
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(24, 16, -20, -48);       //Head
                    AddHurtbox(40, 36, -8, -22);        //Chest
                    AddHurtbox(64, 40, 0, 16);          //Legs
                    AddHurtbox(48, 24, 0, 48);          //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(24, 36, -40, 0);         //Head
                    AddHurtbox(40, 48, -8, 8);          //Chest
                    AddHurtbox(48, 64, 24, -48);        //Front Leg
                    AddHurtbox(48, 32, 44, 0);          //Back Leg
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(32, 32, 0, 20);          //Head
                    AddHurtbox(32, 48, 24, 0);          //Chest
                    AddHurtbox(40, 48, -8, -32);        //Front Leg
                    AddHurtbox(48, 32, 36, -28);        //Back Leg
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(32, 32, 32, -8);         //Head
                    AddHurtbox(64, 40, 0, -36);         //Chest
                    AddHurtbox(64, 32, -24, 0);         //Front Leg
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(32, 32, 32, -24);        //Head
                    AddHurtbox(64, 40, 0, -36);         //Chest
                    AddHurtbox(64, 32, -24, 0);         //Front Leg
                }
                else if (animation.currentFrame == 9)
                {
                    AddHurtbox(56, 40, -12, -24);       //Chest
                    AddHurtbox(48, 40, -32, 16);        //Front Leg
                }
                else if (animation.currentFrame == 10)
                {
                    AddHurtbox(32, 16, -16, -36);       //Head
                    AddHurtbox(56, 24, -8, -16);        //Chest
                    AddHurtbox(48, 40, -16, 16);        //Legs
                    AddHurtbox(24, 20, -28, 46);        //Feet
                }
                else if (animation.currentFrame <= 13)
                {
                    AddHurtbox(32, 16, -4, -36);        //Head
                    AddHurtbox(64, 24, 4, -16);         //Chest
                    AddHurtbox(112, 40, 24, 16);        //Legs
                    AddHurtbox(32, 24, -16, 48);        //Feet
                }
                else if (animation.currentFrame == 14)
                {
                    AddHurtbox(32, 16, -4, -36);        //Head
                    AddHurtbox(64, 24, 4, -16);         //Chest
                    AddHurtbox(80, 40, 16, 16);        //Legs
                    AddHurtbox(32, 24, -8, 48);        //Feet
                }
                else if (animation.currentFrame == 15)
                {
                    AddHurtbox(40, 16, -4, -48);        //Head
                    AddHurtbox(56, 36, -4, -22);        //Chest
                    AddHurtbox(56, 40, 4, 16);         //Legs
                    AddHurtbox(48, 24, 8, 48);          //Feet
                }
            }

            if (state == States.Attack && attack.name == "Air Light")
            {
                hurtboxes.Clear();
                if (animation.currentFrame <= 2)
                {
                    AddHurtbox(40, 16, -4, -48);        //Head
                    AddHurtbox(64, 36, 0, -22);         //Chest
                    AddHurtbox(56, 40, -4, 16);         //Legs
                    AddHurtbox(48, 24, 0, 48);          //Feet
                }
                else if (animation.currentFrame <= 6)
                {
                    AddHurtbox(36, 20, 18, -26);        //Head
                    AddHurtbox(24, 24, 44, 16);         //Arm
                    AddHurtbox(56, 32, 16, 0);          //Chest
                    AddHurtbox(16, 32, 0, 16);          //Legs
                    AddHurtbox(48, 56, -32, 16);        //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(36, 20, 10, -30);        //Head
                    AddHurtbox(32, 20, 32, 2);          //Arm
                    AddHurtbox(32, 24, 4, -8);          //Chest
                    AddHurtbox(24, 24, 0, 16);          //Legs
                    AddHurtbox(40, 48, -32, 16);        //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(40, 16, 4, -40);        //Head
                    AddHurtbox(56, 36, -4, -14);        //Chest
                    AddHurtbox(48, 32, -8, 20);         //Legs
                    AddHurtbox(40, 24, -8, 48);         //Feet
                }
                else if (animation.currentFrame == 9)
                {
                    AddHurtbox(40, 16, -4, -48);        //Head
                    AddHurtbox(64, 36, 0, -22);         //Chest
                    AddHurtbox(56, 40, -4, 16);         //Legs
                    AddHurtbox(48, 24, 0, 48);          //Feet
                }
            }

            if (state == States.Attack && attack.name == "Air Medium")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 24, -4, -40);    //Head
                    AddHurtbox(56, 32, -4, -12);    //Chest
                    AddHurtbox(64, 32, -16, 20);    //Legs
                    AddHurtbox(48, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 24, -4, -40);    //Head
                    AddHurtbox(56, 32, -8, -12);    //Chest
                    AddHurtbox(64, 32, -20, 20);    //Legs
                    AddHurtbox(32, 36, -8, 54);     //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(40, 20, -4, -38);    //Head
                    AddHurtbox(48, 32, -8, -12);    //Chest
                    AddHurtbox(64, 32, 4, 20);      //Legs
                    AddHurtbox(32, 36, -8, 54);     //Feet
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(40, 16, -8, -36);    //Head
                    AddHurtbox(52, 32, -6, -12);    //Chest
                    AddHurtbox(96, 32, 24, 20);     //Legs
                    AddHurtbox(32, 36, 4, 54);      //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(56, 36, -8, -14);    //Chest
                    AddHurtbox(36, 32, 38, 0);      //Legs
                    AddHurtbox(40, 68, 0, 38);      //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(40, 36, -16, -14);   //Chest
                    AddHurtbox(48, 32, -8, 20);     //Legs
                    AddHurtbox(40, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(40, 36, -12, -14);   //Chest
                    AddHurtbox(48, 32, -8, 20);     //Legs
                    AddHurtbox(40, 24, -16, 48);    //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(40, 16, -4, -36);    //Head
                    AddHurtbox(72, 32, 8, -12);     //Chest
                    AddHurtbox(112, 32, 24, 20);    //Legs
                    AddHurtbox(32, 32, -12, 52);    //Feet
                }
                else if (animation.currentFrame == 9 || animation.currentFrame == 10)
                {
                    AddHurtbox(40, 16, -4, -36);    //Head
                    AddHurtbox(72, 28, 8, -14);     //Chest
                    AddHurtbox(112, 28, 24, 14);    //Legs
                    AddHurtbox(32, 40, 0, 48);      //Feet
                }
                else if (animation.currentFrame == 11)
                {
                    AddHurtbox(40, 16, -4, -36);    //Head
                    AddHurtbox(64, 28, 4, -14);     //Chest
                    AddHurtbox(80, 32, 24, 16);     //Legs
                    AddHurtbox(32, 28, 4, 46);      //Feet
                }
                else if (animation.currentFrame == 12)
                {
                    AddHurtbox(40, 16, -4, -40);    //Head
                    AddHurtbox(56, 32, -4, -16);    //Chest
                    AddHurtbox(48, 36, 8, 18);      //Legs
                    AddHurtbox(52, 24, 10, 48);     //Feet
                }
            }

            if (state == States.Attack && attack.name == "Air Down Heavy")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(32, 16, -12, -48);       //Head
                    AddHurtbox(40, 36, -8, -22);        //Chest
                    AddHurtbox(48, 48, -16, 20);          //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(24, 16, -20, -48);       //Head
                    AddHurtbox(40, 36, -8, -22);        //Chest
                    AddHurtbox(64, 40, 0, 16);          //Legs
                    AddHurtbox(48, 24, 0, 48);          //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(24, 36, -40, 0);         //Head
                    AddHurtbox(40, 48, -8, 8);          //Chest
                    AddHurtbox(48, 64, 24, -48);        //Front Leg
                    AddHurtbox(48, 32, 44, 0);          //Back Leg
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(32, 32, 0, 20);          //Head
                    AddHurtbox(32, 48, 24, 0);          //Chest
                    AddHurtbox(40, 48, -8, -32);        //Front Leg
                    AddHurtbox(48, 32, 36, -28);        //Back Leg
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(32, 32, 32, -8);         //Head
                    AddHurtbox(64, 40, 0, -36);         //Chest
                    AddHurtbox(64, 32, -24, 0);         //Front Leg
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(96, 40, -12, -24);       //Chest
                    AddHurtbox(48, 40, -32, 16);        //Front Leg
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(24, 24, 24, -40);        //Head
                    AddHurtbox(56, 40, -12, -24);       //Chest
                    AddHurtbox(48, 48, -24, 20);        //Legs
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(32, 20, 4, -50);         //Head
                    AddHurtbox(40, 40, -8, -24);        //Chest
                    AddHurtbox(48, 48, -8, 20);         //Legs
                }
                else if (animation.currentFrame == 9)
                {
                    AddHurtbox(32, 20, -4, -50);        //Head
                    AddHurtbox(56, 40, 0, -24);         //Chest
                    AddHurtbox(48, 48, -8, 20);         //Legs
                }
            }
            
            if (state == States.Attack && (attack.name == "Air Heavy" || attack.name == "Air Heavy No Fire"))
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(48, 44, 16, -18);        //Chest
                    AddHurtbox(48, 32, 4, 20);          //Legs
                    AddHurtbox(32, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(24, 32, 60, -8);         //Head
                    AddHurtbox(48, 40, 24, -16);        //Chest
                    AddHurtbox(48, 32, 4, 20);          //Legs
                    AddHurtbox(32, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(28, 36, -54, -12);       //Feet
                    AddHurtbox(48, 48, -16, -32);       //Legs
                    AddHurtbox(32, 48, 24, 0);          //Chest
                    AddHurtbox(32, 32, -8, 20);         //Head
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(12, 36, -46, -12);       //Head
                    AddHurtbox(48, 36, -16, -6);        //Chest
                    AddHurtbox(48, 40, -16, -44);       //Legs
                    AddHurtbox(40, 64, 28, -16);        //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(24, 28, -20, -34);       //Head
                    AddHurtbox(32, 40, -8, -8);         //Chest
                    AddHurtbox(40, 96, 28, -16);        //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(32, 16, 0, -44);         //Head
                    AddHurtbox(56, 32, 8, -20);         //Chest
                    AddHurtbox(56, 32, 16, 12);         //Legs
                    AddHurtbox(40, 40, 48, 32);         //Feet
                }
                else if (animation.currentFrame <= 10)
                {
                    AddHurtbox(40, 16, 8, -48);         //Head
                    AddHurtbox(56, 36, 8, -22);         //Chest
                    AddHurtbox(40, 40, 4, 16);         //Legs
                    AddHurtbox(32, 28, -8, 50);         //Feet
                }
            }

            if (state == States.Attack && (attack.name == "Jet Flurry" || attack.name == "Jet Flurry No Fire"))
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, -12, -28);       //Head
                    AddHurtbox(48, 24, -12, -8);        //Chest
                    AddHurtbox(56, 32, -8, 20);         //Legs
                    AddHurtbox(72, 32, -8, 52);         //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 16, -16, -28);       //Head
                    AddHurtbox(48, 24, -20, -8);        //Chest
                    AddHurtbox(56, 32, -12, 20);        //Legs
                    AddHurtbox(72, 32, -8, 52);         //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(40, 16, 0, -28);         //Head
                    AddHurtbox(56, 24, -4, -8);         //Chest
                    AddHurtbox(64, 32, -8, 20);         //Legs
                    AddHurtbox(72, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame <= 8)
                {
                    AddHurtbox(40, 16, 0, -28);         //Head
                    AddHurtbox(56, 24, -4, -8);         //Chest
                    AddHurtbox(56, 32, 48, -8);         //Arm
                    AddHurtbox(64, 32, -8, 20);         //Legs
                    AddHurtbox(72, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame == 9)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame <= 12)
                {
                    AddHurtbox(40, 16, 16, -28);        //Head
                    AddHurtbox(56, 24, 12, -8);         //Chest
                    AddHurtbox(32, 48, 48, -40);        //Arm
                    AddHurtbox(64, 32, 12, 20);         //Legs
                    AddHurtbox(72, 32, 4, 52);          //Feet
                }
                else if (animation.currentFrame <= 14)
                {
                    AddHurtbox(40, 16, 16, -28);        //Head
                    AddHurtbox(56, 24, 12, -8);         //Chest
                    AddHurtbox(64, 32, 12, 20);         //Legs
                    AddHurtbox(72, 32, 4, 52);          //Feet
                }
                else if (animation.currentFrame == 15)
                {
                    AddHurtbox(40, 16, -4, -28);        //Head
                    AddHurtbox(48, 24, -4, -8);         //Chest
                    AddHurtbox(56, 32, -8, 20);         //Legs
                    AddHurtbox(72, 32, -8, 52);         //Feet
                }
            }

            if (state == States.Attack && (attack.name == "Front Flip Kick" || attack.name == "Front Flip Kick No Fire"))
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(24, 32, 60, -8);         //Head
                    AddHurtbox(48, 40, 24, -16);        //Chest
                    AddHurtbox(48, 32, 4, 20);          //Legs
                    AddHurtbox(32, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(28, 36, -54, -12);       //Feet
                    AddHurtbox(48, 48, -16, -32);       //Legs
                    AddHurtbox(32, 48, 24, 0);          //Chest
                    AddHurtbox(32, 32, -8, 20);         //Head
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(12, 36, -46, -12);       //Head
                    AddHurtbox(48, 36, -16, -6);        //Chest
                    AddHurtbox(48, 40, -16, -44);       //Legs
                    AddHurtbox(40, 64, 28, -16);        //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(24, 28, -20, -34);       //Head
                    AddHurtbox(32, 40, -8, -8);         //Chest
                    AddHurtbox(40, 96, 28, -16);        //Feet
                }
                else
                {
                    AddHurtbox(48, 28, 8, -10);         //Chest
                    AddHurtbox(48, 64, 56, -8);         //Front Leg
                    AddHurtbox(72, 48, -4, 28);         //Back Leg
                }
            }
            if (state == States.Attack && (attack.name == "Front Flip Kick Land" || attack.name == "Front Flip Kick Land No Fire"))
            {
                hurtboxes.Clear();
                if (animation.currentFrame <= 4)
                {
                    AddHurtbox(72, 40, 32, 24);         //Chest
                    AddHurtbox(64, 32, 60, 56);         //Front Leg
                    AddHurtbox(64, 28, -4, 58);         //Back Leg
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(40, 8, 4, 0);            //Head
                    AddHurtbox(72, 40, 16, 24);         //Chest
                    AddHurtbox(64, 32, 60, 56);         //Front Leg
                    AddHurtbox(64, 28, -4, 58);         //Back Leg
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(40, 12, -12, -10);       //Head
                    AddHurtbox(72, 40, -4, 16);         //Chest
                    AddHurtbox(112, 32, 8, 52);         //Feet
                }
            }


            if (state == States.Attack && attack.name == "Guard Light")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(52, 32, 2, -12);     //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(80, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(56, 32, 16, -24);    //Chest
                    AddHurtbox(80, 36, 20, 10);     //Legs
                    AddHurtbox(72, 40, 8, 48);      //Feet
                }
                else if (animation.currentFrame <= 4)
                {
                    AddHurtbox(56, 36, 24, -26);    //Chest
                    AddHurtbox(80, 36, 24, 10);     //Legs
                    AddHurtbox(72, 40, 12, 48);     //Feet
                }
                else if (animation.currentFrame == 5)
                {
                    AddHurtbox(56, 32, 16, -24);    //Chest
                    AddHurtbox(64, 36, 20, 10);     //Legs
                    AddHurtbox(72, 40, 12, 48);     //Feet
                }
                else if (animation.currentFrame == 6)
                {
                    AddHurtbox(52, 36, 6, -14);     //Chest
                    AddHurtbox(64, 32, 4, 20);      //Legs
                    AddHurtbox(80, 32, 0, 52);      //Feet
                }
            }

            if (state == States.Attack && attack.name == "Guard Heavy")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(40, 16, 0, -4);          //Head
                    AddHurtbox(64, 32, 4, 20);          //Chest
                    AddHurtbox(96, 32, -4, 52);         //Legs
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(24, 16, -24, -36);       //Head
                    AddHurtbox(32, 32, -24, -12);       //Chest
                    AddHurtbox(40, 32, -20, 20);        //Legs
                    AddHurtbox(48, 32, -16, 52);        //Feet
                }
                else if (animation.currentFrame == 3)
                {
                    AddHurtbox(24, 16, -24, -36);       //Head
                    AddHurtbox(40, 32, -20, -12);       //Chest
                    AddHurtbox(48, 32, -16, 20);        //Legs
                    AddHurtbox(48, 32, -16, 52);        //Feet
                }
                else if (animation.currentFrame == 4)
                {
                    AddHurtbox(24, 20, -12, -38);       //Head
                    AddHurtbox(40, 32, -8, -12);        //Chest
                    AddHurtbox(48, 32, -12, 20);        //Legs
                    AddHurtbox(48, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame <= 6)
                {
                    AddHurtbox(40, 32, -8, -12);        //Chest
                    AddHurtbox(48, 32, -12, 20);        //Legs
                    AddHurtbox(48, 32, -12, 52);        //Feet
                }
                else if (animation.currentFrame == 7)
                {
                    AddHurtbox(56, 32, -16, 20);        //Legs
                    AddHurtbox(64, 32, -8, 52);         //Feet
                }
                else if (animation.currentFrame == 8)
                {
                    AddHurtbox(48, 32, 16, -12);        //Head
                    AddHurtbox(72, 32, 8, 20);          //Legs
                    AddHurtbox(80, 32, 0, 52);          //Feet
                }
                else if (animation.currentFrame == 9)
                {
                    AddHurtbox(40, 16, 16, -32);        //Head
                    AddHurtbox(48, 28, 16, -10);        //Chest
                    AddHurtbox(48, 24, 56, 0);          //Arm
                    AddHurtbox(80, 32, 16, 20);         //Legs
                    AddHurtbox(88, 32, 8, 52);          //Feet
                }
                else if (animation.currentFrame == 10)
                {
                    AddHurtbox(40, 20, 12, -22);        //Head
                    AddHurtbox(80, 24, 24, 0);          //Chest
                    AddHurtbox(64, 24, 4, 24);          //Legs
                    AddHurtbox(96, 32, 8, 52);          //Feet
                }
                else if (animation.currentFrame == 11)
                {
                    AddHurtbox(40, 16, 8, -20);         //Head
                    AddHurtbox(80, 24, 16, 0);           //Chest
                    AddHurtbox(72, 24, 4, 24);          //Legs
                    AddHurtbox(96, 32, 0, 52);          //Feet
                }
                else if (animation.currentFrame <= 14)
                {
                    AddHurtbox(40, 16, 0, -28);         //Head
                    AddHurtbox(64, 24, 0, -8);          //Chest
                    AddHurtbox(64, 32, -4, 20);         //Legs
                    AddHurtbox(88, 32, 0, 52);          //Feet
                }
            }

            if (state == States.Attack && attack.name == "Guard Special")
            {
                hurtboxes.Clear();
                if (animation.currentFrame == 1)
                {
                    AddHurtbox(52, 32, 2, -12);     //Chest
                    AddHurtbox(64, 32, 0, 20);      //Legs
                    AddHurtbox(80, 32, 0, 52);      //Feet
                }
                else if (animation.currentFrame == 2)
                {
                    AddHurtbox(40, 16, -8, -36);    //Head
                    AddHurtbox(54, 32, -4, -12);    //Chest
                    AddHurtbox(64, 32, -4, 20);     //Legs
                    AddHurtbox(80, 32, -0, 52);     //Feet
                }
                else if (animation.currentFrame <= 4)
                {
                    AddHurtbox(40, 16, -8, -36);    //Head
                    AddHurtbox(54, 32, -4, -12);    //Chest
                    AddHurtbox(64, 32, -4, 20);     //Legs
                    AddHurtbox(80, 32, -0, 52);     //Feet
                }
                else if (animation.currentFrame <= 7)
                {
                    AddHurtbox(56, 16, 16, -4);     //Chest
                    AddHurtbox(64, 32, 12, 20);     //Legs
                    AddHurtbox(80, 32, 8, 52);      //Feet
                }
                else if (animation.currentFrame <= 9)
                {
                    AddHurtbox(40, 24, 0, -8);      //Chest
                    AddHurtbox(64, 32, 4, 20);      //Legs
                    AddHurtbox(80, 32, 0, 52);      //Feet
                }
            }
        }

        public override void AttackUpdate()
        {
            BasicGrabUpdate(12, 60, new Vector2(80 * direction, -16));
        }

        public override Dictionary<string, object> GetState()
        {
            var state = base.GetState();
            state.Add("uppercutCharge", uppercutCharge);
            state.Add("heatCharges", heatCharges);
            state.Add("heatCooldown", heatCooldown);
            return state;
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            base.LoadState(state);

            uppercutCharge = (int)state["uppercutCharge"];
            heatCharges = (int)state["heatCharges"];
            heatCooldown = (int)state["heatCooldown"];
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (heatCharges == 0 || (attackTimer > 0 && attack.name == "Burnout Grab")) animation.DrawSilhouette(spriteBatch, position, drawLayer + 0.00001f, direction, new Color(255, 80, 0, 80), rotation, drawScale);
            if (heatCooldown > 0 && !(attackTimer > 0 && attack.name == "Burnout Grab")) animation.DrawSilhouette(spriteBatch, position, drawLayer + 0.00001f, direction, new Color(60, 60, 60, Math.Clamp(heatCooldown * 8, 0, 80)), rotation, drawScale);
        }

        void ConsumeHeatCharge()
        {
            if (heatCharges > 0 && !(scene is InGameTraining t && heatSettings[t.characterSettings["JetHeatSetting"]] == "Infinite")) heatCharges--;
        }

        void ApplyBurn(Fighter target, float intensity) => Buff.Apply(target, "Burn", (int)(intensity * 30), (b, p) =>
        {
            RectangleF r = p.hurtboxes[game.random.Next(p.hurtboxes.Count)].Box;
            int x = game.random.Next((int)r.Width - 3) + (int)r.X + 4;
            int y = game.random.Next((int)r.Height - 3) + (int)r.Y - 20;
            if (game.random.Next(2) == 0) new Particle(new Animation("Particles/Red/prt_Ember1", 5, 3, new Vector2(16), false), new Vector2(x, y), 15, Vector2.Zero) { drawLayer = p.drawLayer + 0.00001f };
            else new Particle(new Animation("Particles/Red/prt_Steam", 5, 4, new Vector2(16), false), new Vector2(x, y), 20, Vector2.Zero)
            { drawLayer = p.drawLayer + (game.random.Next(2) == 0 ? 0.000001f : -0.000001f) };
        }, (sb, b, p) =>
        {
            p.animation.DrawSilhouette(sb, p.position, p.drawLayer + 0.0000001f, p.direction, new Color(Color.Orange, 0.2f), p.rotation, p.drawScale);
        });
    }

    class NewHeatMeter : UI.UIElement
    {
        static Animation normalBar = new Animation("Characters/Jet/HeatHealthOverlay", 3, 0, new Vector2(18, 12)) { currentFrame = 1 };
        static Animation heatedBar = new Animation("Characters/Jet/HeatHealthOverlay", 3, 0, new Vector2(18, 12)) { currentFrame = 2 };
        static Animation burnoutBar = new Animation("Characters/Jet/HeatHealthOverlay", 3, 0, new Vector2(18, 12)) { currentFrame = 3 };

        Jet owner;

        public NewHeatMeter(Jet owner) : base("", 960, 160)
        {
            this.owner = owner;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            int direction = owner.ID == 0 ? -1 : 1;

            for (int i = 0; i < owner.heatMaxCharges; i++)
            {
                Animation anim = owner.heatCooldown > 0 ? burnoutBar : owner.heatCharges <= i ? heatedBar : normalBar;

                spritebatch.Draw(anim.spriteSheet, position + new Vector2(128 * direction * (i + 1), 0),
                new Rectangle((anim.currentFrame - 1) * (int)anim.cellSize.X, 0,
                (int)anim.cellSize.X, (int)anim.cellSize.Y),
                Color.White, 0, new Vector2(anim.cellSize.X, anim.cellSize.Y) / 2, 8, direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.42f);
            }
        }
    }
}