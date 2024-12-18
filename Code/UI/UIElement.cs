namespace RCArena.Code.UI
{
    /// <summary>
    /// Base class for implementing UI with a built in onClick function
    /// </summary>
    public class UIElement : IDisposable
    {
        private bool mouseClicked = false;
        private bool prevMouseClick = false;

        public Animation animation;
        public Vector2 position;
        public float layer = 0.5f;
        public float drawScale = 1;
        public float hoverScale = 1;
        public Action<MenuCursor, ButtonMatrix, UIElement> onClick;
        public Vector2 size;

        public UIElement parent = null;
        public List<UIElement> children = new List<UIElement>();
        public ButtonMatrix parentMatrix;
        public int matrixID = -1;

        protected bool MouseHovering => new Rectangle((int)position.X - (int)size.X / 2, (int)position.Y - (int)size.Y / 2, (int)size.X, (int)size.Y).Contains(Game.Instance.mouseScreenPosition);

        public UIElement(string texture, float x = 0, float y = 0, Action<MenuCursor, ButtonMatrix, UIElement> onClickEvent = null)
        {
            if (texture != "" && texture != null) animation = new Animation(texture);
            position = new Vector2(x, y);
            onClick = onClickEvent;
        }

        public UIElement(Animation animation, float x = 0, float y = 0, Action<MenuCursor, ButtonMatrix, UIElement> onClickEvent = null)
        {
            if (animation != null) this.animation = animation;
            position = new Vector2(x, y);
            onClick = onClickEvent;
        }

        public virtual void Update()
        {
            animation?.Update();

            foreach (UIElement child in children) child.Update();
            if (MouseHovering && Game.Instance.mouseLeftClick) mouseClicked = true;
            if (MouseHovering && !Game.Instance.mouseLeftClick && onClick != null)
            {
                if (mouseClicked)
                {
                    hoverScale = 1;
                    if (prevMouseClick)
                    {
                        onClick?.Invoke(null, parentMatrix, this);
                        mouseClicked = false;
                    }
                }
                else hoverScale = MathHelper.Lerp(hoverScale, 1.1f, 0.25f);
            }
            else
            {
                if (!MouseHovering) mouseClicked = false;
                hoverScale = MathHelper.Lerp(hoverScale, 1f, 0.25f);
            }
            prevMouseClick = Game.Instance.mouseLeftClick;
        }

        public virtual void Draw(SpriteBatch spritebatch)
        {
            animation?.Draw(spritebatch, parent == null ? position : parent.position + position, 1, Color.White, layer, 0, drawScale * hoverScale);

            foreach (UIElement child in children) child.Draw(spritebatch);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((position - animation.spriteSheet.Bounds.Center.ToVector2() * drawScale * Animation.BaseDrawScale).ToPoint(), (animation.spriteSheet.Bounds.Size.ToVector2() * drawScale * Animation.BaseDrawScale).ToPoint());
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
