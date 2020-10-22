using System.Threading.Tasks;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// An interface defining a spectator client instance.
    /// </summary>
    public interface ISpectatorClient
    {
        /// <summary>
        /// Signals that a user has begun a new play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="beatmapId">The beatmap the user is playing.</param>
        Task UserBeganPlaying(string userId, int beatmapId);

        /// <summary>
        /// Signals that a user has finished a play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="beatmapId">The beatmap the user has finished playing.</param>
        Task UserFinishedPlaying(string userId, int beatmapId);

        /// <summary>
        /// Called when new frames are available for a subscribed user's play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="data">The frame data.</param>
        Task UserSentFrames(string userId, FrameDataBundle data);
    }
}
