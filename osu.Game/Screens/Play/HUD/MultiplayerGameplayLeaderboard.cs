// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public class MultiplayerGameplayLeaderboard : GameplayLeaderboard
    {
        private readonly ScoreProcessor scoreProcessor;

        /// <summary>
        /// Construct a new leaderboard.
        /// </summary>
        /// <param name="scoreProcessor">A score processor instance to handle score calculation for scores of users in the match.</param>
        public MultiplayerGameplayLeaderboard(ScoreProcessor scoreProcessor)
        {
            this.scoreProcessor = scoreProcessor;
        }

        [Resolved]
        private SpectatorStreamingClient streamingClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly Dictionary<int, TrackedUserData> userScores = new Dictionary<int, TrackedUserData>();

        private Bindable<ScoringMode> scoringMode;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            streamingClient.OnNewFrames += handleIncomingFrames;

            foreach (var user in streamingClient.PlayingUsers)
            {
                streamingClient.WatchUser(user);
                var resolvedUser = userLookupCache.GetUserAsync(user).Result;

                var trackedUser = new TrackedUserData();

                userScores[user] = trackedUser;
                AddPlayer(trackedUser.Score, resolvedUser);
            }

            scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
            scoringMode.BindValueChanged(updateAllScores, true);
        }

        private void updateAllScores(ValueChangedEvent<ScoringMode> mode)
        {
            foreach (var trackedData in userScores.Values)
                trackedData.UpdateScore(scoreProcessor, mode.NewValue);
        }

        private void handleIncomingFrames(int userId, FrameDataBundle bundle)
        {
            if (userScores.TryGetValue(userId, out var trackedData))
            {
                trackedData.LastHeader = bundle.Header;
                trackedData.UpdateScore(scoreProcessor, scoringMode.Value);
            }
        }

        private class TrackedUserData
        {
            public readonly BindableDouble Score = new BindableDouble();

            public readonly BindableDouble Accuracy = new BindableDouble();

            public readonly BindableInt CurrentCombo = new BindableInt();

            [CanBeNull]
            public FrameHeader LastHeader;

            public void UpdateScore(ScoreProcessor processor, ScoringMode mode)
            {
                if (LastHeader == null)
                    return;

                (Score.Value, Accuracy.Value) = processor.GetScoreAndAccuracy(mode, LastHeader.MaxCombo, LastHeader.Statistics);

                CurrentCombo.Value = LastHeader.Combo;
            }
        }
    }
}
