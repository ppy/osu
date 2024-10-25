// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public partial class SpectatorResultsScreen : SoloResultsScreen
    {
        public SpectatorResultsScreen(ScoreInfo score)
            : base(score)
        {
        }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            spectatorClient.OnUserBeganPlaying += userBeganPlaying;
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (userId == Score?.UserID)
            {
                Schedule(() =>
                {
                    if (this.IsCurrentScreen()) this.Exit();
                });
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient.IsNotNull())
                spectatorClient.OnUserBeganPlaying -= userBeganPlaying;
        }
    }
}
