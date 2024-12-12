namespace ChromaCore.Code.Utils.ScreenTransitions
{
    public class ScreenTransition : IDisposable
    {
        public int duration = 60;
        public int transitionTime = 30;

        public delegate void OnTransition();
        public OnTransition onTransition;

        protected int timer;

        public virtual void Update()
        {
            timer++;

            if (timer == transitionTime) onTransition?.Invoke();
            if (timer == duration)
            {
                Game.Instance.screenTransition = null;
                Dispose();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch) { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
