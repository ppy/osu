// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Game.Online.Spectator;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;

namespace osu.Game.Screens.Play
{
    public partial class SoloSpectatorPlayer : SpectatorPlayer
    {
        private readonly Score score;

        [Cached(typeof(IGameplayLeaderboardProvider))]
        private SoloGameplayLeaderboardProvider leaderboardProvider = new SoloGameplayLeaderboardProvider();

        protected override UserActivity InitialActivity => new UserActivity.SpectatingUser(Score.ScoreInfo);

        public SoloSpectatorPlayer(Score score)
            : base(score, new PlayerConfiguration { AllowUserInteraction = false, ShowLeaderboard = true })
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SpectatorClient.OnUserBeganPlaying += userBeganPlaying;

            AddInternal(leaderboardProvider);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            SpectatorClient.OnUserBeganPlaying -= userBeganPlaying;

            return base.OnExiting(e);
        }

        private void userBeganPlaying(int userId, SpectatorState state)
        {
            if (userId != score.ScoreInfo.UserID) return;

            Schedule(() =>
            {
                if (this.IsCurrentScreen()) this.Exit();
            });
        }

        #region Fail handling

        protected override bool CheckModsAllowFailure()
        {
            if (!allowFail)
                return false;

            return base.CheckModsAllowFailure();
        }

        private bool allowFail;

        /// <summary>
        /// Should be called when it is apparent that the player being spectated has failed.
        /// This will subsequently stop blocking the fail screen from displaying (usually done out of safety).
        /// </summary>
        public void AllowFail() => allowFail = true;

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (SpectatorClient.IsNotNull())
                SpectatorClient.OnUserBeganPlaying -= userBeganPlaying;
        }
    }
}
