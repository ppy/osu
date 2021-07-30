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
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Rankings.Tables
{
    public abstract class UserBasedTable : RankingsTable<UserStatistics>
    {
        protected UserBasedTable(int page, IReadOnlyList<UserStatistics> rankings)
            : base(page, rankings)
        {
        }

        protected virtual IEnumerable<string> GradeColumns => new List<string> { "SS", "S", "A" };

        protected override RankingsTableColumn[] CreateAdditionalHeaders() => new[]
            {
                new RankingsTableColumn("Accuracy", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new RankingsTableColumn("Play Count", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
            }.Concat(CreateUniqueHeaders())
             .Concat(GradeColumns.Select(grade => new GradeTableColumn(grade, Anchor.Centre, new Dimension(GridSizeMode.AutoSize))))
             .ToArray();

        protected sealed override Country GetCountry(UserStatistics item) => item.User.Country;

        protected sealed override Drawable CreateFlagContent(UserStatistics item)
        {
            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: TEXT_SIZE, italics: true))
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                TextAnchor = Anchor.CentreLeft
            };
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

        protected abstract RankingsTableColumn[] CreateUniqueHeaders();

        protected abstract Drawable[] CreateUniqueContent(UserStatistics item);

        private class GradeTableColumn : RankingsTableColumn
        {
            public GradeTableColumn(LocalisableString? header = null, Anchor anchor = Anchor.TopLeft, Dimension dimension = null, bool highlighted = false)
                : base(header, anchor, dimension, highlighted)
            {
            }

            public override HeaderText CreateHeaderText() => new GradeHeaderText(Header, Highlighted);
        }

        private class GradeHeaderText : HeaderText
        {
            public GradeHeaderText(LocalisableString text, bool isHighlighted)
                : base(text, isHighlighted)
            {
                Margin = new MarginPadding
                {
                    // Grade columns have extra horizontal padding for readibility
                    Horizontal = 20,
                    Vertical = 5
                };
            }
        }
    }
}
