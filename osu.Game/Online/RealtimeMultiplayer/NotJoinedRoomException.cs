using System;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class NotJoinedRoomException : Exception
    {
        public NotJoinedRoomException()
            : base("This user has not yet joined a multiplayer room.")
        {
        }
    }
}
