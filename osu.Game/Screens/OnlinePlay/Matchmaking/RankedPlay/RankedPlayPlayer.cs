// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayPlayer : MultiplayerPlayer
    {
        private readonly MultiplayerRoomUser localUser;
        private readonly MultiplayerRoomUser opponentUser;

        public RankedPlayPlayer(Room room, PlaylistItem playlistItem, MultiplayerRoomUser localUser, MultiplayerRoomUser opponentUser)
            : base(room, playlistItem, [localUser, opponentUser])
        {
            this.localUser = localUser;
            this.opponentUser = opponentUser;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LeaderboardProvider.Scores.CollectionChanged += scoresUpdated;
        }

        private void scoresUpdated(object? sender, NotifyCollectionChangedEventArgs args)
        {
            // `MultiplayerLeaderboardProvider.Scores` is being populated asynchronously since it needs
            // to load user details from the API. Because of that, the bindables containing total scores
            // are not immediately available after loading the player.
            // We wait until the list has two members (which is always the case in ranked play)
            // until binding the score display.
            if (LeaderboardProvider.Scores.Count != 2)
                return;

            bindScoreDisplay();
            LeaderboardProvider.Scores.CollectionChanged -= scoresUpdated;
        }

        private void bindScoreDisplay()
        {
            ScoreDisplay.Alpha = 1;

            // Team 1 in `MatchScoreDisplay` is red, so we've got to bind those in a counterintuitive order.
            // TODO: implement a custom component for this
            ScoreDisplay.Team1Score.BindTarget = LeaderboardProvider.Scores.Single(s => s.User.OnlineID == opponentUser.UserID).TotalScore;
            ScoreDisplay.Team2Score.BindTarget = LeaderboardProvider.Scores.Single(s => s.User.OnlineID == localUser.UserID).TotalScore;
        }
    }
}
