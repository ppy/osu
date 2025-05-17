// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class MultiScoreDiff : RollingCounter<long>, ISerialisableDrawable
    {
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved]
        private Player player { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private IGameplayLeaderboardProvider leaderboardProvider { get; set; } = null!;

        protected override double RollingDuration => 0;

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        protected override void Update()
        {
            base.Update();

            // If the user is not in a multiplayer room
            if (multiplayerClient.LocalUser == null)
            {
                return;
            }

            Logger.Log($"user name : {multiplayerClient!.LocalUser!.User!.Username}");

            Logger.Log($"user count : {leaderboardProvider.Scores.Count}");

            // If the leaderborad is yet fully loaded
            if (leaderboardProvider.Scores.Count == 0)
            {
                return;
            }

            var scoresButMe = leaderboardProvider.Scores.ToList().Where(s => s.User.Username != multiplayerClient.LocalUser.User.Username).ToList();

            Logger.Log($"scoresButMe count : {scoresButMe.Count}");

            // If the user is the only one in the room
            if (scoresButMe.Count == 0)
            {
                return;
            }

            var curTopScore = scoresButMe.ToList().OrderByDescending(s => s.TotalScore.Value).FirstOrDefault();

            Current.Value = player.Score.ScoreInfo.TotalScore - curTopScore!.TotalScore.Value;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
