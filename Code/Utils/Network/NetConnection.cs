using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ChromaCore.Code.Utils.Network
{
    public class NetConnection
    {
        public bool isHost = false;
        public int playerID = 0;
        public TcpClient client;
        DateTime lastPing;
        public float latency;
        public int playerController = -1;

        public Action onDisconnect;

        string incompleteBuffer = "";

        public NetConnection()
        {
            isHost = true;
            TcpListener server = new TcpListener(System.Net.IPAddress.Any, 9000);
            server.Start();
            client = server.AcceptTcpClient();
            server.Stop();
            SendMessage("ping", new JsonObject());
        }

        public NetConnection(string ip, int port)
        {
            if (ip.Contains(':'))
            {
                string[] ipPort = ip.Split(':');
                if (int.TryParse(ipPort[1], out int num))
                {
                    ip = ipPort[0];
                    port = num;
                }
            }
            client = new TcpClient(ip, port);
            lastPing = DateTime.Now;
        }

        public void SendMessage(string type, JsonObject message)
        {
            if (client == null || !client.Connected) return;
            message.Add("type", type);
            List<byte> b = new List<byte>() { (byte)'~' };
            b.AddRange(Encoding.ASCII.GetBytes(message.ToJsonString()));
            client.GetStream().Write(b.ToArray());
        }

        public List<JsonDocument> ReceiveMessages()
        {
            List<JsonDocument> messages = new List<JsonDocument>();
            List<byte> bytes = new List<byte>();
            while (client.Available > 0)
            {
                byte[] buffer = new byte[client.Available];
                client.GetStream().Read(buffer, 0, buffer.Length);
                bytes.AddRange(buffer);
            }
            string[] input = Encoding.ASCII.GetString(bytes.ToArray()).Split('~', StringSplitOptions.RemoveEmptyEntries);

            foreach (string str in input)
            {
                JsonDocument message = null;
                try
                {
                    message = JsonDocument.Parse(incompleteBuffer + str);
                    incompleteBuffer = "";
                }
                catch
                {
                    incompleteBuffer = incompleteBuffer + str;
                    break;
                }
                string type = message.RootElement.GetProperty("type").GetString();
                if (type == "ping")
                {
                    latency = (float)(DateTime.Now - lastPing).TotalMilliseconds;
                    lastPing = DateTime.Now;
                    SendMessage("ping", new JsonObject());
                }
                else if (type == "disconnect")
                {
                    SendMessage("disconnect", new JsonObject());
                    client.Close();
                    onDisconnect?.Invoke();
                    return new List<JsonDocument>();
                }
                else
                {
                    messages.Add(message);
                }
            }

            return messages;
        }
    }
}
