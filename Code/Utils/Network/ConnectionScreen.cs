using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChromaCore.Code.Objects.Players.Characters;
using ChromaCore.Code.Scenes;

namespace ChromaCore.Code.Utils.Network
{
    public class ConnectionScreen : Scene
    {
        bool host;
        string ip;
        int port;
        NetConnection connection;
        public int player1Controller = -1;

        public ConnectionScreen()
        {
            this.host = true;
        }

        public ConnectionScreen(string ip, int port)
        {
            this.host = false;
            this.ip = ip;
            this.port = port;
            DataManager.soundEffectVolume = 0;
        }

        public override void Load()
        {
            UI = new UICanvas();
            UI.AddElement(new TextBlock(host ? "Waiting for Opponent..." : "Connecting to Opponent...", "ExampleContent/UIFont", Color.White, "", 960, 540));

            Task.Run(() =>
            {
                connection = host ? new NetConnection() { playerID = 0, playerController = player1Controller, onDisconnect = () => Game.Instance.ChangeScene(new MainMenu()) } : new NetConnection(ip, port) { playerID = 1, playerController = player1Controller, onDisconnect = () => Game.Instance.ChangeScene(new MainMenu()) };
                Game.Instance.ChangeScene(new CharacterSelect(connection, player1Controller));
            });
        }

        public override void Update(GameTime gametime)
        {

        }

        public override void Draw(SpriteBatch spritebatch)
        {
            Game.Instance.GraphicsDevice.Clear(Color.SlateGray);

            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp);
            UI.Draw(spritebatch);
            spritebatch.End();
        }
    }
}
