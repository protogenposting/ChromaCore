using System;

namespace ChromaCore
{
    public static class Program
    {
        public static Game game;

        [STAThread]
        static void Main()
        {
            using (game = new Game())
            {
                game.Run();
            }
        }
    }
}
