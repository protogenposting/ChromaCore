using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using RCArena.Code.Scenes;
using RCArena.Code.Effects;

namespace RCArena.Code.Objects.Players.Characters
{
    public class Kyoki : Fighter
    {
        public override MusicTrack ThemeSong => new MusicTrack("Music/KyokiTheme", 134.4);

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

            drawScale = 0.125f;
        }

        public override void BaseStats()
        {
            ResetStats();

            AutoGeneratePalettes("Characters/Kyoki/Palette", 2);

            healthMax = 350;

            collider = new Collider(this, 48, 80, new Vector2(-24, -12));
            idleHurtbox = new Collider(this, 80, 112, new Vector2(-40, -44));
            crouchHurtbox = new Collider(this, 72, 88, new Vector2(-36, -20));

            idleAnim = new Animation("Characters/Kyoki/Idle", 1, 0);
            crouchAnim = new CompoundAnimation("Characters/Jet/Crouch", 1, 4, 12, 3);
            unCrouchAnim = new Animation("Characters/Jet/UnCrouch", 2, 4, false);
            walkAnim = new CompoundAnimation("Characters/Jet/Walk", 1, 8, 5);
            walkBackAnim = new CompoundAnimation("Characters/Jet/WalkBackward", 1, 8, 8);
            runAnim = new CompoundAnimation("Characters/Jet/Run", 2, 6, 6, 4);
            backdashAnim = new Animation("Characters/Jet/Backdash", 4, 4, false);
            jumpSquatAnim = new Animation("Characters/Jet/JumpSquat", 1, 0);
            airAnim = new Animation("Characters/Jet/Jump", 4, 0);
            airHurtAnim = new Animation("Characters/Jet/HurtAir", 5, 4, false);
            groundHurtAnim = new Animation("Characters/Jet/HurtStand", 2, 6, false);
            crouchHurtAnim = new Animation("Characters/Jet/HurtCrouch", 2, 6, false);
            overheadHurtAnim = new Animation("Characters/Jet/HurtOverhead", 2, 6, false);
            knockdownAnim = new Animation("Characters/Jet/Knockdown", 4, 3, false);
            guardHighAnim = new Animation("Characters/Jet/Guard_High", 3, 6);
            guardLowAnim = new Animation("Characters/Jet/Guard_Low", 3, 6);
            techForwardAnim = new Animation("Characters/Jet/Tech_Forward", 6, 5, false);
            techBackAnim = new Animation("Characters/Jet/Tech_Backward", 6, 5, false);

            attacks.Clear();
        }

        public override void AttackUpdate()
        {
            BasicGrabUpdate(12, 60, new Vector2(80 * direction, -16));
        }

        public override Dictionary<string, object> GetState()
        {
            var state = base.GetState();
            return state;
        }

        public override void LoadState(Dictionary<string, object> state)
        {
            base.LoadState(state);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}