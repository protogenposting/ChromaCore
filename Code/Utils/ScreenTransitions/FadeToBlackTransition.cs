namespace RCArena.Code.Utils.ScreenTransitions
{
    public class FadeToBlackTransition : ScreenTransition
    {
        public override void Draw(SpriteBatch spriteBatch)
        {
            float interval = duration / 3f;
            float fade = timer < interval ? timer / interval : timer > interval * 2 ? (duration - timer) / interval : 1;

            Texture2D tex = new Texture2D(Game.Instance.GraphicsDevice, 1, 1);
            tex.SetData(new Color[] { new Color(Color.Black, fade) });
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(tex, new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.End();
            tex.Dispose();
        }
    }
}
