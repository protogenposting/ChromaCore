namespace RCArena.Code.Utils.Map
{
    /// <summary>
    /// <para>Contains the tilemap, camera bounds, and a draw function that get called by InGame</para>
    /// <para>Should be derived to setup the environment</para>
    /// </summary>
    public class Room
    {
        public Rectangle bounds;

        public TileToMap[,] map;

        public Vector2[] spawn = new Vector2[2];

        public void SetupTiles() => TileMap.GenerateMap(map);

        public virtual void Draw(SpriteBatch spritebatch) { }
    }
}
