namespace ChromaCore.Code.UI
{
    /// <summary>
    /// Customizable grid of UI elements that can be traversed by a menu cursor
    /// </summary>
    public class ButtonMatrix : IDisposable
    {
        public bool enabled = true;
        public UIElement[,] buttons;

        public Action<MenuCursor, ButtonMatrix> upInput = DefaultUpPressed;
        public Action<MenuCursor, ButtonMatrix> downInput = DefaultDownPressed;
        public Action<MenuCursor, ButtonMatrix> leftInput = DefaultLeftPressed;
        public Action<MenuCursor, ButtonMatrix> rightInput = DefaultRightPressed;
        public Action<MenuCursor, ButtonMatrix> confirmInput = DefaultConfirmPressed;
        public Action<MenuCursor, ButtonMatrix> backInput = DefaultBackPressed;

        public bool loopCursor = true;
        public Vector2 multiCursorOffset = Vector2.Zero;
        public Vector2 cursorOffset = Vector2.Zero;

        public ButtonMatrix(UIElement[,] elements, Action<MenuCursor, ButtonMatrix> backPressedEvent = null, bool loopCursor = true)
        {
            buttons = elements;
            backInput = backPressedEvent;
            this.loopCursor = loopCursor;
        }

        public static ButtonMatrix Create1DMatrix(bool vertical, int startX, int startY, int spacing, List<UIElement> buttons, Action<MenuCursor, ButtonMatrix> backPressedEvent = null, bool loopCursor = true, Action<UIElement, int> buttonModifiers = null)
        {
            if (buttons.Count == 0) return new ButtonMatrix(new UIElement[0, 0], backPressedEvent, loopCursor);

            ButtonMatrix m = new ButtonMatrix(new UIElement[vertical ? 1 : buttons.Count, !vertical ? 1 : buttons.Count], backPressedEvent, loopCursor);

            for (int i = 0; i < buttons.Count; i++)
            {
                int pos = i;

                UIElement button = buttons[i];
                button.position = new Vector2(startX + (vertical ? 0 : spacing * i), startY + (!vertical ? 0 : spacing * i));
                button.parentMatrix = m;
                button.matrixID = i;
                buttonModifiers?.Invoke(button, pos);
                m.buttons[vertical ? 0 : i, !vertical ? 0 : i] = button;
            }

            m.cursorOffset = buttons[0].animation != null ? new Vector2(buttons[0].animation.spriteSheet.Width / -2 * Animation.BaseDrawScale, 0) : Vector2.Zero;

            return m;
        }

        public virtual void Update()
        {
            if (!enabled) return;
            foreach (UIElement e in buttons)
            {
                e?.Update();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!enabled) return;
            foreach (UIElement e in buttons)
            {
                e?.Draw(spriteBatch);
            }
        }

        public static void DefaultUpPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            Vector2 oldPos = cursor.position;
            do
            {
                if (cursor.position.Y > 0) cursor.position.Y--;
                else if (cursor.position.Y == 0 && matrix.loopCursor) cursor.position.Y = matrix.buttons.GetLength(1) - 1;
                if (cursor.position == oldPos) break;
            } while (matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y] == null);
        }
        public static void DefaultDownPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            Vector2 oldPos = cursor.position;
            do
            {
                if (cursor.position.Y < matrix.buttons.GetLength(1) - 1) cursor.position.Y++;
                else if (cursor.position.Y == matrix.buttons.GetLength(1) - 1 && matrix.loopCursor) cursor.position.Y = 0;
                if (cursor.position == oldPos) break;
            } while (matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y] == null);
        }
        public static void DefaultLeftPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            Vector2 oldPos = cursor.position;
            do
            {
                if (cursor.position.X > 0) cursor.position.X--;
                else if (cursor.position.X == 0 && matrix.loopCursor) cursor.position.X = matrix.buttons.GetLength(0) - 1;
                if (cursor.position == oldPos) break;
            } while (matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y] == null);
        }
        public static void DefaultRightPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            Vector2 oldPos = cursor.position;
            do
            {
                if (cursor.position.X < matrix.buttons.GetLength(0) - 1) cursor.position.X++;
                else if (cursor.position.X == matrix.buttons.GetLength(0) - 1 && matrix.loopCursor) cursor.position.X = 0;
                if (cursor.position == oldPos) break;
            } while (matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y] == null);
        }
        public static void DefaultConfirmPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y].onClick != null) matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y].onClick(cursor, matrix, matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y]);
        }
        public static void DefaultBackPressed(MenuCursor cursor, ButtonMatrix matrix)
        {

        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
