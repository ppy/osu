// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;

namespace osu.Game.Overlays.Rankings.Tables
{
    public class ScoresTable : UserBasedTable
    {
        public ScoresTable(int page, IReadOnlyList<UserStatistics> rankings)
            : base(page, rankings)
        {
        }

        protected override TableColumn[] CreateUniqueHeaders() => new[]
        {
            new TableColumn("Total Score", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            new TableColumn("Ranked Score", Anchor.Centre, new Dimension(GridSizeMode.AutoSize))
        };

        protected override Drawable[] CreateUniqueContent(UserStatistics item) => new Drawable[]
        {
            new ColoredRowText
            {
                Text = $@"{item.TotalScore:N0}",
            },
            new RowText
            {
                Text = $@"{item.RankedScore:N0}",
            }
        };

        protected override string HighlightedColumn() => @"Ranked Score";
    }
}
