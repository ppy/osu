// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class MultiSpectatorResultsScreen : SpectatorResultsScreen
    {
        public MultiSpectatorResultsScreen(ScoreInfo score)
            : base(score)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(() => StatisticsPanel.ToggleVisibility(), 1000);
        }

        protected override Task<IEnumerable<ScoreInfo>> FetchScores() => Task.FromResult<IEnumerable<ScoreInfo>>([]);

        protected override Task<IEnumerable<ScoreInfo>> FetchNextPage(int direction) => Task.FromResult<IEnumerable<ScoreInfo>>([]);
    }
}
