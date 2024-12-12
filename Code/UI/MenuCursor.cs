using ChromaCore.Code.Utils.Visual;

namespace ChromaCore.Code.UI
{
    public class MenuCursor : IDisposable
    {
        public static bool show = true;

        public static SoundEffect hoverSound = null;
        public static SoundEffect confirmSound = null;
        public Animation animation = new Animation("ExampleContent/UICursor");
        public Controller input;
        public ButtonMatrix matrix;
        public Vector2 position = Vector2.Zero;

        public int inputLockout = 0;

        public delegate void InputAction(MenuCursor cursor, ButtonMatrix matrix);
        public InputAction UpInput = DefaultUpPressed;
        public InputAction DownInput = DefaultDownPressed;
        public InputAction LeftInput = DefaultLeftPressed;
        public InputAction RightInput = DefaultRightPressed;
        public InputAction ConfirmInput = DefaultConfirmPressed;
        public InputAction BackInput = DefaultBackPressed;

        public MenuCursor(int controllerID, ButtonMatrix initialMatrix)
        {
            input = new Controller(controllerID);
            matrix = initialMatrix;
            inputLockout = 10;
        }

        public virtual void Update()
        {
            input.UpdateKeys(1);

            if (inputLockout <= 0 && matrix != null)
            {
                Vector2 prevpos = position;
                if (input.KeyPressed(Controller.Key_Up))
                {
                    if (show)
                    {
                        if (UpInput != null) UpInput(this, matrix);
                        inputLockout = 6;
                        if (position != prevpos) hoverSound?.Play(Game.SoundEffectVolume, 0, 0);
                    }
                    input.ClearBuffer(Controller.Key_Up);
                    show = true;
                }
                if (input.KeyPressed(Controller.Key_Down))
                {
                    if (show)
                    {
                        if (DownInput != null) DownInput(this, matrix);
                        inputLockout = 6;
                        if (position != prevpos) hoverSound?.Play(Game.SoundEffectVolume, 0, 0);
                    }
                    input.ClearBuffer(Controller.Key_Down);
                    show = true;
                }
                if (input.KeyPressed(Controller.Key_Left))
                {
                    if (show)
                    {
                        if (LeftInput != null) LeftInput(this, matrix);
                        inputLockout = 6;
                        if (position != prevpos) hoverSound?.Play(Game.SoundEffectVolume, 0, 0);
                    }
                    input.ClearBuffer(Controller.Key_Left);
                    show = true;
                }
                if (input.KeyPressed(Controller.Key_Right))
                {
                    if (show)
                    {
                        if (RightInput != null) RightInput(this, matrix);
                        inputLockout = 6;
                        if (position != prevpos) hoverSound?.Play(Game.SoundEffectVolume, 0, 0);
                    }
                    input.ClearBuffer(Controller.Key_Right);
                    show = true;
                }
                if (input.KeyPressed(Controller.Key_MenuConfirm))
                {
                    if (show)
                    {
                        confirmSound?.Play(Game.SoundEffectVolume, 0, 0);
                        if (ConfirmInput != null) ConfirmInput(this, matrix);
                        inputLockout = 6;
                    }
                    input.ClearBuffer(Controller.Key_MenuConfirm);
                }
                if (input.KeyPressed(Controller.Key_MenuBack))
                {
                    if (show)
                    {
                        if (BackInput != null)
                        {
                            BackInput(this, matrix);
                            hoverSound?.Play(Game.SoundEffectVolume, 0, 0);
                        }
                        inputLockout = 6;
                    }
                    input.ClearBuffer(Controller.Key_MenuBack);
                }
            }
            else if (inputLockout > 0) inputLockout--;
        }

        public virtual void SwitchMatrix(ButtonMatrix newMatrix, int buttonX = 0, int buttonY = 0)
        {
            matrix = newMatrix;
            position = new Vector2(buttonX, buttonY);
            inputLockout = 6;
        }

        public virtual void Draw(SpriteBatch spritebatch)
        {
            if (!show) return;
            if (matrix != null && !Game.Instance.Scene.UI.elements.Exists(e => e is PopupMessage))
            {
                int numOnButton = 0;
                int myNumOnButton = 0;

                foreach (MenuCursor c in Game.Instance.Scene.cursors) if (c != null && c.matrix == matrix && c.position == position)
                    {
                        if (c == this) myNumOnButton = numOnButton;
                        numOnButton++;
                    }

                Vector2 drawPosition = matrix.buttons[(int)position.X, (int)position.Y].position + matrix.cursorOffset - matrix.multiCursorOffset * ((float)(numOnButton - 1) / 2) + matrix.multiCursorOffset * myNumOnButton;

                if (animation != null && matrix != null) spritebatch.Draw(animation.spriteSheet,
                    drawPosition,
                    new Rectangle((animation.currentFrame - 1) * (int)animation.cellSize.X, 0, (int)animation.cellSize.X, (int)animation.cellSize.Y),
                    Color.White, 0, new Vector2(animation.cellSize.X / 2, animation.cellSize.Y / 2), 4, SpriteEffects.None, 0.99f);

                //SpriteFont font = Game.LoadAsset<SpriteFont>("Fonts/baseFont");
                //if (input.id >= 0) spritebatch.DrawString(font, "p" + (input.id + 1), drawPosition, Color.White, 0, font.MeasureString("p" + (input.id + 1)) / 2, 1, SpriteEffects.None, 0.991f);
            }
        }

        public static void DefaultUpPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (cursor.position.Y > 0) cursor.position.Y--;
            else if (cursor.position.Y == 0 && matrix.loopCursor) cursor.position.Y = matrix.buttons.GetLength(1) - 1;
        }
        public static void DefaultDownPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (cursor.position.Y < matrix.buttons.GetLength(1) - 1) cursor.position.Y++;
            else if (cursor.position.Y == matrix.buttons.GetLength(1) - 1 && matrix.loopCursor) cursor.position.Y = 0;
        }
        public static void DefaultLeftPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (cursor.position.X > 0) cursor.position.X--;
            else if (cursor.position.X == 0 && matrix.loopCursor) cursor.position.X = matrix.buttons.GetLength(0) - 1;
        }
        public static void DefaultRightPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (cursor.position.X < matrix.buttons.GetLength(0) - 1) cursor.position.X++;
            else if (cursor.position.X == matrix.buttons.GetLength(0) - 1 && matrix.loopCursor) cursor.position.X = 0;
        }
        public static void DefaultConfirmPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y].onClick != null) matrix.buttons[(int)cursor.position.X, (int)cursor.position.Y].onClick(cursor, matrix);
        }
        public static void DefaultBackPressed(MenuCursor cursor, ButtonMatrix matrix)
        {
            if (matrix.onBackPressed != null) matrix.onBackPressed(cursor, matrix);
        }

        public void Dispose()
        {
            Game.Instance.Scene.cursors.Remove(this);
            GC.SuppressFinalize(this);
        }
    }
}
