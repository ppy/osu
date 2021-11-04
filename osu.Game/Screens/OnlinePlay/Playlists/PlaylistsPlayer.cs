// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsPlayer : RoomSubmittingPlayer
    {
        public Action Exited;

        protected override UserActivity InitialActivity => new UserActivity.InPlaylistGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public PlaylistsPlayer(Room room, PlaylistItem playlistItem, PlayerConfiguration configuration = null)
            : base(room, playlistItem, configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            // Sanity checks to ensure that PlaylistsPlayer matches the settings for the current PlaylistItem
            if (Beatmap.Value.BeatmapInfo.OnlineBeatmapID != PlaylistItem.Beatmap.Value.OnlineID)
                throw new InvalidOperationException("Current Beatmap does not match PlaylistItem's Beatmap");

            if (ruleset.Value.ID != PlaylistItem.Ruleset.Value.ID)
                throw new InvalidOperationException("Current Ruleset does not match PlaylistItem's Ruleset");

            if (!PlaylistItem.RequiredMods.All(m => Mods.Value.Any(m.Equals)))
                throw new InvalidOperationException("Current Mods do not match PlaylistItem's RequiredMods");
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            Exited?.Invoke();

            return false;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(Room.RoomID.Value != null);
            return new PlaylistsResultsScreen(score, Room.RoomID.Value.Value, PlaylistItem, true);
        }

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            Score.ScoreInfo.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }
    }
}
