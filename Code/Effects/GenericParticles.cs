using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCArena.Code.Effects
{
    public static class GenericParticles
    {
        public static ParticleSpawner hitSpiral => new ParticleSpawner(new Animation("Particles/Common/prt_Spiral", 5, 3, new Vector2(64), false), 15, Vector2.Zero) { additiveBlend = true };
        public static ParticleSpawner hitExplosion => new ParticleSpawner(new Animation("Particles/Common/prt_Explosion", 5, 4, new Vector2(64), false), 20, Vector2.Zero) { additiveBlend = false, dynamicRotation = true };
        public static ParticleSpawner hitFireExplosion => new ParticleSpawner(new Animation("Particles/Common/prt_FireExplosion", 5, 4, new Vector2(64), false), 20, Vector2.Zero) { additiveBlend = false, dynamicRotation = true };
        public static ParticleSpawner hitArrow => new ParticleSpawner(new Animation("Particles/Common/prt_Arrow", 5, 4, new Vector2(64), false), 20, Vector2.Zero) { dynamicRotation = true };
        public static ParticleSpawner hitSmallArrow => new ParticleSpawner(new Animation("Particles/Common/prt_ArrowSmall", 5, 4, new Vector2(64), false), 20, Vector2.Zero) { dynamicRotation = true };
        public static ParticleSpawner hitDiamond => new ParticleSpawner(new Animation("Particles/Common/prt_Diamond", 5, 4, new Vector2(64), false), 20, Vector2.Zero) { additiveBlend = true };
        public static ParticleSpawner hitHelix => new ParticleSpawner(new Animation("Particles/Common/prt_Helix", 5, 4, new Vector2(64), false), 20, Vector2.Zero) { additiveBlend = false, dynamicRotation = true };
        public static ParticleSpawner hitLines => new ParticleSpawner(new Animation("Particles/Common/prt_Lines", 5, 4, new Vector2(64), false), 20, Vector2.Zero);
        public static ParticleSpawner hitThinCircle => new ParticleSpawner(new Animation("Particles/Common/prt_ThinCircle", 5, 4, new Vector2(64), false), 20, Vector2.Zero);

        public static ParticleSpawner blockHit = new ParticleSpawner(new Animation("Particles/Common/prt_Block", 10, 2, new Vector2(64), false), 20, Vector2.Zero, 1, 0) { drawColor = new Color(180, 255, 255), dynamicRotation = true };
        public static ParticleSpawner throwTech = new ParticleSpawner(new Animation("Particles/Common/prt_ThrowTech", 10, 3, new Vector2(64), false), 30, Vector2.Zero, 1, 0) { drawColor = new Color(120, 255, 180), dynamicRotation = true };

        public static MultiParticleSpawner hitEffect => new MultiParticleSpawner(new List<ParticleSpawner>()
        {
            hitThinCircle,
            hitLines,
            hitSmallArrow
        });

        public static MultiParticleSpawner HitLightNeutral()
        {
            return new MultiParticleSpawner(new List<ParticleSpawner>()
            {
                hitThinCircle,
                hitLines
            });
        }

        //Angle controls the rotation of certain parts, if -1, it will rotate based on knockback angle
        public static MultiParticleSpawner HitLightDirectional(int angle = -1)
        {
            ParticleSpawner arrow = hitSmallArrow;
            if (angle != -1)
            {
                arrow.rotation = angle;
                arrow.dynamicRotation = false;
            }

            return new MultiParticleSpawner(new List<ParticleSpawner>()
            {
                hitThinCircle,
                hitLines,
                arrow
            });
        }

        public static MultiParticleSpawner HitFireNeutral()
        {
            ParticleSpawner lines = hitLines;
            lines.drawColor = new Color(255, 180, 0);

            return new MultiParticleSpawner(new List<ParticleSpawner>()
            {
                lines
            });
        }

        //Angle controls the rotation of certain parts, if -1, it will rotate based on knockback angle
        public static MultiParticleSpawner HitFireDirectional(int angle = -1)
        {
            ParticleSpawner explosion = hitExplosion;
            ParticleSpawner fireExplosion = hitFireExplosion;
            explosion.drawColor = new Color(255, 180, 0);
            if (angle != -1)
            {
                explosion.rotation = angle;
                explosion.dynamicRotation = false;
            }
            fireExplosion.drawColor = new Color(255, 120, 0);
            if (angle != -1)
            {
                fireExplosion.rotation = angle;
                fireExplosion.dynamicRotation = false;
            }

            return new MultiParticleSpawner(new List<ParticleSpawner>()
            {
                explosion,
                fireExplosion
            });
        }
    }
}
