using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using System;
using System.Collections.Generic;

namespace Symcol.Core.Networking
{
    //TODO: This NEEDS its own clock to avoid fuckery later on with DoubleTime and HalfTime
    public class NetworkingClientHandler : Container
    {
        //30 Seconds by default
        protected virtual double TimeOutTime => 30000;

        protected readonly NetworkingClient ReceiveClient;

        protected readonly NetworkingClient SendClient;

        /// <summary>
        /// Just a client signature basically
        /// </summary>
        public ClientInfo ClientInfo;

        /// <summary>
        /// All Connecting clients
        /// </summary>
        public readonly List<ClientInfo> ConnectingClients = new List<ClientInfo>();

        /// <summary>
        /// All Connected clients
        /// </summary>
        public readonly List<ClientInfo> ConncetedClients = new List<ClientInfo>();

        /// <summary>
        /// Clients waiting in our match
        /// </summary>
        public readonly List<ClientInfo> InMatchClients = new List<ClientInfo>();

        /// <summary>
        /// Clients loaded and ready to start
        /// </summary>
        public readonly List<ClientInfo> LoadedClients = new List<ClientInfo>();

        /// <summary>
        /// Clients ingame playing
        /// </summary>
        public readonly List<ClientInfo> InGameClients = new List<ClientInfo>();

        /// <summary>
        /// Gets hit when we get a Packet
        /// </summary>
        public Action<Packet> OnPacketReceive;

        /// <summary>
        /// (Peer) Call this when we connect to a Host (Includes list of connected peers + Host)
        /// </summary>
        public Action<List<ClientInfo>> OnConnectedToHost;

        /// <summary>
        /// (Host) Whenever a new client Connects
        /// </summary>
        public Action<ClientInfo> OnClientConnect;

        /// <summary>
        /// (Host) Whenever a new client Disconnects
        /// </summary>
        public Action<ClientInfo> OnClientDisconnect;

        /// <summary>
        /// (Host/Peer) When a new Client joins the game
        /// </summary>
        public Action<ClientInfo> OnClientJoin;

        /// <summary>
        /// Receive a full player list
        /// </summary>
        public Action<List<ClientInfo>> OnReceivePlayerList;

        /// <summary>
        /// if we are connected and in a match
        /// </summary>
        public bool InMatch;

        /// <summary>
        /// Are we in a game
        /// </summary>
        public bool InGame;

        /// <summary>
        /// Are we loaded and ready to start?
        /// </summary>
        public bool Loaded;

        /// <summary>
        /// Called to leave an in-progress game
        /// </summary>
        public Action OnAbort;

        /// <summary>
        /// Called to load the game
        /// </summary>
        public Action<List<ClientInfo>> OnLoadGame;

        /// <summary>
        /// Called to start the game once loaded
        /// </summary>
        public Action StartGame;

        public readonly ClientType ClientType;

        public NetworkingClientHandler(ClientType type, string ip, int port = 25570, string thisLocalIp = "0.0.0.0")
        {
            AlwaysPresent = true;

            ClientType = type;

            switch (type)
            {
                case ClientType.Host:
                    ReceiveClient = new NetworkingClient(false, ip, port);
                    break;
                case ClientType.Peer:
                    ReceiveClient = new NetworkingClient(false, thisLocalIp, port);
                    SendClient = new NetworkingClient(true, ip, port);
                    break;
                case ClientType.Server:
                    throw new NotImplementedException();
            }

            Logger.Log("Created a RulesetNetworkingClientHandler", LoggingTarget.Network, LogLevel.Verbose);

            if (ClientInfo == null)
                ClientInfo = new ClientInfo
                {
                    Port = port
                };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ClientType == ClientType.Peer)
                ConnectToHost();
        }

