// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsPlayer : RoomSubmittingPlayer
    {
        public Action Exited;

        public PlaylistsPlayer(PlaylistItem playlistItem, PlayerConfiguration configuration = null)
            : base(playlistItem, configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            // Sanity checks to ensure that PlaylistsPlayer matches the settings for the current PlaylistItem
            if (Beatmap.Value.BeatmapInfo.OnlineBeatmapID != PlaylistItem.Beatmap.Value.OnlineBeatmapID)
                throw new InvalidOperationException("当前谱面与游玩列表不匹配"); //Current Beatmap does not match PlaylistItem's Beatmap

            if (ruleset.Value.ID != PlaylistItem.Ruleset.Value.ID)
                throw new InvalidOperationException("当前游戏模式与游玩列表不匹配"); //Current Ruleset does not match PlaylistItem's Ruleset

            if (!PlaylistItem.RequiredMods.All(m => Mods.Value.Any(m.Equals)))
                throw new InvalidOperationException("当前Mods与游玩列表所需要的不匹配");
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
            Debug.Assert(RoomId.Value != null);
            return new PlaylistsResultsScreen(score, RoomId.Value.Value, PlaylistItem, true);
        }

        protected override void PrepareScoreForResults()
        {
            base.PrepareScoreForResults();

            Score.ScoreInfo.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }
    }
}
