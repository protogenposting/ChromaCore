using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromaCore.Code.Utils.Visual
{
    public struct Light
    {
        public int type;
        public Vector2 position;
        public float radius;
        public Color color;
        public Vector3 decay;
        public Vector4 extra;

        public Light(int type, Vector2 position, float radius, Color color, Vector3 decay, Vector4 extra)
        {
            this.type = type;
            this.position = position;
            this.radius = radius;
            this.color = color;
            this.decay = decay;
            this.extra = extra;
        }
    }
}
