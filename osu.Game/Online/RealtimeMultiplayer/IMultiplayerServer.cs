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

        /// <summary>
        /// Transfer the host of the currently joined room to another user in the room.
        /// </summary>
        /// <param name="userId">The new user which is to become host.</param>
        /// <exception cref="NotHostException">A user other than the current host is attempting to transfer host.</exception>
        Task TransferHost(long userId);

        /// <summary>
        /// As the host, update the settings of the currently joined room.
        /// </summary>
        /// <param name="settings">The new settings to apply.</param>
        Task ChangeSettings(MultiplayerRoomSettings settings);

        /// <summary>
        /// Change the local user state in the currently joined room.
        /// </summary>
        /// <param name="newState">The proposed new state.</param>
        /// <exception cref="InvalidStateChange">If the state change requested is not valid, given the previous state or room state.</exception>
        Task ChangeState(MultiplayerUserState newState);

        /// <summary>
        /// As the host of a room, start the match.
        /// </summary>
        /// <exception cref="NotHostException">A user other than the current host is attempting to start the game.</exception>
        Task StartMatch();
    }
}
