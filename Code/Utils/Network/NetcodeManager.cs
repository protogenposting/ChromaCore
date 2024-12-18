using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using RCArena.Code.Effects;
using RCArena.Code.Objects;
using RCArena.Code.Scenes;

namespace RCArena.Code.Utils.Network
{
    public class NetcodeManager
    {
        public bool rollingBack = false;

        InGame scene;
        public int playerCount;
        public int myPlayerID;
        const int InputDelay = 3;
        bool shouldRollback = false;
        List<List<bool[]>> inputHistory = new();
        Dictionary<(int playerID, int frame), bool[]> earlyInputs = new();
        public List<NetConnection> connections;
        public GameState rollbackState;
        public int rollbackFrame;
        public int[] frameCounters;
        public Fighter[] players;

        public NetcodeManager(InGame scene, NetConnection connection)
        {
            this.scene = scene;
            connections = new List<NetConnection>() { connection };
            myPlayerID = connection.playerID;
            frameCounters = new int[connections.Count + 1];
        }

        public void Update()
        {
            List<bool[]> bools = new List<bool[]>();
            for (int i = 0; i < players.Length; i++)
            {
                if (i == myPlayerID) bools.Add(players[i].input.GetKeysDown(new InputState(players[i].input.id)));
                else
                {
                    bool[] b = new bool[Controller.NumKeys];
                    if (earlyInputs.ContainsKey((i, frameCounters[myPlayerID])))
                    {
                        earlyInputs[(i, frameCounters[myPlayerID])].CopyTo(b, 0);
                        earlyInputs.Remove((i, frameCounters[myPlayerID]));
                    }
                    else if (inputHistory.Count > 0) inputHistory[^1][i].CopyTo(b, 0);
                    bools.Add(b);
                }
            }
            inputHistory.Add(bools);

            //Send Local Inputs
            JsonObject json = new JsonObject
            {
                { "value", JsonSerializer.SerializeToNode(inputHistory[^1][myPlayerID]) },
                { "frame", JsonSerializer.SerializeToNode(frameCounters[myPlayerID]) },
                { "pID", JsonSerializer.SerializeToNode(myPlayerID) },
            };
            foreach (NetConnection con in connections) con.SendMessage("input", json);

            //Receive Remote Inputs
            ReceiveInputs();

            //Catchup if behind remote frame
            int maxRemoteFrame = 0;
            for (int i = 0; i < frameCounters.Length; i++) if (i != myPlayerID && frameCounters[i] > maxRemoteFrame) maxRemoteFrame = frameCounters[i];
            if (frameCounters[myPlayerID] < maxRemoteFrame)
            {
                for (int i = 0; i < maxRemoteFrame - frameCounters[myPlayerID]; i++)
                {
                    bools = new List<bool[]>();
                    for (int j = 0; j < players.Length; j++)
                    {
                        if (j == myPlayerID) bools.Add(players[j].input.GetKeysDown(new InputState(players[j].input.id)));
                        else
                        {
                            bool[] b = new bool[Controller.NumKeys];
                            if (earlyInputs.ContainsKey((i, frameCounters[myPlayerID] + i)))
                            {
                                earlyInputs[(j, frameCounters[myPlayerID] + i)].CopyTo(b, 0);
                                earlyInputs.Remove((j, frameCounters[myPlayerID] + i));
                            }
                            else if (inputHistory.Count > 0) inputHistory[^1][j].CopyTo(b, 0);
                            bools.Add(b);
                        }
                    }
                    inputHistory.Add(bools);
                    JsonObject json2 = new JsonObject
                    {
                        { "value", JsonSerializer.SerializeToNode(inputHistory[^1][myPlayerID]) },
                        { "frame", JsonSerializer.SerializeToNode(frameCounters[myPlayerID] + i) },
                        { "pID", JsonSerializer.SerializeToNode(myPlayerID) },
                    };
                    foreach (NetConnection con in connections) con.SendMessage("input", json2);
                }
                frameCounters[myPlayerID] = maxRemoteFrame;
                shouldRollback = true;
            }

            //Rollback
            rollingBack = true;
            if (shouldRollback || frameCounters[myPlayerID] % 20 == 0)
            {
                rollbackState.LoadState(scene);
                for (int i = rollbackFrame + 1; i <= frameCounters[myPlayerID]; i++)
                {
                    if (i > InputDelay && Game.Instance.screenTransition == null && scene.introTimer <= 0 && scene.winner == 0)
                    {
                        for (int j = 0; j < players.Length; j++)
                        {
                            players[j].input.UpdateKeys(inputHistory[i - InputDelay][j], players[j].direction);
                        }
                    }
                    else foreach (var player in players) player.input.ClearAllInputs();
                    scene.AdvanceFrame();

                    if (i == frameCounters.Min() || rollbackFrame < frameCounters.Min() && i == frameCounters[myPlayerID])
                    {
                        rollbackState = new GameState(scene);
                        rollbackFrame = i;
                    }
                }
            }

            //Regular Update
            rollingBack = false;
            if (frameCounters[myPlayerID] > InputDelay && Game.Instance.screenTransition == null && scene.introTimer <= 0 && scene.winner == 0)
            {
                for (int j = 0; j < players.Length; j++)
                {
                    players[j].input.UpdateKeys(inputHistory[frameCounters[myPlayerID] - InputDelay][j], players[j].direction);
                }
            }
            else foreach (var player in players) player.input.ClearAllInputs();
            scene.AdvanceFrame();

            frameCounters[myPlayerID]++;
        }

