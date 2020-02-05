// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Users;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Rankings.Tables
{
    public abstract class UserBasedTable : RankingsTable<UserStatistics>
    {
        protected UserBasedTable(int page, IReadOnlyList<UserStatistics> rankings)
            : base(page, rankings)
        {
        }

        protected override TableColumn[] CreateAdditionalHeaders() => new[]
        {
            new TableColumn("Accuracy", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Play Count", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
        }.Concat(CreateUniqueHeaders()).Concat(new[]
        {
            new TableColumn("SS", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("S", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("A", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
        }).ToArray();

        protected sealed override Country GetCountry(UserStatistics item) => item.User.Country;

        protected sealed override Drawable CreateFlagContent(UserStatistics item)
        {
            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: TEXT_SIZE)) { AutoSizeAxes = Axes.Both };
            username.AddUserLink(item.User);
            return username;
        }

        protected sealed override Drawable[] CreateAdditionalContent(UserStatistics item) => new[]
        {
            new ColoredRowText { Text = item.DisplayAccuracy, },
            new ColoredRowText { Text = $@"{item.PlayCount:N0}", },
        }.Concat(CreateUniqueContent(item)).Concat(new[]
        {
            new ColoredRowText { Text = $@"{item.GradesCount[ScoreRank.XH] + item.GradesCount[ScoreRank.X]:N0}", },
            new ColoredRowText { Text = $@"{item.GradesCount[ScoreRank.SH] + item.GradesCount[ScoreRank.S]:N0}", },
            new ColoredRowText { Text = $@"{item.GradesCount[ScoreRank.A]:N0}", }
        }).ToArray();

        protected abstract TableColumn[] CreateUniqueHeaders();

        protected abstract Drawable[] CreateUniqueContent(UserStatistics item);
    }
}
