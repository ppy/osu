// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A player instance which submits to a room backing. This is generally used by playlists and multiplayer.
    /// </summary>
    public abstract class RoomSubmittingPlayer : SubmittingPlayer
    {
        [Resolved(typeof(Room), nameof(Room.RoomID))]
        protected Bindable<long?> RoomId { get; private set; }

        protected readonly PlaylistItem PlaylistItem;

        protected RoomSubmittingPlayer(PlaylistItem playlistItem, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            PlaylistItem = playlistItem;
        }

        protected override APIRequest<APIScoreToken> CreateTokenRequestRequest() => new CreateRoomScoreRequest(RoomId.Value ?? 0, PlaylistItem.ID, Game.VersionHash);

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token) => new SubmitRoomScoreRequest(token, RoomId.Value ?? 0, PlaylistItem.ID, score.ScoreInfo);
    }
}
