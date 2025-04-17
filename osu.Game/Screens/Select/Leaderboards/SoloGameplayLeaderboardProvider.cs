// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.Select.Leaderboards
{
    public partial class SoloGameplayLeaderboardProvider : Component, IGameplayLeaderboardProvider
    {
        public bool IsPartial { get; private set; }

        public IBindableList<IGameplayLeaderboardScore> Scores => scores;
        private readonly BindableList<IGameplayLeaderboardScore> scores = new BindableList<IGameplayLeaderboardScore>();

        [BackgroundDependencyLoader]
        private void load(LeaderboardManager? leaderboardManager, GameplayState? gameplayState)
        {
            var globalScores = leaderboardManager?.Scores.Value;

            IsPartial = leaderboardManager?.CurrentCriteria?.Scope != BeatmapLeaderboardScope.Local && globalScores?.TopScores.Count >= 50;

            if (globalScores != null)
            {
                foreach (var topScore in globalScores.AllScores.OrderByTotalScore())
                    scores.Add(new GameplayLeaderboardScore(topScore, false));
            }

            if (gameplayState != null)
            {
                scores.Add(new GameplayLeaderboardScore(gameplayState.Score.ScoreInfo.User, gameplayState.ScoreProcessor, true)
                {
                    // Local score should always show lower than any existing scores in cases of ties.
                    DisplayOrder = { Value = long.MaxValue }
                });
            }
        }
    }
}
