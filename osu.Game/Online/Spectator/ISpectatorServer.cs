using System.Threading.Tasks;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// An interface defining the spectator server instance.
    /// </summary>
    public interface ISpectatorServer
    {
        /// <summary>
        /// Signal the start of a new play session.
        /// </summary>
        /// <param name="beatmapId">The beatmap currently being played. Eventually this should be replaced with more complete metadata.</param>
        Task BeginPlaySession(int beatmapId);

        /// <summary>
        /// Send a bundle of frame data for the current play session.
        /// </summary>
        /// <param name="data">The frame data.</param>
        Task SendFrameData(FrameDataBundle data);

        /// <summary>
        /// Signal the end of a play session.
        /// </summary>
        /// <param name="beatmapId">The beatmap that was completed. This should be replaced with a play token once that flow is established.</param>
        Task EndPlaySession(int beatmapId);

        /// <summary>
        /// Request spectating data for the specified user. May be called on multiple users and offline users.
        /// For offline users, a subscription will be created and data will begin streaming on next play.
        /// </summary>
        /// <param name="userId">The user to subscribe to.</param>
        /// <returns></returns>
        Task StartWatchingUser(string userId);

        /// <summary>
        /// Stop requesting spectating data for the specified user. Unsubscribes from receiving further data.
        /// </summary>
        /// <param name="userId">The user to unsubscribe from.</param>
        Task EndWatchingUser(string userId);
    }
}
