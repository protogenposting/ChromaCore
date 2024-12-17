namespace ChromaCore.Code.UI
{
    /// <summary>
    /// Allows keyboard input into the ui to ouput the entered text
    /// </summary>
    public class TextBox : TextBlock
    {
        public bool active;
        private int flashingBarTimer;
        public string promptText = "";
        public string entryText = "";

        /// <summary>
        /// Runs when the user presses ENTER while active
        /// </summary>
        public Action<TextBox> onEnter;

        private ButtonMatrix holdMatrix;
        private MenuCursor holdCursor;

        public bool mouseTriggered = false;

        KeyboardState prevKeyState = Keyboard.GetState();

        public List<Keys> availableKeys = new List<Keys>();

        public TextBox(string entryText, string promptText, string fontURL, Color textColor, string texture, float x = 0, float y = 0) : base(promptText + entryText, fontURL, textColor, texture, x, y)
        {
            for (Keys k = Keys.A; k <= Keys.Z; k++) availableKeys.Add(k);
            for (Keys k = Keys.D0; k <= Keys.D9; k++) availableKeys.Add(k);
            for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++) availableKeys.Add(k);
            availableKeys.Add(Keys.Space);
            availableKeys.Add(Keys.OemPeriod);
            availableKeys.Add(Keys.OemSemicolon);

            this.promptText = promptText;
            this.entryText = entryText;

            onClick = OpenText;
        }

        public TextBox(string entryText, string promptText, string fontURL, Color textColor, Animation animation, float x = 0, float y = 0) : base(promptText + entryText, fontURL, textColor, animation, x, y)
        {
            for (Keys k = Keys.A; k <= Keys.Z; k++) availableKeys.Add(k);
            for (Keys k = Keys.D0; k <= Keys.D9; k++) availableKeys.Add(k);
            for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++) availableKeys.Add(k);
            availableKeys.Add(Keys.Space);
            availableKeys.Add(Keys.OemPeriod);
            availableKeys.Add(Keys.OemSemicolon);

            this.promptText = promptText;
            this.entryText = entryText;

            onClick = OpenText;
        }

        public override void Update()
        {
            base.Update();

            if (active)
            {
                flashingBarTimer++;
                if (flashingBarTimer >= 60) flashingBarTimer = 0;

                foreach (Keys k in availableKeys)
                {
                    if (Keyboard.GetState().IsKeyDown(k) && !prevKeyState.IsKeyDown(k))
                    {
                        if (k == Keys.Space) entryText += ' ';
                        else if (k == Keys.OemPeriod) entryText += '.';
                        else if (k == Keys.OemSemicolon) entryText += Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift) ? ":" : ";";
                        else if (k.ToString().Length > 1) entryText += k.ToString()[^1];
                        else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift)) entryText += k.ToString().ToUpper();
                        else entryText += k.ToString().ToLower();
                    }
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Back) && !prevKeyState.IsKeyDown(Keys.Back) && entryText.Length > 0) entryText = entryText.Remove(entryText.Length - 1);
                if (Keyboard.GetState().IsKeyDown(Keys.Enter) && !prevKeyState.IsKeyDown(Keys.Enter))
                {
                    active = false;
                    if (onEnter != null) onEnter(this);
                    holdCursor.matrix = holdMatrix;
                    holdCursor.inputLockout = 10;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Escape) && !prevKeyState.IsKeyDown(Keys.Escape))
                {
                    active = false;
                    holdCursor.matrix = holdMatrix;
                    holdCursor.SwitchMatrix(holdMatrix, 0, 1);
                    holdCursor.inputLockout = 10;
                }
                if (holdCursor != null && holdCursor.input.id != -1 && holdCursor.input.KeyPressed(Controller.Key_MenuConfirm))
                {
                    holdCursor.input.ClearAllBuffers();
                    active = false;
                    if (onEnter != null) onEnter(this);
                    holdCursor.matrix = holdMatrix;
                    holdCursor.inputLockout = 10;
                }
                if (holdCursor != null && holdCursor.input.id != -1 && holdCursor.input.KeyPressed(Controller.Key_MenuBack))
                {
                    holdCursor.input.ClearAllBuffers();
                    active = false;
                    holdCursor.SwitchMatrix(holdMatrix, 0, 1);
                    holdCursor.inputLockout = 10;
                }

                prevKeyState = Keyboard.GetState();
            }
            else flashingBarTimer = 0;

            text = promptText + entryText;
            if ((flashingBarTimer / 30) % 2 == 1) text = ' ' + text + '|';
        }

        void OpenText(MenuCursor cursor, ButtonMatrix matrix, UIElement button)
        {
            mouseTriggered = cursor == null;
            if (cursor == null) cursor = Game.Instance.Scene.cursors[0];
            if (matrix == null) matrix = cursor.matrix;
            active = true;
            holdMatrix = matrix;
            holdCursor = cursor;

            if (cursor != null)
                cursor.matrix = null;
        }
    }
}
