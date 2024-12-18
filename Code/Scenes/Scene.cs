namespace RCArena.Code.Scenes
{
    /// <summary>
    /// <para>Basic container to handle game rules and logic</para>
    /// <para>Contains built in UICanvas and cursor fields</para>
    /// <para>Should be derived to perform specific tasks</para>
    /// </summary>
    public abstract class Scene
    {
        public UICanvas UI;
        public Camera camera;
        public List<MenuCursor> cursors = new List<MenuCursor>();

        public abstract void Load();

        public abstract void Update(GameTime gametime);

        public abstract void Draw(SpriteBatch spritebatch);

        public virtual void UpdateCursors()
        {
            if (!UI.elements.Exists(e => e is PopupMessage)) foreach (MenuCursor c in cursors.ToArray()) c.Update();
        }
    }
}