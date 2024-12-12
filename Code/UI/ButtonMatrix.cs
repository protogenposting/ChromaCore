using ChromaCore.Code.Utils.Visual;

namespace ChromaCore.Code.UI
{
    /// <summary>
    /// Customizable grid of UI elements that can be traversed by a menu cursor
    /// </summary>
    public class ButtonMatrix : IDisposable
    {
        public UIElement[,] buttons;

        public bool loopCursor = true;
        public Vector2 multiCursorOffset = Vector2.Zero;
        public Vector2 cursorOffset = Vector2.Zero;

        public delegate void BackPressed(MenuCursor cursor, ButtonMatrix matrix);
        public BackPressed onBackPressed;
        bool prevLeftClick = false;

        public ButtonMatrix(UIElement[,] elements, BackPressed backPressedEvent = null, bool loopCursor = true)
        {
            buttons = elements;
            onBackPressed = backPressedEvent;
            this.loopCursor = loopCursor;
        }

        public static ButtonMatrix Create1DMatrix(bool vertical, int startX, int startY, int spacing, List<UIElement> buttons, BackPressed backPressedEvent = null, bool loopCursor = true, Action<UIElement, int> buttonModifiers = null)
        {
            if (buttons.Count == 0) return new ButtonMatrix(new UIElement[0, 0], backPressedEvent, loopCursor);

            ButtonMatrix m = new ButtonMatrix(new UIElement[vertical ? 1 : buttons.Count, !vertical ? 1 : buttons.Count], backPressedEvent, loopCursor);

            for (int i = 0; i < buttons.Count; i++)
            {
                int pos = i;

                UIElement button = buttons[i];
                button.position = new Vector2(startX + (vertical ? 0 : spacing * i), startY + (!vertical ? 0 : spacing * i));
                buttonModifiers?.Invoke(button, pos);
                m.buttons[vertical ? 0 : i, !vertical ? 0 : i] = button;
            }

            m.cursorOffset = new Vector2(buttons[0].animation.spriteSheet.Width / -2 * Animation.BaseDrawScale, 0);

            return m;
        }

        public virtual void Update()
        {
            foreach (UIElement e in buttons)
            {
                e.Update();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach (UIElement e in buttons)
            {
                e.Draw(spriteBatch);
            }
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
