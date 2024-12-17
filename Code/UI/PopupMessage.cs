namespace ChromaCore.Code.UI
{
    /// <summary>
    /// <para>Overrides the current UI to display in the center of the screen</para>
    /// <para>Invokes a method when closed and can be given "confirm or cancel" functionality</para>
    /// </summary>
    public class PopupMessage : TextBlock
    {
        int timer = 0;
        MenuCursor cursor;

        public string confirmText = "Confirm";
        public string cancelText = "Cancel";

        //Implement your own button prompts
        public Animation confirmButtonSprite;
        public Animation cancelButtonSprite;
        public float buttonPromptScale = 1;

        public static string BoxTexture = "ExampleContent/UIPopupMessageBox";

        Action<MenuCursor, ButtonMatrix, UIElement> onCancel;

        PopupMessage(string message, string font, MenuCursor cursor = null, Action<MenuCursor, ButtonMatrix, UIElement> onConfirm = null, Action<MenuCursor, ButtonMatrix, UIElement> onCancel = null) : base(message, font, Color.White, BoxTexture)
        {
            if (cursor != null) this.cursor = cursor;
            else
            {
                if (GamePad.GetState(0).IsConnected) this.cursor = new MenuCursor(0, null);
                else this.cursor = new MenuCursor(0, null);
            }

            onClick = onConfirm;
            this.onCancel = onCancel;

            layer = 0.98f;
            position = new Vector2(960, 540);
        }

        public static void Create(string message, string font, MenuCursor cursor = null, Action<MenuCursor, ButtonMatrix, UIElement> onConfirm = null, Action<MenuCursor, ButtonMatrix, UIElement> onCancel = null, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            if (Game.Instance.Scene.UI.elements.Exists(e => e is PopupMessage)) return;

            PopupMessage p = new PopupMessage(message, font, cursor, onConfirm, onCancel);

            p.confirmText = confirmText;
            p.cancelText = cancelText;

            Game.Instance.Scene.UI.AddElement(p);
        }

        public override void Update()
        {
            base.Update();

            cursor.input.UpdateKeys(1);

            timer++;
            if (timer >= 40 && cursor.input.KeyPressed(Controller.Key_MenuConfirm))
            {
                cursor.input.ClearAllBuffers();
                timer = -10;
            }
            if (timer >= 40 && cursor.input.KeyPressed(Controller.Key_MenuBack) && onCancel != null)
            {
                cursor.input.ClearAllBuffers();
                timer = -30;
            }

            if (timer <= 10)
            {
                drawScale = timer / 10f;
            }

            if (timer < 0)
            {
                drawScale = (-timer % 20) / 10f;

                if (timer == -1)
                {
                    onClick?.Invoke(cursor, null, this);
                    Game.Instance.Scene.UI.elements.Remove(this);
                    Dispose();
                }
                if (timer == -21)
                {
                    onCancel.Invoke(cursor, null, this);
                    Game.Instance.Scene.UI.elements.Remove(this);
                    Dispose();
                }
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            string txt = text;
            if (timer <= 10) text = "";
            base.Draw(spritebatch);
            text = txt;

            Texture2D button = confirmButtonSprite?.spriteSheet;

            if (timer > 10)
            {
                if (onCancel == null)
                {
                    if (button != null) spritebatch.Draw(button, AlignmentToPosition(TextAlignment.BottomRight) - new Vector2((font.MeasureString(confirmText).X / 2 * textScale) + 64, 16), null, Color.White, 0, button.Bounds.Center.ToVector2(), 2, SpriteEffects.None, layer + 0.01f);
                    spritebatch.DrawString(font, confirmText, AlignmentToPosition(TextAlignment.BottomRight), Color.White, 0, AlignmentToOrigin(TextAlignment.BottomRight, confirmText, font), textScale, SpriteEffects.None, layer + 0.01f);
                }
                else
                {
                    if (button != null) spritebatch.Draw(button, AlignmentToPosition(TextAlignment.BottomLeft) + new Vector2(0, -16), null, Color.White, 0, button.Bounds.Center.ToVector2(), 2, SpriteEffects.None, layer + 0.01f);
                    spritebatch.DrawString(font, confirmText, AlignmentToPosition(TextAlignment.BottomLeft) + new Vector2((font.MeasureString(confirmText).X / 2 * textScale), 0), Color.White, 0, AlignmentToOrigin(TextAlignment.BottomLeft, confirmText, font), textScale, SpriteEffects.None, layer + 0.01f);

                    button = cancelButtonSprite?.spriteSheet;

                    if (button != null) spritebatch.Draw(button, AlignmentToPosition(TextAlignment.BottomRight) - new Vector2((font.MeasureString(confirmText).X / 2 * textScale) + 32, 16), null, Color.White, 0, button.Bounds.Center.ToVector2(), 2, SpriteEffects.None, layer + 0.01f);
                    spritebatch.DrawString(font, cancelText, AlignmentToPosition(TextAlignment.BottomRight), Color.White, 0, AlignmentToOrigin(TextAlignment.BottomRight, cancelText, font), textScale, SpriteEffects.None, layer + 0.01f);
                }
            }
        }
    }
}
