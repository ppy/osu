using System;
using System.Collections.Generic;

namespace Symcol.Core.Networking
{
    [Serializable]
    public class BasicPacket : Packet
    {
        /// <summary>
        /// Ask host if we can connect
        /// </summary>
        public bool Connect;

        /// <summary>
        /// Tell the host we are breaking up
        /// </summary>
        public bool Disconnect;

        /// <summary>
        /// Testing Connection
        /// </summary>
        public bool Test;

        /// <summary>
        /// Send a force exit to others
        /// </summary>
        public bool Abort;

        /// <summary>
        /// PreLoad the game
        /// </summary>
        public bool LoadGame;

        /// <summary>
        /// Request a list of all players from Host
        /// </summary>
        public bool RequestPlayerList;

        /// <summary>
        /// List of players in this match that we should account for
        /// </summary>
        public List<ClientInfo> PlayerList = new List<ClientInfo>();

        /// <summary>
        /// Tell Host we are PreLoaded
        /// </summary>
        public bool Loaded;

        /// <summary>
        /// Start the game already!
        /// </summary>
        public bool StartGame;

        /// <summary>
        /// Send to host when game started
        /// </summary>
        public bool GameStarted;

        public BasicPacket(ClientInfo clientInfo) : base(clientInfo)
        {
        }
    }
}
