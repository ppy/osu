// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class SoloSpectatorPlayer : SpectatorPlayer
    {
        private readonly Score score;

        public SoloSpectatorPlayer(Score score, PlayerConfiguration configuration = null)
            : base(score, configuration)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SpectatorClient.OnUserBeganPlaying += userBeganPlaying;
        }

        public override bool OnExiting(IScreen next)
        {
            SpectatorClient.OnUserBeganPlaying -= userBeganPlaying;

            return base.OnExiting(next);
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (userId != score.ScoreInfo.UserID) return;

            Schedule(() =>
            {
                if (this.IsCurrentScreen()) this.Exit();
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (SpectatorClient != null)
                SpectatorClient.OnUserBeganPlaying -= userBeganPlaying;
        }
    }
}
