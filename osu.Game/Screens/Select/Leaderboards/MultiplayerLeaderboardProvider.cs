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
using osu.Framework.Graphics.Containers;
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

namespace osu.Game.Screens.Select.Leaderboards
{
    [LongRunningLoad]
    public partial class MultiplayerLeaderboardProvider : CompositeComponent, IGameplayLeaderboardProvider
    {
        public IBindableList<GameplayLeaderboardScore> Scores => scores;
        private readonly BindableList<GameplayLeaderboardScore> scores = new BindableList<GameplayLeaderboardScore>();

        protected readonly Dictionary<int, TrackedUserData> UserScores = new Dictionary<int, TrackedUserData>();
        public readonly SortedDictionary<int, BindableLong> TeamScores = new SortedDictionary<int, BindableLong>();

        public bool HasTeams => TeamScores.Count > 0;

        public bool IsPartial => false;

        private readonly MultiplayerRoomUser[] users;

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();
        private readonly IBindableList<int> playingUserIds = new BindableList<int>();

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public MultiplayerLeaderboardProvider(MultiplayerRoomUser[] users)
        {
            this.users = users;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api, CancellationToken cancellationToken)
        {
            config.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);

            foreach (var user in users)
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

            userLookupCache.GetUsersAsync(users.Select(u => u.UserID).ToArray(), cancellationToken)
                           .ContinueWith(task =>
                           {
                               Schedule(() =>
                               {
                                   var lookedUpUsers = task.GetResultSafely();

                                   for (int i = 0; i < lookedUpUsers.Length; i++)
                                   {
                                       var user = lookedUpUsers[i] ?? new APIUser
                                       {
                                           Id = users[i].UserID,
                                           Username = "Unknown user",
                                       };

                                       var trackedUser = UserScores[user.Id];

                                       var leaderboardScore = new GameplayLeaderboardScore(user, trackedUser.ScoreProcessor, user.Id == api.LocalUser.Value.Id)
                                       {
                                           HasQuit = { BindTarget = trackedUser.UserQuit },
                                           TeamColour = UserScores[user.OnlineID].Team is int team ? getTeamColour(team) : null,
                                       };
                                       scores.Add(leaderboardScore);
                                   }
                               });
                           }, cancellationToken);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // BindableList handles binding in a really bad way (Clear then AddRange) so we need to do this manually..
            foreach (var user in users)
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
            if (!HasTeams)
                return;

            foreach (var teamTotal in TeamScores.Values) teamTotal.Value = 0;

            foreach (var u in UserScores.Values)
            {
                if (u.Team == null)
                    continue;

                if (TeamScores.TryGetValue(u.Team.Value, out var team))
                    team.Value += u.ScoreProcessor.TotalScore.Value;
            }
        }

        private Color4 getTeamColour(int team)
        {
            switch (team)
            {
                case 0:
                    return colours.TeamColourRed.Lighten(1.2f);

                default:
                    return colours.TeamColourBlue.Lighten(1.2f);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient.IsNotNull())
            {
                foreach (var user in users)
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
