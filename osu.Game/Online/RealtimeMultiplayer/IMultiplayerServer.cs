using System.Threading.Tasks;

namespace osu.Game.Online.RealtimeMultiplayer
{
    /// <summary>
    /// An interface defining the spectator server instance.
    /// </summary>
    public interface IMultiplayerServer
    {
        /// <summary>
        /// Request to join a multiplayer room.
        /// </summary>
        /// <param name="roomId">The databased room ID.</param>
        /// <exception cref="UserAlreadyInMultiplayerRoom">If the user is already in the requested (or another) room.</exception>
        Task<MultiplayerRoom> JoinRoom(long roomId);

        /// <summary>
        /// Request to leave the currently joined room.
        /// </summary>
        Task LeaveRoom();
    }
}