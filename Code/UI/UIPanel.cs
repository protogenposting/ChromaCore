using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromaCore.Code.UI
{
    public class UIPanel : UIElement
    {
        new public Vector2 size;
        public Color drawColor = Color.Black;

        public const float textScale = 2;

        public UIPanel(float x, float y, float width, float height) : this(x, y, width, height, Color.White) { }

        public UIPanel(float x, float y, float width, float height, Color color) : base("ExampleContent/UIPanel", x, y)
        {
            this.size = new Vector2(width, height);
            this.drawColor = color;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            int vSlice = animation.spriteSheet.Height / 3;
            int hSlice = animation.spriteSheet.Width / 3;

            if (size.X < hSlice * 6 || size.Y < vSlice * 6) return;
            int top = (int)(position.Y - size.Y / 2);
            int bottom = (int)(position.Y + size.Y / 2);
            int left = (int)(position.X - size.X / 2);
            int right = (int)(position.X + size.X / 2);

            //Corners
            spritebatch.Draw(animation.spriteSheet, new Rectangle(left, top, hSlice * 4, vSlice * 4), new Rectangle(0, 0, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
            spritebatch.Draw(animation.spriteSheet, new Rectangle(right - 64, top, hSlice * 4, vSlice * 4), new Rectangle(hSlice * 2, 0, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
            spritebatch.Draw(animation.spriteSheet, new Rectangle(left, bottom - 64, hSlice * 4, vSlice * 4), new Rectangle(0, vSlice * 2, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
            spritebatch.Draw(animation.spriteSheet, new Rectangle(right - 64, bottom - 64, hSlice * 4, vSlice * 4), new Rectangle(hSlice * 2, vSlice * 2, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);

            //Sides
            if (size.Y > 128)
            {
                spritebatch.Draw(animation.spriteSheet, new Rectangle(left, top + vSlice * 4, hSlice * 4, (int)size.Y - vSlice * 8), new Rectangle(0, vSlice, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
                spritebatch.Draw(animation.spriteSheet, new Rectangle(right - hSlice * 4, top + vSlice * 4, hSlice * 4, (int)size.Y - vSlice * 8), new Rectangle(hSlice * 2, vSlice, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
            }
            if (size.X > 128)
            {
                spritebatch.Draw(animation.spriteSheet, new Rectangle(left + hSlice * 4, top, (int)size.X - hSlice * 8, vSlice * 4), new Rectangle(hSlice, 0, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
                spritebatch.Draw(animation.spriteSheet, new Rectangle(left + hSlice * 4, bottom - vSlice * 4, (int)size.X - hSlice * 8, vSlice * 4), new Rectangle(hSlice, vSlice * 2, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
            }
            if (size.X > 128 || size.Y > 128)
            {
                spritebatch.Draw(animation.spriteSheet, new Rectangle(left + hSlice * 4, top + vSlice * 4, (int)size.X - hSlice * 8, (int)size.Y - vSlice * 8), new Rectangle(hSlice, vSlice, hSlice, vSlice), drawColor, 0, Vector2.Zero, SpriteEffects.None, layer);
            }

            foreach (UIElement child in children) child.Draw(spritebatch);
        }
    }
}
