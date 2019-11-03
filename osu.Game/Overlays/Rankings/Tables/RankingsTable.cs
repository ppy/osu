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

namespace osu.Game.Overlays.Rankings.Tables
{
    public abstract class RankingsTable<TModel> : TableContainer
    {
        protected const int TEXT_SIZE = 14;
        private const float horizontal_inset = 20;
        private const float row_height = 25;
        private const int items_per_page = 50;

        private readonly int page;
        private readonly FillFlowContainer backgroundFlow;

        public IReadOnlyList<TModel> Rankings
        {
            set
            {
                Content = null;
                backgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                value.ForEach(_ => backgroundFlow.Add(new TableRowBackground()));

                Columns = CreateHeaders();
                Content = value.Select((s, i) => CreateContent(page * items_per_page - (items_per_page - i), s)).ToArray().ToRectangular();
            }
        }

        protected RankingsTable(int page)
        {
            this.page = page;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, row_height);

            AddInternal(backgroundFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Margin = new MarginPadding { Top = row_height }
            });
        }

        protected abstract TableColumn[] CreateHeaders();

        protected abstract Drawable[] CreateContent(int index, TModel item);

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? string.Empty, HighlightedColumn());

        protected virtual string HighlightedColumn() => @"Performance";

        private class HeaderText : OsuSpriteText
        {
            private readonly string highlighted;

            public HeaderText(string text, string highlighted)
            {
                this.highlighted = highlighted;

                Text = text;
                Font = OsuFont.GetFont(size: 12);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                if (Text != highlighted)
                    Colour = colours.GreySeafoamLighter;
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
            private void load(OsuColour colours)
            {
                Colour = colours.GreySeafoamLighter;
            }
        }
    }
}
