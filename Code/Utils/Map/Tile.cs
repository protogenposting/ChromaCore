using ChromaCore.Code.Objects;

namespace ChromaCore.Code.Utils.Map
{
    /// <summary>
    /// Static GameObject that, by default, never moves and is used for physics collisions
    /// </summary>
    public class Tile : GameObject
    {
        public bool forceCollision = true;
        public bool platform = false;

        public Texture2D sourceTexture;
        public Rectangle sourceRectangle;

        public Tile(string sprite)
        {
            if (sprite != "") animation = new Animation(sprite);
            drawLayer = 0.8f;
            position.X = 0;
            position.Y = 0;
            collider = new Collider(this, TileMap.Tile_Size, TileMap.Tile_Size, new Vector2(-TileMap.Tile_Size / 2, -TileMap.Tile_Size / 2));
        }

        public Tile(string sprite, int x, int y)
        {
            if (sprite != "") animation = new Animation(sprite);
            drawLayer = 0.8f;
            position.X = x;
            position.Y = y;
            collider = new Collider(this, TileMap.Tile_Size, TileMap.Tile_Size, new Vector2(-TileMap.Tile_Size / 2, -TileMap.Tile_Size / 2));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!sourceRectangle.IsEmpty)
            {
                spriteBatch.Draw(sourceTexture, position, sourceRectangle, drawColor, rotation, new Vector2(8), 4 * drawScale, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, drawLayer);
            }
            else base.Draw(spriteBatch);
        }
    }
}
