using ChromaCore.Code.Objects;
using ChromaCore.Code.Scenes;

namespace ChromaCore.Code.Utils.Map
{
    /// <summary>
    /// Contains methods for setting up tiles in InGame
    /// </summary>
    public static class TileMap
    {
        /// <summary>
        /// Determines the spacing of tiles when they are generated as well as the size of Tile object colliders
        /// </summary>
        public static int Tile_Size = 64;

        public static void GenerateMap(TileToMap[,] tilemap)
        {
            Game main = Game.Instance;
            InGame scene = (InGame)main.Scene;
            scene.tiles = new Tile[tilemap.GetLength(0), tilemap.GetLength(1)];
            for (int i = 0; i < tilemap.GetLength(1); i++)
            {
                for (int j = 0; j < tilemap.GetLength(0); j++)
                {
                    if (tilemap[j, i] != null)
                    {
                        if (tilemap[j, i] is ObjectSpawner os)
                        {
                            GameObject obj = (GameObject)Activator.CreateInstance(os.objectType);
                            obj.position = new Vector2(i * Tile_Size, j * Tile_Size);
                            scene.miscObjects.Add(obj);
                        }
                        else
                        {
                            Tile t = new Tile(tilemap[j, i].sprite, i * Tile_Size, j * Tile_Size) { platform = tilemap[j, i].platform };
                            scene.tiles[j, i] = t;
                        }
                    }
                }
            }
        }

        public static TileToMap[,] TileMapFromPNG(string spriteURL, Dictionary<Color, TileToMap> colorDictionary)
        {
            Texture2D texture = Game.LoadAsset<Texture2D>(spriteURL);
            TileToMap[,] tileMap = new TileToMap[texture.Height, texture.Width];

            Color[] colorMap = new Color[texture.Width * texture.Height];
            texture.GetData(colorMap);

            for (int i = 0; i < colorMap.Length; i++)
            {
                Vector2 pos = new Vector2(i % texture.Width, (int)Math.Floor((float)i / texture.Width));
                if (colorDictionary.ContainsKey(colorMap[i])) tileMap[(int)pos.Y, (int)pos.X] = colorDictionary[colorMap[i]];
            }

            return tileMap;
        }
    }

    public class TileToMap
    {
        public string sprite;
        public bool solid;
        public bool platform;
        public string sourceTexture;

        public TileToMap() { }
    }

    public class ObjectSpawner : TileToMap
    {
        public Type objectType;
        public ObjectSpawner() { }
    }
}
