using ChromaCore.Code.Objects;

namespace ChromaCore.Code.Utils.Collision
{
    /// <summary>
    /// Collides in a rectangle with directional checks
    /// </summary>
    public class Collider
    {
        private GameObject owner;
        public GameObject Owner => owner;
        public Vector2 positionOffset;
        private Vector2 size;
        public float directionalOffset;
        public Func<RectangleF, RectangleF, bool> CollisionCondition = (r1, r2) => true;
        public static Func<RectangleF, RectangleF, bool> SlopeFunction(GameObject owner, Func<float, float, bool> slopeFunction) => (r1, r2) =>
        {
            float y1 = (r1.Bottom - r2.Bottom) / r1.Height;
            float y2 = (r1.Bottom - r2.Top) / r1.Height;
            float x1 = (r2.Left - (owner.direction == 1 ? r1.Left : r1.Right)) / r1.Width * owner.direction;
            float x2 = (r2.Right - (owner.direction == 1 ? r1.Left : r1.Right)) / r1.Width * owner.direction;
            return slopeFunction(y1, x1) || slopeFunction(y1, x2) || slopeFunction(y2, x1) || slopeFunction(y2, x2);
        };

        public Collider() { }

        public Collider(GameObject owner, float width, float height, Vector2 originOffset, float directionalOffset = 0)
        {
            this.owner = owner;
            this.directionalOffset = directionalOffset;
            this.size = new Vector2(width, height);
            this.positionOffset = originOffset;
        }

        public void Resize(float width, float height, Vector2 originOffset, float directionalOffset = 0)
        {
            this.size = new Vector2(width, height);
            positionOffset = originOffset;
        }

        public RectangleF Box => new RectangleF(owner.position + positionOffset + new Vector2(directionalOffset * owner.direction, 0), size);
        public RectangleF Top => new RectangleF(owner.position + positionOffset + new Vector2(directionalOffset * owner.direction, -1), new Vector2(size.X, 1));
        public RectangleF Bottom => new RectangleF(owner.position + positionOffset + new Vector2(directionalOffset * owner.direction, size.Y), new Vector2(size.X, 1));
        public RectangleF Left => new RectangleF(owner.position + positionOffset + new Vector2(directionalOffset * owner.direction - 1, 0), new Vector2(1, size.Y));
        public RectangleF Right => new RectangleF(owner.position + positionOffset + new Vector2(directionalOffset * owner.direction + size.X, 0), new Vector2(1, size.Y));
    }

    public struct RectangleF
    {
        public float X, Y, Width, Height;

        public float Left => X;
        public float Right => X + Width;
        public float Top => Y;
        public float Bottom => Y + Height;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(Vector2 position, Vector2 size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public bool Intersects(RectangleF r2)
        {
            var collideX = (Left >= r2.Left && Left <= r2.Right) || (Right >= r2.Left && Right <= r2.Right) || (r2.Left >= Left && r2.Left <= Right) || (r2.Right >= Left && r2.Right <= Right);
            var collideY = (Top >= r2.Top && Top <= r2.Bottom) || (Bottom >= r2.Top && Bottom <= r2.Bottom) || (r2.Top >= Top && r2.Top <= Bottom) || (r2.Bottom >= Top && r2.Bottom <= Bottom);
            return collideX && collideY;
        }
    }
}
