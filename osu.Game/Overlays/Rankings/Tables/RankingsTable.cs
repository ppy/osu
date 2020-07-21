// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Rankings.Tables
{
    public abstract class RankingsTable<TModel> : TableContainer
    {
        protected const int TEXT_SIZE = 12;
        private const float horizontal_inset = 20;
        private const float row_height = 25;
        private const int items_per_page = 50;

        private readonly int page;
        private readonly IReadOnlyList<TModel> rankings;

        protected RankingsTable(int page, IReadOnlyList<TModel> rankings)
        {
            this.page = page;
            this.rankings = rankings;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, row_height);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer backgroundFlow;

            AddInternal(backgroundFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Margin = new MarginPadding { Top = row_height }
            });

            rankings.ForEach(_ => backgroundFlow.Add(new TableRowBackground()));

            Columns = mainHeaders.Concat(CreateAdditionalHeaders()).ToArray();
            Content = rankings.Select((s, i) => createContent((page - 1) * items_per_page + i, s)).ToArray().ToRectangular();
        }

        private Drawable[] createContent(int index, TModel item) => new Drawable[] { createIndexDrawable(index), createMainContent(item) }.Concat(CreateAdditionalContent(item)).ToArray();

        private static TableColumn[] mainHeaders => new[]
        {
            new TableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.Absolute, 40)), // place
            new TableColumn(string.Empty, Anchor.CentreLeft, new Dimension(GridSizeMode.Distributed)), // flag and username (country name)
        };

        protected abstract TableColumn[] CreateAdditionalHeaders();

        protected abstract Drawable[] CreateAdditionalContent(TModel item);

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? string.Empty, HighlightedColumn());

        protected abstract Country GetCountry(TModel item);

        protected abstract Drawable CreateFlagContent(TModel item);

        private OsuSpriteText createIndexDrawable(int index) => new OsuSpriteText
        {
            Text = $"#{index + 1}",
            Font = OsuFont.GetFont(size: TEXT_SIZE, weight: FontWeight.SemiBold)
        };

        private FillFlowContainer createMainContent(TModel item) => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(7, 0),
            Children = new[]
            {
                new UpdateableFlag(GetCountry(item))
                {
                    Size = new Vector2(20, 13),
                    ShowPlaceholderOnNull = false,
                },
                CreateFlagContent(item)
            }
        };

        protected virtual string HighlightedColumn() => @"Performance";

        private class HeaderText : OsuSpriteText
        {
            private readonly string highlighted;

            public HeaderText(string text, string highlighted)
            {
                this.highlighted = highlighted;

                Text = text;
                Font = OsuFont.GetFont(size: 12);
                Margin = new MarginPadding { Horizontal = 10 };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                if (Text != highlighted)
                    Colour = colourProvider.Foreground1;
            }
        }

        protected class RowText : OsuSpriteText
        {
            public RowText()
            {
                Font = OsuFont.GetFont(size: TEXT_SIZE);
                Margin = new MarginPadding { Horizontal = 10 };
            }
        }

        protected class ColoredRowText : RowText
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
            }
        }
    }
}