        void ReceiveInputs()
        {
            List<JsonDocument> messages = new List<JsonDocument>();
            foreach (NetConnection con in connections) messages.AddRange(con.ReceiveMessages());
            foreach (JsonDocument message in messages)
            {
                string type = message.RootElement.GetProperty("type").GetString();
                if (type == "input")
                {
                    bool[] b = message.RootElement.GetProperty("value").Deserialize<bool[]>();
                    int frame = message.RootElement.GetProperty("frame").GetInt32();
                    int id = message.RootElement.GetProperty("pID").GetInt32();

                    if (frame >= inputHistory.Count)
                    {
                        earlyInputs.Add((id, frame), b);
                    }
                    else if (!inputHistory[frame][id].SequenceEqual(b))
                    {
                        shouldRollback = true;
                        inputHistory[frame][id] = b;

                        if (frame > frameCounters[id])
                        {
                            for (int i = frame; i < inputHistory.Count; i++)
                            {
                                inputHistory[i][id] = b;
                            }
                        }
                    }

                    frameCounters[id] = Math.Max(frame, frameCounters[id]);
                }
            }
        }
    }

    public struct GameState
    {
        Dictionary<Fighter, Dictionary<string, object>> playerState = new Dictionary<Fighter, Dictionary<string, object>>();
        Dictionary<Hitbox, Dictionary<string, object>> hitboxState = new Dictionary<Hitbox, Dictionary<string, object>>();
        Dictionary<GameObject, Dictionary<string, object>> objectState = new Dictionary<GameObject, Dictionary<string, object>>();
        Dictionary<Particle, Dictionary<string, object>> particleState = new Dictionary<Particle, Dictionary<string, object>>();
        private List<(SoundEffectInstance, Vector2, float)> soundState = new();
        Vector2 camPosition;
        int versusTimer;
        int introTimer;
        Fighter playerInCorner;
        int hitpause;
        int prepareHitpause;
        int winner;
        int winScreenTimer;

        public GameState(InGame scene)
        {
            foreach (Fighter o in scene.players)
            {
                playerState.Add(o, o.GetState());
            }
            foreach (Hitbox h in scene.hitboxes)
            {
                hitboxState.Add(h, h.GetState());
            }
            foreach (GameObject o in scene.miscObjects)
            {
                objectState.Add(o, o.GetState());
            }
            foreach (Particle p in scene.particles)
            {
                particleState.Add(p, p.GetState());
            }
            camPosition = scene.camera.position;

            soundState = new List<(SoundEffectInstance, Vector2, float)>(scene.soundEffects);

            versusTimer = scene.versusTimer;
            introTimer = scene.introTimer;
            playerInCorner = scene.playerInCorner;
            hitpause = scene.hitpause;
            prepareHitpause = scene.prepareHitpause;
            winner = scene.winner;
            winScreenTimer = scene.winScreenTimer;
        }

        public void LoadState(InGame scene)
        {
            scene.players = playerState.Keys.ToList();
            foreach (var p in playerState)
            {
                p.Key.LoadState(p.Value);
            }
            scene.hitboxes = hitboxState.Keys.ToList();
            foreach (var h in hitboxState)
            {
                h.Key.LoadState(h.Value);
            }
            scene.miscObjects = objectState.Keys.ToList();
            foreach (var o in objectState)
            {
                o.Key.LoadState(o.Value);
            }
            scene.particles = particleState.Keys.ToList();
            foreach (var p in particleState)
            {
                p.Key.LoadState(p.Value);
            }
            scene.camera.position = camPosition;

            scene.versusTimer = versusTimer;
            scene.introTimer = introTimer;
            scene.playerInCorner = playerInCorner;
            scene.hitpause = hitpause;
            scene.prepareHitpause = prepareHitpause;
            scene.winner = winner;
            scene.winScreenTimer = winScreenTimer;

            foreach (var sound in scene.soundEffects.ToArray())
            {
                //if (!soundState.Contains(sound))
                //{
                //    sound.Item1.Stop();
                //    scene.soundEffects.Remove(sound);
                //}
            }
        }
    }
}