        protected override void Update()
        {
            base.Update();

            PacketRestart:
            Packet p = null;

            if (ReceiveClient.UdpClient.Available > 0)
                p = ReceiveClient.ReceivePacket();

            if (p is BasicPacket packet)
            {
                //Hosts
                if (SendClient == null)
                {
                    if (packet.Disconnect)
                    {
                        OnClientDisconnect?.Invoke(packet.ClientInfo);
                        foreach (ClientInfo client in ConnectingClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                ConnectingClients.Remove(client);
                                Logger.Log("A Connecting Client has Disconnected", LoggingTarget.Network, LogLevel.Verbose);
                                break;
                            }
                        foreach (ClientInfo client in ConncetedClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                ConncetedClients.Remove(client);
                                Logger.Log("A Client has Disconnected", LoggingTarget.Network, LogLevel.Verbose);
                                break;
                            }
                        foreach (ClientInfo client in InMatchClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                InMatchClients.Remove(client);
                                break;
                            }
                        foreach (ClientInfo client in LoadedClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                LoadedClients.Remove(client);
                                break;
                            }
                        foreach (ClientInfo client in InGameClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                InGameClients.Remove(client);
                                break;
                            }
                    }

                    if (packet.Connect)
                    {
                        packet.ClientInfo.StartedTestConnectionTime = Time.Current;
                        ConnectingClients.Add(packet.ClientInfo);

                        NetworkingClient client = new NetworkingClient(true, packet.ClientInfo.IP, packet.ClientInfo.Port);

                        List<ClientInfo> playerList = new List<ClientInfo>
                        {
                            ClientInfo
                        };

                        foreach (ClientInfo clientInfo in ConncetedClients)
                            playerList.Add(clientInfo);

                        client.SendPacket(new BasicPacket(ClientInfo)
                        {
                            PlayerList = playerList,
                            Connect = true
                        });

                        Logger.Log("A Client is Connecting. . .", LoggingTarget.Network, LogLevel.Verbose);
                    }

                    if (packet.RequestPlayerList)
                    {
                        NetworkingClient client = new NetworkingClient(true, packet.ClientInfo.IP, packet.ClientInfo.Port);

                        List<ClientInfo> playerList = new List<ClientInfo>
                        {
                            ClientInfo
                        };

                        foreach (ClientInfo clientInfo in ConncetedClients)
                            playerList.Add(clientInfo);

                        client.SendPacket(new BasicPacket(ClientInfo)
                        {
                            PlayerList = playerList,
                            RequestPlayerList = true
                        });

                        Logger.Log("A Client is Connecting. . .", LoggingTarget.Network, LogLevel.Verbose);
                    }

                    if (packet.Loaded)
                        foreach (ClientInfo client in InMatchClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                Logger.Log("A Client has Loaded and is ready to start", LoggingTarget.Network, LogLevel.Verbose);
                                InMatchClients.Remove(client);
                                LoadedClients.Add(client);
                                break;
                            }

                    if (packet.GameStarted)
                        foreach (ClientInfo client in LoadedClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                Logger.Log("A Client has started!", LoggingTarget.Network, LogLevel.Verbose);
                                LoadedClients.Remove(client);
                                InGameClients.Add(client);
                                break;
                            }

                    if (packet.Test)
                    {
                        foreach (ClientInfo client in ConnectingClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                client.Ping = (int)Time.Current - (int)client.StartedTestConnectionTime;
                                ConnectingClients.Remove(client);
                                ConncetedClients.Add(client);
                                InMatchClients.Add(client);
                                OnClientJoin?.Invoke(client);
                                client.LastConnectionTime = Time.Current;
                                client.ConncetionTryCount = 0;
                                Logger.Log("Successfully connected to a Client! Ping: " + client.Ping, LoggingTarget.Network, LogLevel.Verbose);
                                break;
                            }
                        foreach (ClientInfo client in ConncetedClients)
                            if (client.IP == packet.ClientInfo.IP)
                            {
                                client.Ping = (int)Time.Current - (int)client.StartedTestConnectionTime;
                                client.LastConnectionTime = Time.Current;
                                client.ConncetionTryCount = 0;
                                Logger.Log("Successfully maintained connection to a Client! Ping: " + client.Ping, LoggingTarget.Network, LogLevel.Verbose);
                            }
                    }
                }

                if (InMatchClients.Count == 0 && LoadedClients.Count > 0 && Loaded && !InGame)
                    SendStartGame();

                //Peers
                else if (SendClient != null)
                {
                    if (packet.Connect)
                    {
                        if (!InGame && !InMatch)
                        {
                            InMatch = true;
                            OnConnectedToHost?.Invoke(packet.PlayerList);
                        }
                        Logger.Log("Connected to Host!", LoggingTarget.Network, LogLevel.Verbose);
                    }

                    if (packet.Test)
                    {
                        SendToHost(new BasicPacket(ClientInfo) { Test = true });
                        Logger.Log("Received connection test info from host, returning. . .", LoggingTarget.Network, LogLevel.Verbose);
                    }

                    if (packet.RequestPlayerList)
                        OnReceivePlayerList?.Invoke(packet.PlayerList);

                    if (packet.StartGame)
                    {
                        StartGame?.Invoke();
                        SendToHost(new BasicPacket(ClientInfo) { GameStarted = true });
                        InGame = true;
                    }

                    if (packet.Abort)
                    {
                        OnAbort?.Invoke();
                        InGame = false;
                        Loaded = false;
                    }

                    if (packet.LoadGame)
                    {
                        Logger.Log("Received instructions to LoadGame for " + packet.PlayerList.Count + " players", LoggingTarget.Network, LogLevel.Verbose);
                        OnLoadGame?.Invoke(packet.PlayerList);
                    }
                }
            }

            if (p != null)
                OnPacketReceive?.Invoke(p);

            if (ReceiveClient.UdpClient.Available > 0)
                goto PacketRestart;

            foreach (ClientInfo client in ConnectingClients)
            {
                if (client.LastConnectionTime + TimeOutTime / 10 <= Time.Current && client.ConncetionTryCount == 0)
                {
                    client.StartedTestConnectionTime = Time.Current;
                    TestConnection(client);
                }

                if (client.LastConnectionTime + TimeOutTime / 6 <= Time.Current && client.ConncetionTryCount == 1)
                    TestConnection(client);

                if (client.LastConnectionTime + TimeOutTime / 3 <= Time.Current && client.ConncetionTryCount == 2)
                    TestConnection(client);

                if (client.StartedTestConnectionTime + TimeOutTime <= Time.Current)
                {
                    ConnectingClients.Remove(client);
                    Logger.Log("Connection to a connecting client lost! - " + client.IP + ":" + client.Port, LoggingTarget.Network, LogLevel.Error);
                    break;
                }
            }

            foreach (ClientInfo client in ConncetedClients)
            {
                if (client.LastConnectionTime + TimeOutTime / 6 <= Time.Current && client.ConncetionTryCount == 0)
                {
                    client.StartedTestConnectionTime = Time.Current;
                    TestConnection(client);
                }

                if (client.LastConnectionTime + TimeOutTime / 3 <= Time.Current && client.ConncetionTryCount == 1)
                    TestConnection(client);

                if (client.LastConnectionTime + TimeOutTime / 2 <= Time.Current && client.ConncetionTryCount == 2)
                    TestConnection(client);

                if (client.StartedTestConnectionTime + TimeOutTime <= Time.Current)
                {
                    ConncetedClients.Remove(client);
                    InGameClients.Remove(client);
                    LoadedClients.Remove(client);
                    InGameClients.Remove(client);
                    Logger.Log("Connection to a connected client lost! - " + client.IP + ":" + client.Port, LoggingTarget.Network, LogLevel.Error);
                    break;
                }
            }
        }

