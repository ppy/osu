// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
        protected readonly Dictionary<int, TrackedUserData> UserScores = new Dictionary<int, TrackedUserData>();

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly ScoreProcessor scoreProcessor;
        private readonly BindableList<int> playingUsers;
        private Bindable<ScoringMode> scoringMode;

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
            scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);

            foreach (var userId in playingUsers)
            {
                // probably won't be required in the final implementation.
                var resolvedUser = userLookupCache.GetUserAsync(userId).Result;

                var trackedUser = CreateUserData(userId, scoreProcessor);
                trackedUser.ScoringMode.BindTo(scoringMode);

                var leaderboardScore = AddPlayer(resolvedUser, resolvedUser?.Id == api.LocalUser.Value.Id);
                leaderboardScore.Accuracy.BindTo(trackedUser.Accuracy);
                leaderboardScore.TotalScore.BindTo(trackedUser.Score);
                leaderboardScore.Combo.BindTo(trackedUser.CurrentCombo);
                leaderboardScore.HasQuit.BindTo(trackedUser.UserQuit);

                UserScores[userId] = trackedUser;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // BindableList handles binding in a really bad way (Clear then AddRange) so we need to do this manually..
            foreach (int userId in playingUsers)
            {
                spectatorClient.WatchUser(userId);

                if (!multiplayerClient.CurrentMatchPlayingUserIds.Contains(userId))
                    usersChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { userId }));
            }

            playingUsers.BindTo(multiplayerClient.CurrentMatchPlayingUserIds);
            playingUsers.BindCollectionChanged(usersChanged);

            // this leaderboard should be guaranteed to be completely loaded before the gameplay starts (is a prerequisite in MultiplayerPlayer).
            spectatorClient.OnNewFrames += handleIncomingFrames;
        }

        private void usersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (var userId in e.OldItems.OfType<int>())
                    {
                        spectatorClient.StopWatchingUser(userId);

                        if (UserScores.TryGetValue(userId, out var trackedData))
                            trackedData.MarkUserQuit();
                    }

                    break;
            }
        }

        private void handleIncomingFrames(int userId, FrameDataBundle bundle) => Schedule(() =>
        {
            if (!UserScores.TryGetValue(userId, out var trackedData))
                return;

            trackedData.Frames.Add(new TimedFrame(bundle.Frames.First().Time, bundle.Header));
            trackedData.UpdateScore();
        });

        protected virtual TrackedUserData CreateUserData(int userId, ScoreProcessor scoreProcessor) => new TrackedUserData(userId, scoreProcessor);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                foreach (var user in playingUsers)
                {
                    spectatorClient.StopWatchingUser(user);
                }

                spectatorClient.OnNewFrames -= handleIncomingFrames;
            }
        }

        protected class TrackedUserData
        {
            public readonly int UserId;
            public readonly ScoreProcessor ScoreProcessor;

            public readonly BindableDouble Score = new BindableDouble();
            public readonly BindableDouble Accuracy = new BindableDouble(1);
            public readonly BindableInt CurrentCombo = new BindableInt();
            public readonly BindableBool UserQuit = new BindableBool();

            public readonly IBindable<ScoringMode> ScoringMode = new Bindable<ScoringMode>();

            public readonly List<TimedFrame> Frames = new List<TimedFrame>();

            public TrackedUserData(int userId, ScoreProcessor scoreProcessor)
            {
                UserId = userId;
                ScoreProcessor = scoreProcessor;

                ScoringMode.BindValueChanged(_ => UpdateScore());
            }

            public void MarkUserQuit() => UserQuit.Value = true;

            public virtual void UpdateScore()
            {
                if (Frames.Count == 0)
                    return;

                SetFrame(Frames.Last());
            }

            protected void SetFrame(TimedFrame frame)
            {
                var header = frame.Header;

                Score.Value = ScoreProcessor.GetImmediateScore(ScoringMode.Value, header.MaxCombo, header.Statistics);
                Accuracy.Value = header.Accuracy;
                CurrentCombo.Value = header.Combo;
            }
        }

        protected class TimedFrame : IComparable<TimedFrame>
        {
            public readonly double Time;
            public readonly FrameHeader Header;

            public TimedFrame(double time)
            {
                Time = time;
            }

            public TimedFrame(double time, FrameHeader header)
            {
                Time = time;
                Header = header;
            }

            public int CompareTo(TimedFrame other) => Time.CompareTo(other.Time);
        }
    }
}
