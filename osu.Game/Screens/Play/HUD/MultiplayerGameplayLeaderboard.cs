// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    [LongRunningLoad]
    public partial class MultiplayerGameplayLeaderboard : GameplayLeaderboard
    {
        protected readonly Dictionary<int, TrackedUserData> UserScores = new Dictionary<int, TrackedUserData>();

        public readonly SortedDictionary<int, BindableLong> TeamScores = new SortedDictionary<int, BindableLong>();

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        private Bindable<ScoringMode> scoringMode = null!;

        private readonly MultiplayerRoomUser[] playingUsers;

        private readonly IBindableList<int> playingUserIds = new BindableList<int>();

        private bool hasTeams => TeamScores.Count > 0;

        /// <summary>
        /// Construct a new leaderboard.
        /// </summary>
        /// <param name="users">IDs of all users in this match.</param>
        public MultiplayerGameplayLeaderboard(MultiplayerRoomUser[] users)
        {
            playingUsers = users;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api, CancellationToken cancellationToken)
        {
            scoringMode = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode);

            foreach (var user in playingUsers)
            {
                var scoreProcessor = new SpectatorScoreProcessor(user.UserID);
                scoreProcessor.Mode.BindTo(scoringMode);
                scoreProcessor.TotalScore.BindValueChanged(_ => Scheduler.AddOnce(updateTotals));
                AddInternal(scoreProcessor);

                var trackedUser = new TrackedUserData(user, scoreProcessor);
                UserScores[user.UserID] = trackedUser;

                if (trackedUser.Team is int team && !TeamScores.ContainsKey(team))
                    TeamScores.Add(team, new BindableLong());
            }

            userLookupCache.GetUsersAsync(playingUsers.Select(u => u.UserID).ToArray(), cancellationToken)
                           .ContinueWith(task =>
                           {
                               Schedule(() =>
                               {
                                   var users = task.GetResultSafely();

                                   for (int i = 0; i < users.Length; i++)
                                   {
                                       var user = users[i] ?? new APIUser
                                       {
                                           Id = playingUsers[i].UserID,
                                           Username = "Unknown user",
                                       };

                                       var trackedUser = UserScores[user.Id];

                                       var leaderboardScore = Add(user, user.Id == api.LocalUser.Value.Id);
                                       leaderboardScore.GetDisplayScore = trackedUser.ScoreProcessor.GetDisplayScore;
                                       leaderboardScore.Accuracy.BindTo(trackedUser.ScoreProcessor.Accuracy);
                                       leaderboardScore.TotalScore.BindTo(trackedUser.ScoreProcessor.TotalScore);
                                       leaderboardScore.Combo.BindTo(trackedUser.ScoreProcessor.Combo);
                                       leaderboardScore.HasQuit.BindTo(trackedUser.UserQuit);
                                   }
                               });
                           }, cancellationToken);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // BindableList handles binding in a really bad way (Clear then AddRange) so we need to do this manually..
            foreach (var user in playingUsers)
            {
                spectatorClient.WatchUser(user.UserID);

                if (!multiplayerClient.CurrentMatchPlayingUserIds.Contains(user.UserID))
                    playingUsersChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { user.UserID }));
            }

            // bind here is to support players leaving the match.
            // new players are not supported.
            playingUserIds.BindTo(multiplayerClient.CurrentMatchPlayingUserIds);
            playingUserIds.BindCollectionChanged(playingUsersChanged);
        }

        protected override GameplayLeaderboardScore CreateLeaderboardScoreDrawable(IUser? user, bool isTracked)
        {
            var leaderboardScore = base.CreateLeaderboardScoreDrawable(user, isTracked);

            if (user != null)
            {
                if (UserScores[user.OnlineID].Team is int team)
                {
                    leaderboardScore.BackgroundColour = getTeamColour(team).Lighten(1.2f);
                    leaderboardScore.TextColour = Color4.White;
                }
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

        private void playingUsersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (int userId in e.OldItems.OfType<int>())
                    {
                        spectatorClient.StopWatchingUser(userId);

                        if (UserScores.TryGetValue(userId, out var trackedData))
                            trackedData.MarkUserQuit();
                    }

                    break;
            }
        }

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
                    team.Value += u.ScoreProcessor.TotalScore.Value;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient.IsNotNull())
            {
                foreach (var user in playingUsers)
                    spectatorClient.StopWatchingUser(user.UserID);
            }
        }

        protected class TrackedUserData
        {
            public readonly MultiplayerRoomUser User;
            public readonly SpectatorScoreProcessor ScoreProcessor;

            public readonly BindableBool UserQuit = new BindableBool();

            public int? Team => (User.MatchState as TeamVersusUserState)?.TeamID;

            public TrackedUserData(MultiplayerRoomUser user, SpectatorScoreProcessor scoreProcessor)
            {
                User = user;
                ScoreProcessor = scoreProcessor;
            }

            public void MarkUserQuit() => UserQuit.Value = true;
        }
    }
}