        /// <summary>
        /// Poke!
        /// </summary>
        /// <param name="clientInfo"></param>
        protected void TestConnection(ClientInfo clientInfo)
        {
            clientInfo.ConncetionTryCount++;
            NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
            client.SendPacket(new BasicPacket(ClientInfo) { Test = true });
            Logger.Log("Testing a client's connection - " + clientInfo.IP + ":" + clientInfo.Port, LoggingTarget.Network, LogLevel.Verbose);
        }

        public void RequestPlayerList()
        {
            BasicPacket packet = new BasicPacket(ClientInfo) { RequestPlayerList = true };
            SendToHost(packet);
        }

        /// <summary>
        /// Tell peers to start loading game
        /// </summary>
        public virtual void StartLoadingGame()
        {
            if (SendClient == null)
            {
                BasicPacket packet = new BasicPacket(ClientInfo) { LoadGame = true };

                foreach (ClientInfo client in InMatchClients)
                    packet.PlayerList.Add(client);
                packet.PlayerList.Add(ClientInfo);

                SendToInMatchClients(packet);

                OnLoadGame?.Invoke(packet.PlayerList);
            }
            else
                Logger.Log("Called StartLoadingGame - We are not the Host!", LoggingTarget.Network, LogLevel.Verbose);
        }

        /// <summary>
        /// Call this when the game is Loaded and ready to be started
        /// </summary>
        public virtual void GameLoaded()
        {
            Loaded = true;
            SendToHost(new BasicPacket(ClientInfo) { Loaded = true });
        }

        /// <summary>
        /// Connects to the Host
        /// </summary>
        public virtual void ConnectToHost()
        {
            SendToHost(new BasicPacket(ClientInfo) { Connect = true });
            Logger.Log("Attempting conection to Host. . .", LoggingTarget.Network, LogLevel.Verbose);
        }

