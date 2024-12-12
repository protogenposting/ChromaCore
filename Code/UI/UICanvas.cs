namespace ChromaCore.Code.UI
{
    /// <summary>
    /// Base container for storing all UI elements on a scene
    /// </summary>
    public class UICanvas : IDisposable
    {
        public List<UIElement> elements;
        public List<ButtonMatrix> buttonMatricies;

        public UICanvas()
        {
            elements = new List<UIElement>();
            buttonMatricies = new List<ButtonMatrix>();
        }

        public virtual void Update()
        {
            foreach (UIElement e in elements.ToArray()) e.Update();
            foreach (ButtonMatrix m in buttonMatricies.ToArray()) m.Update();
        }

        public virtual void Draw(SpriteBatch spritebatch)
        {
            foreach (UIElement e in elements.ToArray()) e.Draw(spritebatch);
            foreach (ButtonMatrix m in buttonMatricies.ToArray()) m.Draw(spritebatch);
        }

        public virtual void Clear()
        {
            foreach (UIElement e in elements.ToArray()) e.Dispose();
            foreach (ButtonMatrix m in buttonMatricies.ToArray()) m.Dispose();
            elements = new List<UIElement>();
            buttonMatricies = new List<ButtonMatrix>();
        }

        public virtual void AddElement(UIElement element)
        {
            elements.Add(element);
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
