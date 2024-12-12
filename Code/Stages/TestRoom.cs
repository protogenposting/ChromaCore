using System;
using System.Collections.Generic;
using System.Text;
using ChromaCore.Code.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace ChromaCore.Code.Stages
{
    public class TestRoom : Room
    {
        static Texture2D background = Game.LoadAsset<Texture2D>("Stages/TestRoom/Background");

        public TestRoom()
        {
            spawn[0] = new Vector2(682, 860);
            spawn[1] = new Vector2(982, 860);

            TileToMap invisibleWall = new TileToMap()
            {
                sprite = "",
                solid = true
            };
            TileToMap floor = new TileToMap()
            {
                sprite = "Stages/TestRoom/FloorTile",
                solid = true
            };

            Dictionary<Color, TileToMap> colorDictionary = new Dictionary<Color, TileToMap>()
            {
                {new Color(64, 64, 64), floor },
                {new Color(255, 255, 255), invisibleWall }
            };

            map = TileMap.TileMapFromPNG("Stages/TestRoom/Tilemap", colorDictionary);

            bounds = new Rectangle(0, 0, (map.GetLength(1) - 1) * TileMap.Tile_Size, (map.GetLength(0) - 1) * TileMap.Tile_Size);
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            InGame scene = Game.Instance.Scene as InGame;
            Vector2 parallaxCenter = bounds.Center.ToVector2();
            Vector2 parallaxOffset = scene.camera.position - parallaxCenter;

            spritebatch.Draw(background, parallaxCenter + parallaxOffset / 8, null, Color.White, 0, background.Bounds.Center.ToVector2(), 4, SpriteEffects.None, 0.1f);
        }
    }
}