        /// <summary>
        /// Tell peers to start and starts ours
        /// </summary>
        public virtual void SendStartGame()
        {
            if (SendClient == null)
            {
                SendToLoadedClients(new BasicPacket(ClientInfo) { StartGame = true });
                InGame = true;
                Logger.Log("Sending Start Game", LoggingTarget.Network, LogLevel.Verbose);
            }
            StartGame?.Invoke();
        }

        /// <summary>
        /// Send a Packet to the Host
        /// </summary>
        /// <param name="packet"></param>
        public void SendToHost(Packet packet)
        {
            if (SendClient != null)
                SendClient.SendPacket(packet);
        }

        /// <summary>
        /// Send a Packet to all Connecting clients
        /// </summary>
        /// <param name="packet"></param>
        public void SendToConnectingClients(Packet packet)
        {
            if (SendClient == null)
                foreach (ClientInfo clientInfo in ConnectingClients)
                {
                    NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
                    client.SendPacket(packet);
                }
        }

        /// <summary>
        /// Send a Packet to all clients Connected and waiting
        /// </summary>
        /// <param name="packet"></param>
        public void SendToConnectedClients(Packet packet)
        {
            if (SendClient == null)
                foreach (ClientInfo clientInfo in ConncetedClients)
                {
                    NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
                    client.SendPacket(packet);
                }
        }

        /// <summary>
        /// Send a Packet to all clients In this Match
        /// </summary>
        /// <param name="packet"></param>
        public void SendToInMatchClients(Packet packet)
        {
            if (SendClient == null)
                foreach (ClientInfo clientInfo in InMatchClients)
                {
                    NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
                    client.SendPacket(packet);
                }
        }

        /// <summary>
        /// Send a Packet to all clients Loaded
        /// </summary>
        /// <param name="packet"></param>
        public void SendToLoadedClients(Packet packet)
        {
            if (SendClient == null)
                foreach (ClientInfo clientInfo in LoadedClients)
                {
                    NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
                    client.SendPacket(packet);
                }
        }

        /// <summary>
        /// Send a Packet to all clients InGame
        /// </summary>
        /// <param name="packet"></param>
        public void SendToInGameClients(Packet packet)
        {
            if (SendClient == null)
                foreach (ClientInfo clientInfo in InGameClients)
                {
                    NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
                    client.SendPacket(packet);
                }
        }

        /// <summary>
        /// Send a Packet to ALL clients we know
        /// </summary>
        /// <param name="packet"></param>
        public void SendToAllClients(Packet packet)
        {
            if (SendClient == null)
            {
                SendToConnectingClients(packet);
                SendToConnectedClients(packet);
            }
        }

        /// <summary>
        /// Send tto all but the one that sent it
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="playerID"></param>
        public void ShareWithOtherPeers(Packet packet)
        {
            if (SendClient == null)
                foreach (ClientInfo clientInfo in InGameClients)
                    if (packet.ClientInfo.IP != clientInfo.IP)
                    {
                        NetworkingClient client = new NetworkingClient(true, clientInfo.IP, clientInfo.Port);
                        client.SendPacket(packet);
                    }
        }

        public virtual void AbortGame()
        {
            SendToLoadedClients(new BasicPacket(ClientInfo) { Abort = true });
            SendToInGameClients(new BasicPacket(ClientInfo) { Abort = true });

            restart:
            foreach (ClientInfo client in LoadedClients)
            {
                LoadedClients.Remove(client);
                InMatchClients.Add(client);
                goto restart;
            }
            foreach (ClientInfo client in InGameClients)
            {
                InGameClients.Remove(client);
                InMatchClients.Add(client);
                goto restart;
            }

            InGame = false;
            Loaded = false;

            OnAbort?.Invoke();
        }

        public virtual void Disconnect()
        {
            Packet packet = new BasicPacket(ClientInfo) { Disconnect = true };

            OnAbort?.Invoke();
            InMatch = false;
            InGame = false;
            Loaded = false;

            if (SendClient == null)
            {
                SendToConnectingClients(packet);
                SendToConnectedClients(packet);
            }
            else
                SendToHost(packet);
        }

        /// <summary>
        /// Die
        /// </summary>
        /// <param name="isDisposing"></param>
        protected override void Dispose(bool isDisposing)
        {
            ReceiveClient?.Clear();

            if (SendClient != null)
            {
                SendToHost(new BasicPacket(ClientInfo) { Disconnect = true });
                SendClient.Clear();
            }

            base.Dispose(isDisposing);
        }
    }

    public enum ClientType
    {
        Host,
        Peer,
        Server
    }
}
