// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengePlayer : PlaylistsPlayer
    {
        protected override UserActivity InitialActivity => new UserActivity.PlayingDailyChallenge(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public DailyChallengePlayer(Room room, PlaylistItem playlistItem, PlayerConfiguration? configuration = null)
            : base(room, playlistItem, configuration)
        {
        }
    }
}
