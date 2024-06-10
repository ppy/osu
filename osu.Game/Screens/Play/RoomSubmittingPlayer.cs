// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A player instance which submits to a room backing. This is generally used by playlists and multiplayer.
    /// </summary>
    public abstract partial class RoomSubmittingPlayer : SubmittingPlayer
    {
        protected readonly PlaylistItem PlaylistItem;
        protected readonly Room Room;

        protected RoomSubmittingPlayer(Room room, PlaylistItem playlistItem, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            Room = room;
            PlaylistItem = playlistItem;
        }

        protected override APIRequest<APIScoreToken> CreateTokenRequest()
        {
            if (!(Room.RoomID.Value is long roomId))
                return null;

            int beatmapId = Beatmap.Value.BeatmapInfo.OnlineID;
            int rulesetId = Ruleset.Value.OnlineID;

            if (beatmapId <= 0)
                return null;

            if (!Ruleset.Value.IsLegacyRuleset())
                return null;

            return new CreateRoomScoreRequest(roomId, PlaylistItem.ID, Beatmap.Value.BeatmapInfo, rulesetId, Game.VersionHash);
        }

        protected override APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token)
        {
            Debug.Assert(Room.RoomID.Value != null);
            return new SubmitRoomScoreRequest(score.ScoreInfo, token, Room.RoomID.Value.Value, PlaylistItem.ID);
        }
    }
}
