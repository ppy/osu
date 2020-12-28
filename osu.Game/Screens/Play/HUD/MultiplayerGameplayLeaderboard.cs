// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    [LongRunningLoad]
    public class MultiplayerGameplayLeaderboard : GameplayLeaderboard
    {
        private readonly ScoreProcessor scoreProcessor;

        private readonly Dictionary<int, TrackedUserData> userScores = new Dictionary<int, TrackedUserData>();

        [Resolved]
        private SpectatorStreamingClient streamingClient { get; set; }

        [Resolved]
        private StatefulMultiplayerClient multiplayerClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private Bindable<ScoringMode> scoringMode;

        private readonly BindableList<int> playingUsers;

        /// <summary>
        /// Construct a new leaderboard.
        /// </summary>
        /// <param name="scoreProcessor">A score processor instance to handle score calculation for scores of users in the match.</param>
        /// <param name="userIds">IDs of all users in this match.</param>
        public MultiplayerGameplayLeaderboard(ScoreProcessor scoreProcessor, int[] userIds)
        {
            // todo: this will eventually need to be created per user to support different mod combinations.
            this.scoreProcessor = scoreProcessor;

            // todo: this will likely be passed in as User instances.
            playingUsers = new BindableList<int>(userIds);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            streamingClient.OnNewFrames += handleIncomingFrames;

            foreach (var userId in playingUsers)
            {
                streamingClient.WatchUser(userId);

                // probably won't be required in the final implementation.
                var resolvedUser = userLookupCache.GetUserAsync(userId).Result;

                var trackedUser = new TrackedUserData();

                userScores[userId] = trackedUser;
                var leaderboardScore = AddPlayer(resolvedUser, resolvedUser.Id == api.LocalUser.Value.Id);

                ((IBindable<double>)leaderboardScore.Accuracy).BindTo(trackedUser.Accuracy);
                ((IBindable<double>)leaderboardScore.TotalScore).BindTo(trackedUser.Score);
                ((IBindable<int>)leaderboardScore.Combo).BindTo(trackedUser.CurrentCombo);
                ((IBindable<bool>)leaderboardScore.HasQuit).BindTo(trackedUser.UserQuit);
            }

            scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);
            scoringMode.BindValueChanged(updateAllScores, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // BindableList handles binding in a really bad way (Clear then AddRange) so we need to do this manually..
            foreach (int userId in playingUsers)
            {
                if (!multiplayerClient.PlayingUsers.Contains(userId))
                    usersChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { userId }));
            }

            playingUsers.BindTo(multiplayerClient.PlayingUsers);
            playingUsers.BindCollectionChanged(usersChanged);
        }

        private void usersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (var userId in e.OldItems.OfType<int>())
                    {
                        streamingClient.StopWatchingUser(userId);

                        if (userScores.TryGetValue(userId, out var trackedData))
                            trackedData.MarkUserQuit();
                    }

                    break;
            }
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (streamingClient != null)
            {
                foreach (var user in playingUsers)
                {
                    streamingClient.StopWatchingUser(user);
                }

                streamingClient.OnNewFrames -= handleIncomingFrames;
            }
        }

        private class TrackedUserData
        {
            public IBindableNumber<double> Score => score;

            private readonly BindableDouble score = new BindableDouble();

            public IBindableNumber<double> Accuracy => accuracy;

            private readonly BindableDouble accuracy = new BindableDouble(1);

            public IBindableNumber<int> CurrentCombo => currentCombo;

            private readonly BindableInt currentCombo = new BindableInt();

            public IBindable<bool> UserQuit => userQuit;

            private readonly BindableBool userQuit = new BindableBool();

            [CanBeNull]
            public FrameHeader LastHeader;

            public void MarkUserQuit() => userQuit.Value = true;

            public void UpdateScore(ScoreProcessor processor, ScoringMode mode)
            {
                if (LastHeader == null)
                    return;

                score.Value = processor.GetImmediateScore(mode, LastHeader.MaxCombo, LastHeader.Statistics);
                accuracy.Value = LastHeader.Accuracy;
                currentCombo.Value = LastHeader.Combo;
            }
        }
    }
}
