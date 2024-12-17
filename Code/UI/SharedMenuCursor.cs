namespace ChromaCore.Code.UI
{
    public class SharedMenuCursor : MenuCursor
    {
        public bool keyboard = true;
        Controller[] inputs = new Controller[5];
        public int lastPressedID;

        public SharedMenuCursor(ButtonMatrix initialMatrix) : base(-1, initialMatrix)
        {
            inputs[0] = input;
            for (int i = 1; i < inputs.Length; i++)
            {
                inputs[i] = new Controller(i - 1);
            }
        }

        public override void Update()
        {
            foreach (Controller c in inputs) c.UpdateKeys(1);

            int i = 0;
            foreach (Controller c in inputs)
            {
                if (inputLockout <= 0 && matrix != null)
                {
                    if (c.KeyPressed(Controller.Key_Up))
                    {
                        keyboard = i == 0;
                        c.ClearBuffer(Controller.Key_Up);
                        matrix.upInput?.Invoke(this, matrix);
                        inputLockout = 6;
                        lastPressedID = c.id;
                    }
                    if (c.KeyPressed(Controller.Key_Down))
                    {
                        keyboard = i == 0;
                        c.ClearBuffer(Controller.Key_Down);
                        matrix.downInput?.Invoke(this, matrix);
                        inputLockout = 6;
                        lastPressedID = c.id;
                    }
                    if (c.KeyPressed(Controller.Key_Left))
                    {
                        keyboard = i == 0;
                        c.ClearBuffer(Controller.Key_Left);
                        matrix.leftInput?.Invoke(this, matrix);
                        inputLockout = 6;
                        lastPressedID = c.id;
                    }
                    if (c.KeyPressed(Controller.Key_Right))
                    {
                        keyboard = i == 0;
                        c.ClearBuffer(Controller.Key_Right);
                        matrix.rightInput?.Invoke(this, matrix);
                        inputLockout = 6;
                        lastPressedID = c.id;
                    }
                    if (c.KeyPressed(Controller.Key_MenuConfirm))
                    {
                        keyboard = i == 0;
                        c.ClearBuffer(Controller.Key_MenuConfirm);
                        matrix.confirmInput?.Invoke(this, matrix);
                        inputLockout = 6;
                        lastPressedID = c.id;
                    }
                    if (c.KeyPressed(Controller.Key_MenuBack))
                    {
                        keyboard = i == 0;
                        c.ClearBuffer(Controller.Key_MenuBack);
                        matrix.backInput?.Invoke(this, matrix);
                        inputLockout = 6;
                        lastPressedID = c.id;
                    }
                }
                i++;
            }
            if (inputLockout > 0) inputLockout--;
        }
    }
}
