// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osu.Framework.Localisation;

namespace osu.Game.Overlays.Rankings.Tables
{
    public abstract partial class RankingsTable<TModel> : TableContainer
    {
        protected const int TEXT_SIZE = 12;
        private const float row_height = 32;
        private const float row_spacing = 3;
        private const int items_per_page = 50;

        private readonly int page;
        private readonly IReadOnlyList<TModel> rankings;

        protected RankingsTable(int page, IReadOnlyList<TModel> rankings)
        {
            this.page = page;
            this.rankings = rankings;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING };
            RowSize = new Dimension(GridSizeMode.Absolute, row_height + row_spacing);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer backgroundFlow;

            AddInternal(backgroundFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Margin = new MarginPadding { Top = row_height + row_spacing },
                Spacing = new Vector2(0, row_spacing),
            });

            rankings.ForEach(s => backgroundFlow.Add(CreateRowBackground(s)));

            Columns = mainHeaders.Concat(CreateAdditionalHeaders()).Cast<TableColumn>().ToArray();
            Content = rankings.Select((s, i) => CreateRowContent((page - 1) * items_per_page + i, s)).ToArray().ToRectangular();
        }

        protected virtual Drawable CreateRowBackground(TModel item) => new TableRowBackground { Height = row_height };

        protected virtual Drawable[] CreateRowContent(int index, TModel item) => new Drawable[] { createIndexDrawable(index), createMainContent(item) }.Concat(CreateAdditionalContent(item)).ToArray();

        private static RankingsTableColumn[] mainHeaders => new[]
        {
            new RankingsTableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.Absolute, 40)), // place
            new RankingsTableColumn(string.Empty, Anchor.CentreLeft, new Dimension()), // flag and username (country name)
        };

        protected abstract RankingsTableColumn[] CreateAdditionalHeaders();

        protected abstract Drawable[] CreateAdditionalContent(TModel item);

        protected sealed override Drawable CreateHeader(int index, TableColumn column)
            => (column as RankingsTableColumn)?.CreateHeaderText() ?? new HeaderText(column?.Header ?? default, false);

        protected abstract CountryCode GetCountryCode(TModel item);

        protected abstract Drawable CreateFlagContent(TModel item);

        private OsuSpriteText createIndexDrawable(int index) => new RowText
        {
            Text = (index + 1).ToLocalisableString(@"\##"),
            Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.SemiBold)
        };

        private FillFlowContainer createMainContent(TModel item) => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(10, 0),
            Margin = new MarginPadding { Bottom = row_spacing },
            Children = new[]
            {
                new UpdateableFlag(GetCountryCode(item))
                {
                    Size = new Vector2(28, 20),
                },
                CreateFlagContent(item)
            }
        };

        protected class RankingsTableColumn : TableColumn
        {
            protected readonly bool Highlighted;

            public RankingsTableColumn(LocalisableString? header = null, Anchor anchor = Anchor.TopLeft, Dimension dimension = null, bool highlighted = false)
                : base(header, anchor, dimension)
            {
                Highlighted = highlighted;
            }

            public virtual HeaderText CreateHeaderText() => new HeaderText(Header, Highlighted);
        }

        protected partial class HeaderText : OsuSpriteText
        {
            private readonly bool isHighlighted;

            public HeaderText(LocalisableString text, bool isHighlighted)
            {
                this.isHighlighted = isHighlighted;

                Text = text;
                Font = OsuFont.GetFont(size: 12);
                Margin = new MarginPadding { Vertical = 5, Horizontal = 10 };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                if (!isHighlighted)
                    Colour = colourProvider.Foreground1;
            }
        }

        protected partial class RowText : OsuSpriteText
        {
            public RowText()
            {
                Font = OsuFont.GetFont(size: TEXT_SIZE);
                Margin = new MarginPadding { Horizontal = 10, Bottom = row_spacing };
            }
        }

        protected partial class ColouredRowText : RowText
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
            }
        }
    }
}
