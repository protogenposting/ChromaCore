namespace RCArena.Code.UI
{
    public class MenuCursor : IDisposable
    {
        public Animation animation = new Animation("ExampleContent/UICursor");
        public Controller input;
        public ButtonMatrix matrix;
        public Vector2 position = Vector2.Zero;

        public int inputLockout = 0;
        public bool visible = true;

        public UIElement selectedElement => matrix != null && position.X < matrix.buttons.GetLength(0) && position.Y < matrix.buttons.GetLength(1)
            ? matrix.buttons[(int)position.X, (int)position.Y] : null;

        public MenuCursor(int controllerID, ButtonMatrix initialMatrix)
        {
            input = new Controller(controllerID);
            matrix = initialMatrix;
            inputLockout = 6;
        }
        public MenuCursor(Controller controller, ButtonMatrix initialMatrix)
        {
            input = controller;
            matrix = initialMatrix;
            inputLockout = 6;
        }

        public virtual void Update()
        {
            if (Game.Instance.screenTransition == null) input.UpdateKeys(1);

            if (inputLockout <= 0 && matrix != null)
            {
                if (input.KeyPressed(Controller.Key_Up))
                {
                    input.ClearBuffer(Controller.Key_Up);
                    matrix.upInput?.Invoke(this, matrix);
                    inputLockout = 2;
                }
                if (input.KeyPressed(Controller.Key_Down))
                {
                    input.ClearBuffer(Controller.Key_Down);
                    matrix.downInput?.Invoke(this, matrix);
                    inputLockout = 2;
                }
                if (input.KeyPressed(Controller.Key_Left))
                {
                    input.ClearBuffer(Controller.Key_Left);
                    matrix.leftInput?.Invoke(this, matrix);
                    inputLockout = 2;
                }
                if (input.KeyPressed(Controller.Key_Right))
                {
                    input.ClearBuffer(Controller.Key_Right);
                    matrix.rightInput?.Invoke(this, matrix);
                    inputLockout = 2;
                }
                if (input.KeyPressed(Controller.Key_MenuConfirm))
                {
                    input.ClearBuffer(Controller.Key_MenuConfirm);
                    matrix.confirmInput?.Invoke(this, matrix);
                    inputLockout = 6;
                }
                if (input.KeyPressed(Controller.Key_MenuBack))
                {
                    input.ClearBuffer(Controller.Key_MenuBack);
                    matrix.backInput?.Invoke(this, matrix);
                    inputLockout = 6;
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
            if (matrix != null && visible && !Game.Instance.Scene.UI.elements.Exists(e => e is PopupMessage))
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
            }
        }

        public void Dispose()
        {
            Game.Instance.Scene.cursors.Remove(this);
            GC.SuppressFinalize(this);
        }
    }
}
