using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using RCArena.Code.Objects;

namespace RCArena.Code.Effects
{
    public class AfterImage : Particle
    {
        byte tempalpha = 0;
        public AfterImage(GameObject obj, int lifetime, Color color, float decay = 1) : base(null, obj.position, lifetime, Vector2.Zero, decay, obj.rotation, obj.direction)
        {
            animation = obj.animation.Clone();
            animation.frameRate = 0;
            animation.particles = null;
            animation.sounds = null;

            drawColor = color;
            drawLayer = 0.4f;
            updateDuringHitpause = false;
        }

        public override void Update()
        {
            base.Update();
            tempalpha = drawColor.A;
            drawColor.A = (byte)(drawColor.A * acceleration);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animation.DrawSilhouette(spriteBatch, position, drawLayer, direction, new Color(drawColor.R, drawColor.G, drawColor.B, tempalpha), rotation, drawScale);
        }
    }

    public class DelayedAfterImage : AfterImage
    {
        int visibleDuration = 0;

        public DelayedAfterImage(GameObject obj, int delay, int visibleDuration, Color color, float decay = 1) : base(obj, delay + visibleDuration, color, decay)
        {
            this.visibleDuration = visibleDuration;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (duration <= visibleDuration) base.Draw(spriteBatch);
        }
    }
}
