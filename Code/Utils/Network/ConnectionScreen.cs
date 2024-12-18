using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using RCArena.Code.Objects.Players.Characters;
using RCArena.Code.Scenes;

namespace RCArena.Code.Utils.Network
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
            host = true;
        }

        public ConnectionScreen(string ip, int port)
        {
            host = false;
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
                connection.SendMessage("ConfirmConnection", new JsonObject());

                while (true)
                {
                    bool exit = false;
                    var messages = connection.ReceiveMessages();
                    foreach (var message in messages)
                    {
                        string type = message.RootElement.GetProperty("type").GetString();
                        if (type == "ConfirmConnection")
                        {
                            if (message.RootElement.TryGetProperty("PID", out JsonElement val)) connection.playerID = val.GetInt32();
                            exit = true;
                            break;
                        }
                    }
                    if (exit) break;
                }

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
