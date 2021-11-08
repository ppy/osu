// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    [LongRunningLoad]
    public class MultiplayerGameplayLeaderboard : GameplayLeaderboard
    {
        protected readonly Dictionary<int, TrackedUserData> UserScores = new Dictionary<int, TrackedUserData>();

        public readonly SortedDictionary<int, BindableInt> TeamScores = new SortedDictionary<int, BindableInt>();

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        [Resolved]
        private UserLookupCache userLookupCache { get; set; }

        private readonly ScoreProcessor scoreProcessor;
        private readonly MultiplayerRoomUser[] playingUsers;
        private Bindable<ScoringMode> scoringMode;

        private readonly IBindableList<int> playingUserIds = new BindableList<int>();

        private bool hasTeams => TeamScores.Count > 0;

        /// <summary>
        /// Construct a new leaderboard.
        /// </summary>
        /// <param name="scoreProcessor">A score processor instance to handle score calculation for scores of users in the match.</param>
        /// <param name="users">IDs of all users in this match.</param>
        public MultiplayerGameplayLeaderboard(ScoreProcessor scoreProcessor, MultiplayerRoomUser[] users)
        {
            // todo: this will eventually need to be created per user to support different mod combinations.
            this.scoreProcessor = scoreProcessor;

            playingUsers = users;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);

            foreach (var user in playingUsers)
            {
                var trackedUser = CreateUserData(user, scoreProcessor);
                trackedUser.ScoringMode.BindTo(scoringMode);
                UserScores[user.UserID] = trackedUser;

                if (trackedUser.Team is int team && !TeamScores.ContainsKey(team))
                    TeamScores.Add(team, new BindableInt());
            }

            userLookupCache.GetUsersAsync(playingUsers.Select(u => u.UserID).ToArray()).ContinueWith(users => Schedule(() =>
            {
                foreach (var user in users.Result)
                {
                    if (user == null)
                        continue;

                    var trackedUser = UserScores[user.Id];

                    var leaderboardScore = Add(user, user.Id == api.LocalUser.Value.Id);
                    leaderboardScore.Accuracy.BindTo(trackedUser.Accuracy);
                    leaderboardScore.TotalScore.BindTo(trackedUser.Score);
                    leaderboardScore.Combo.BindTo(trackedUser.CurrentCombo);
                    leaderboardScore.HasQuit.BindTo(trackedUser.UserQuit);
                }
            }));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // BindableList handles binding in a really bad way (Clear then AddRange) so we need to do this manually..
            foreach (var user in playingUsers)
            {
                spectatorClient.WatchUser(user.UserID);

                if (!multiplayerClient.CurrentMatchPlayingUserIds.Contains(user.UserID))
                    usersChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { user.UserID }));
            }

            // bind here is to support players leaving the match.
            // new players are not supported.
            playingUserIds.BindTo(multiplayerClient.CurrentMatchPlayingUserIds);
            playingUserIds.BindCollectionChanged(usersChanged);

            // this leaderboard should be guaranteed to be completely loaded before the gameplay starts (is a prerequisite in MultiplayerPlayer).
            spectatorClient.OnNewFrames += handleIncomingFrames;
        }

        protected virtual TrackedUserData CreateUserData(MultiplayerRoomUser user, ScoreProcessor scoreProcessor) => new TrackedUserData(user, scoreProcessor);

        protected override GameplayLeaderboardScore CreateLeaderboardScoreDrawable(APIUser user, bool isTracked)
        {
            var leaderboardScore = base.CreateLeaderboardScoreDrawable(user, isTracked);

            if (UserScores[user.Id].Team is int team)
            {
                leaderboardScore.BackgroundColour = getTeamColour(team).Lighten(1.2f);
                leaderboardScore.TextColour = Color4.White;
            }

            return leaderboardScore;
        }

        private Color4 getTeamColour(int team)
        {
            switch (team)
            {
                case 0:
                    return colours.TeamColourRed;

                default:
                    return colours.TeamColourBlue;
            }
        }

        private void usersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (int userId in e.OldItems.OfType<int>())
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

            updateTotals();
        });

        private void updateTotals()
        {
            if (!hasTeams)
                return;

            foreach (var scores in TeamScores.Values) scores.Value = 0;

            foreach (var u in UserScores.Values)
            {
                if (u.Team == null)
                    continue;

                if (TeamScores.TryGetValue(u.Team.Value, out var team))
                    team.Value += (int)Math.Round(u.Score.Value);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient != null)
            {
                foreach (var user in playingUsers)
                {
                    spectatorClient.StopWatchingUser(user.UserID);
                }

                spectatorClient.OnNewFrames -= handleIncomingFrames;
            }
        }

        protected class TrackedUserData
        {
            public readonly MultiplayerRoomUser User;
            public readonly ScoreProcessor ScoreProcessor;

            public readonly BindableDouble Score = new BindableDouble();
            public readonly BindableDouble Accuracy = new BindableDouble(1);
            public readonly BindableInt CurrentCombo = new BindableInt();
            public readonly BindableBool UserQuit = new BindableBool();

            public readonly IBindable<ScoringMode> ScoringMode = new Bindable<ScoringMode>();

            public readonly List<TimedFrame> Frames = new List<TimedFrame>();

            public int? Team => (User.MatchState as TeamVersusUserState)?.TeamID;

            public TrackedUserData(MultiplayerRoomUser user, ScoreProcessor scoreProcessor)
            {
                User = user;
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
