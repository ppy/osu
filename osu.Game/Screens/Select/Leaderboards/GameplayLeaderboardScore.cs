// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class GameplayLeaderboardScore : IGameplayLeaderboardScore
    {
        public IUser User { get; }
        public bool Tracked { get; }
        public BindableLong TotalScore { get; } = new BindableLong();
        public BindableDouble Accuracy { get; } = new BindableDouble();
        public BindableInt Combo { get; } = new BindableInt();
        public BindableBool HasQuit { get; } = new BindableBool();
        public Bindable<long> DisplayOrder { get; } = new BindableLong();
        public Func<ScoringMode, long> GetDisplayScore { get; set; }
        public Colour4? TeamColour { get; init; }

        public GameplayLeaderboardScore(IUser user, ScoreProcessor scoreProcessor, bool tracked)
        {
            User = user;
            Tracked = tracked;
            TotalScore.BindTarget = scoreProcessor.TotalScore;
            Accuracy.BindTarget = scoreProcessor.Accuracy;
            Combo.BindTarget = scoreProcessor.Combo;
            GetDisplayScore = scoreProcessor.GetDisplayScore;
        }

        public GameplayLeaderboardScore(IUser user, SpectatorScoreProcessor scoreProcessor, bool tracked)
        {
            User = user;
            Tracked = tracked;
            TotalScore.BindTarget = scoreProcessor.TotalScore;
            Accuracy.BindTarget = scoreProcessor.Accuracy;
            Combo.BindTarget = scoreProcessor.Combo;
            GetDisplayScore = scoreProcessor.GetDisplayScore;
        }

        public GameplayLeaderboardScore(ScoreInfo scoreInfo, bool tracked)
        {
            User = scoreInfo.User;
            Tracked = tracked;
            TotalScore.Value = scoreInfo.TotalScore;
            Accuracy.Value = scoreInfo.Accuracy;
            Combo.Value = scoreInfo.Combo;
            DisplayOrder.Value = scoreInfo.OnlineID > 0 ? scoreInfo.OnlineID : scoreInfo.Date.ToUnixTimeSeconds();
            GetDisplayScore = scoreInfo.GetDisplayScore;
        }
    }
}
