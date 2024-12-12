using ChromaCore.Code.Utils.Visual;

namespace ChromaCore.Code.UI
{
    /// <summary>
    /// UI element that displays text on top
    /// </summary>
    public class TextBlock : UIElement
    {
        public string text = "";
        public Color textColor = Color.Black;
        public Vector2 textOffset = Vector2.Zero;
        public SpriteFont font;
        public TextAlignment textAlignment = TextAlignment.Center;

        public const float textScale = 2;

        public TextBlock(string text, string fontURL, Color textColor, string texture, float x = 0, float y = 0) : base(texture, x, y)
        {
            this.text = text;
            this.font = Game.LoadAsset<SpriteFont>(fontURL);
            this.textColor = textColor;
        }

        public TextBlock(string text, string fontURL, Color textColor, Animation animation, float x = 0, float y = 0) : base(animation, x, y)
        {
            this.text = text;
            this.font = Game.LoadAsset<SpriteFont>(fontURL);
            this.textColor = textColor;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch);

            spritebatch.DrawString(font, text, AlignmentToPosition(textAlignment), textColor, 0, AlignmentToOrigin(textAlignment, text, font), textScale * hoverScale * drawScale, SpriteEffects.None, layer + 0.01f);
        }

        public Vector2 AlignmentToPosition(TextAlignment textAlignment)
        {
            if (animation == null) return parent == null ? position : parent.position + position;

            return (parent == null ? position : parent.position + position) + textOffset + new Vector2(
                    textAlignment.ToString().Contains("Left") ? ((-animation.cellSize.X / 2 + 8) * drawScale * Animation.BaseDrawScale * hoverScale) : textAlignment.ToString().Contains("Right") ? ((animation.cellSize.X / 2 - 8) * drawScale * Animation.BaseDrawScale * hoverScale) : 0,
                    textAlignment.ToString().Contains("Top") ? ((-animation.cellSize.Y / 2 + 4) * drawScale * Animation.BaseDrawScale * hoverScale) : textAlignment.ToString().Contains("Bottom") ? ((animation.cellSize.Y / 2 - 4) * drawScale * Animation.BaseDrawScale * hoverScale) : 0);
        }

        public Vector2 AlignmentToOrigin(TextAlignment textAlignment, string text, SpriteFont font)
        {
            Vector2 fontOffset = font.MeasureString(text);

            return new Vector2(
                    textAlignment.ToString().Contains("Left") ? 0 : textAlignment.ToString().Contains("Right") ? fontOffset.X : fontOffset.X / 2,
                    textAlignment.ToString().Contains("Top") ? 0 : textAlignment.ToString().Contains("Bottom") ? fontOffset.Y : fontOffset.Y / 2);
        }
    }

    public enum TextAlignment
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }
}
