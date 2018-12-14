// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class BeatmapLeaderboardScore : LeaderboardScore<ScoreInfo>
    {
        public BeatmapLeaderboardScore(ScoreInfo score, int rank)
            : base(score, rank)
        {
        }

        protected override User GetUser(ScoreInfo model) => model.User;

        protected override IEnumerable<Mod> GetMods(ScoreInfo model) => model.Mods;

        protected override IEnumerable<(FontAwesome icon, string value, string name)> GetStatistics(ScoreInfo model) => new[]
        {
            (FontAwesome.fa_link, model.MaxCombo.ToString(), "Max Combo"),
            (FontAwesome.fa_crosshairs, string.Format(model.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", model.Accuracy), "Accuracy")
        };

        protected override int GetTotalScore(ScoreInfo model) => model.TotalScore;

        protected override ScoreRank GetRank(ScoreInfo model) => model.Rank;
    }
}
