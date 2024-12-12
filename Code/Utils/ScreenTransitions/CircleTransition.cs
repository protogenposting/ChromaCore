using ChromaCore.Code.Scenes;

namespace ChromaCore.Code.Utils.ScreenTransitions
{
    public class CircleTransition : ScreenTransition
    {
        Texture2D pixel = Game.LoadTexture("ExampleContent/Pixel");
        Effect effect = Game.LoadShader("Shaders/ScreenTransitions/CircleTransition");
        public Vector2 target = new Vector2(960, 540);
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Game.Instance.Scene is InGame ig) target = (ig.camera.target - ig.camera.position) * ig.camera.zoom;
            target.X = Math.Clamp(target.X, 0, 1920);
            target.Y = Math.Clamp(target.Y, 0, 1080);

            float interval = duration / 3f;
            float ring = timer <= interval ? ((interval - timer) / interval) : timer >= interval * 2 ? ((interval - (duration - timer)) / interval) : 0;
            ring *= 1800;

            effect.Parameters["target"].SetValue(target);
            effect.Parameters["timer"].SetValue(ring);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, effect);
            spriteBatch.Draw(pixel, new Rectangle(0, 0, 1920, 1080), Color.White);
            spriteBatch.End();
        }
    }
}
