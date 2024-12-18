using RCArena.Code.Objects;
using RCArena.Code.Scenes;

namespace RCArena.Code.UI
{
    public class HealthBar : UIElement
    {
        Texture2D timerCover = Game.LoadAsset<Texture2D>("UI/BattleHUD/HealthBarTimerCover");
        SpriteFont font = Fonts.exampleFont;
        Texture2D redUnderbar = Game.LoadAsset<Texture2D>("UI/BattleHUD/HealthBar_Red");
        SpriteFont ComboCounterFont = Game.LoadAsset<SpriteFont>("Fonts/TitleFont");

        private InGame scene => Game.Instance.Scene is InGame i ? i : null;
        private Fighter[] players => Game.Instance.Scene is InGame scene ? scene.players.GetRange(0, 2).ToArray() : null;

        float[] oldHealthXScale;
        float[] redHealthXScale;

        int[] storeComboCounter;
        int[] storeComboCounterTimer;

        public HealthBar() : base("UI/BattleHUD/HealthBarFrame", 0, 0)
        {
            oldHealthXScale = new float[Math.Min(players.Length, 2)];
            redHealthXScale = new float[Math.Min(players.Length, 2)];
            storeComboCounter = new int[Math.Min(players.Length, 2)];
            storeComboCounterTimer = new int[Math.Min(players.Length, 2)];
        }

        public override void Update()
        {
            base.Update();

            position = new Vector2(960, 0);
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Game game = Game.Instance;

            //Frame
            spritebatch.Draw(animation.spriteSheet, position, null, Color.White, 0, new Vector2(animation.cellSize.X / 2, 0), 4, SpriteEffects.None, 0.4f);
            spritebatch.Draw(timerCover, position, null, Color.White, 0, new Vector2(animation.cellSize.X / 2, 0), 4, SpriteEffects.None, 0.41f);

            //Timer
            spritebatch.DrawString(font, (scene.versusTimer / 60).ToString(), new Vector2(960, 92), Color.White, 0, font.MeasureString((scene.versusTimer / 60).ToString()) / 2, 4, SpriteEffects.None, 0.409f);

            //Health Bars
            for (int i = 0; i < players.Length; i++)
            {
                spritebatch.Draw(players[i].healthBarBack, position, null, Color.White, 0, new Vector2(animation.cellSize.X / 2, 0), 4, i == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.39f);
                spritebatch.Draw(redUnderbar, position, null, Color.White, 0, new Vector2(animation.cellSize.X / 2, 0), 4 * new Vector2(redHealthXScale[i], 1), i == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.391f);

                float xScale = ((float)players[i].health / players[i].healthMax * 0.88f) + 0.12f;
                spritebatch.Draw(players[i].healthBarBar, position, null, Color.White, 0, new Vector2(animation.cellSize.X / 2, 0), 4 * new Vector2(MathHelper.Lerp(oldHealthXScale[i], xScale, 0.25f), 1), i == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.392f);

                if (players[i].healthBarFrame != null) spritebatch.Draw(players[i].healthBarFrame, position, null, Color.White, 0, new Vector2(animation.cellSize.X / 2, i), 4, i == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0.393f);

                oldHealthXScale[i] = MathHelper.Lerp(oldHealthXScale[i], xScale, 0.25f);
                if (players[i].hitstunTimer == 0 && players[i].knockdownTimer <= 4) redHealthXScale[i] = MathHelper.Lerp(redHealthXScale[i], xScale, 0.5f);

                if (players[i].comboCounter > 1)
                {
                    if (storeComboCounterTimer[i] < 60) storeComboCounterTimer[i] += 5;
                    storeComboCounter[i] = players[i].comboCounter;
                }
                else if (storeComboCounterTimer[i] > 0) storeComboCounterTimer[i]--;

                if (players[i].comboCounter > 1 || storeComboCounterTimer[i] > 0)
                {
                    string text = storeComboCounter[i] + " Combo";
                    Vector2 off = new Vector2(ComboCounterFont.MeasureString(text).X * i, ComboCounterFont.MeasureString(text).Y / 2);
                    spritebatch.DrawString(ComboCounterFont, text, position + new Vector2(i == 0 ? -800 : 800, 220), new Color(Color.Lerp(storeComboCounter[i] < 5 ? Color.White : storeComboCounter[i] < 10 ? new Color(0xcd, 0x7f, 0x32) : storeComboCounter[i] < 15 ? Color.Silver : Color.Gold, Color.White, 0.5f), storeComboCounterTimer[i] / 20f), 0, off, 1 + 0.025f * storeComboCounter[i], SpriteEffects.None, 0.49f);
                }
            }
        }
    }
}
