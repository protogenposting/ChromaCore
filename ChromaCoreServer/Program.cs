using System.Net;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Text;

/*
 * The server program will listen for connections on port 9000 endlessly
 * Upon receiving 2 clients, it will pair them and allow them to begin a match
 * 
 * This only relays messages between the two clients, it does not perform any logic pertaining to the game itself
 * 
 * You can view the current players searching for a game by typing 'players' in the console
 * You can view the current matches being played by typing 'matches' in the console
*/

namespace ChromaCoreServer
{
    internal class Program
    {
        internal static List<TcpClient> searchingClients = new List<TcpClient>();
        internal static List<(TcpClient, TcpClient)> pairedClients = new List<(TcpClient, TcpClient)>();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    //Listen for clients
                    var client = listener.AcceptTcpClient();
                    lock (searchingClients)
                    {
                        searchingClients.Add(client);
                        Console.WriteLine(client.Client.Handle + " has connected.");
                        if (searchingClients.Count > 1)
                        {
                            TcpClient c1 = searchingClients[0];
                            TcpClient c2 = searchingClients[1];
                            var pair = (c1, c2);
                            Console.WriteLine($"Paired {c1.Client.Handle} with {c2.Client.Handle}");
                            lock (pairedClients)
                            {
                                pairedClients.Add(pair);
                                SendMessage(c1, "ConfirmConnection", new JsonObject() { { "PID", 0 } });
                                SendMessage(c2, "ConfirmConnection", new JsonObject() { { "PID", 1 } });
                                searchingClients.RemoveRange(0, 2);
                            }

                            //Paired communication
                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    try
                                    {
                                        c1.Client.Send(new byte[0]);
                                        SendMessage(c1, "fakeping", new JsonObject());
                                        List<byte> bytes = new List<byte>();
                                        while (c1.Available > 0)
                                        {
                                            byte[] buffer = new byte[c1.Available];
                                            c1.GetStream().Read(buffer, 0, buffer.Length);
                                            bytes.AddRange(buffer);
                                        }
                                        if (bytes.Count > 0)
                                        {
                                            c2.GetStream().Write(bytes.ToArray());
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        try
                                        {
                                            SendMessage(c1, "disconnect", new JsonObject());
                                            SendMessage(c2, "disconnect", new JsonObject());
                                        }
                                        finally
                                        {
                                            Thread.Sleep(50);
                                            lock (pairedClients)
                                            {
                                                pairedClients.Remove(pair);
                                            }
                                            Console.WriteLine($"{c1.Client.Handle} and {c2.Client.Handle} have disconnected");
                                            c1.Close();
                                            c2.Close();
                                        }
                                    }
                                }
                            });
                            Task.Run(() =>
                            {
                                while (true)
                                {
                                    try
                                    {
                                        c2.Client.Send(new byte[0]);
                                        SendMessage(c2, "fakeping", new JsonObject());
                                        List<byte> bytes = new List<byte>();
                                        while (c2.Available > 0)
                                        {
                                            byte[] buffer = new byte[c2.Available];
                                            c2.GetStream().Read(buffer, 0, buffer.Length);
                                            bytes.AddRange(buffer);
                                        }
                                        if (bytes.Count > 0)
                                        {
                                            c1.GetStream().Write(bytes.ToArray());
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        try
                                        {
                                            SendMessage(c1, "disconnect", new JsonObject());
                                            SendMessage(c2, "disconnect", new JsonObject());
                                        }
                                        finally
                                        {
                                            Thread.Sleep(50);
                                            lock (pairedClients)
                                            {
                                                pairedClients.Remove(pair);
                                            }
                                            Console.WriteLine($"{c1.Client.Handle} and {c2.Client.Handle} have disconnected");
                                            c1.Close();
                                            c2.Close();
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            });

            Task.Run(() =>
            {
                //Ping and remove disconnected clients
                while (true)
                {
                    foreach (var c in searchingClients)
                    {
                        if (c.Connected)
                        {
                            try
                            {
                                c.Client.Send(new byte[0]);
                                SendMessage(c, "ping", new JsonObject());
                            }
                            catch
                            {
                                Console.WriteLine(c.Client.Handle + " has lost connection.");
                                lock (searchingClients)
                                {
                                    c.Close();
                                    searchingClients.Remove(c);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine(c.Client.Handle + " has disconnected.");
                            lock (searchingClients)
                            {
                                c.Close();
                                searchingClients.Remove(c);
                            }
                        }
                    }
                    Thread.Sleep(500);
                }
            });

            //Console controls
            while (true)
            {
                var input = Console.ReadLine();
                if (input != null && input.ToLower() == "players")
                {
                    Console.WriteLine("Here is a list of the current players connected:");
                    foreach (var client in searchingClients) Console.WriteLine(client.Client.Handle);
                }
                if (input != null && input.ToLower() == "matches")
                {
                    Console.WriteLine("Here is a list of the current matches running:");
                    foreach (var pair in pairedClients) Console.WriteLine(pair.Item1.Client.Handle + " vs " + pair.Item2.Client.Handle);
                }
            }

            listener.Stop();
        }

        static void SendMessage(TcpClient client, string type, JsonObject message)
        {
            if (client == null || !client.Connected) return;
            message.Add("type", type);
            List<byte> b = new List<byte>() { (byte)'~' };
            b.AddRange(Encoding.ASCII.GetBytes(message.ToJsonString()));
            client.GetStream().Write(b.ToArray());
        }
    }
}
