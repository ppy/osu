// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using System;
using osuTK.Graphics;

namespace osu.Game.Overlays.News.Sidebar
{
    public class YearsPanel : CompositeDrawable
    {
        private readonly Bindable<APINewsSidebar> metadata = new Bindable<APINewsSidebar>();

        private Container gridPlaceholder;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, Bindable<APINewsSidebar> metadata)
        {
            this.metadata.BindTo(metadata);

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Masking = true;
            CornerRadius = 6;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                gridPlaceholder = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(5)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            metadata.BindValueChanged(m =>
            {
                if (m.NewValue == null)
                {
                    Hide();
                    return;
                }

                gridPlaceholder.Child = new YearsGridContainer(m.NewValue.Years, m.NewValue.CurrentYear);
                Show();
            }, true);
        }

        private class YearButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { text };

            private readonly OsuSpriteText text;
            private readonly bool isCurrent;
            private readonly int year;

            public YearButton(int year, bool isCurrent)
            {
                this.year = year;
                this.isCurrent = isCurrent;

                RelativeSizeAxes = Axes.X;
                Height = 15;
                Child = text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 12, weight: isCurrent ? FontWeight.SemiBold : FontWeight.Medium),
                    Text = year.ToString()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, NewsOverlay overlay)
            {
                IdleColour = isCurrent ? Color4.White : colourProvider.Light2;
                HoverColour = isCurrent ? Color4.White : colourProvider.Light1;
                Action = () =>
                {
                    if (overlay != null)
                        overlay.ShowYear(year);
                };
            }
        }

        private class YearsGridContainer : GridContainer
        {
            private const int column_count = 4;
            private const float spacing = 5f;

            private readonly int rowCount;

            public YearsGridContainer(int[] years, int currentYear)
            {
                rowCount = (int)Math.Ceiling((float)years.Length / column_count);

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                RowDimensions = getRowDimensions();
                ColumnDimensions = getColumnDimensions();
                Content = createContent(years, currentYear);
            }

            private Dimension[] getRowDimensions()
            {
                var rowDimensions = new Dimension[rowCount];
                for (int i = 0; i < rowCount; i++)
                    rowDimensions[i] = new Dimension(GridSizeMode.AutoSize);

                return rowDimensions;
            }

            private Dimension[] getColumnDimensions()
            {
                var columnDimensions = new Dimension[column_count];
                for (int i = 0; i < column_count; i++)
                    columnDimensions[i] = new Dimension(GridSizeMode.Relative, size: 1f / column_count);

                return columnDimensions;
            }

            private Drawable[][] createContent(int[] years, int currentYear)
            {
                var buttons = new Drawable[rowCount][];

                for (int i = 0; i < rowCount; i++)
                {
                    buttons[i] = new Drawable[column_count];

                    for (int j = 0; j < column_count; j++)
                    {
                        var index = i * column_count + j;

                        if (index >= years.Length)
                        {
                            buttons[i][j] = Empty();
                        }
                        else
                        {
                            var year = years[index];
                            var isCurrent = year == currentYear;

                            buttons[i][j] = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Top = i == 0 ? 0 : spacing / 2,
                                    Bottom = i == rowCount - 1 ? 0 : spacing / 2,
                                    Left = j == 0 ? 0 : spacing / 2,
                                    Right = j == column_count - 1 ? 0 : spacing / 2
                                },
                                Child = new YearButton(year, isCurrent)
                            };
                        }
                    }
                }

                return buttons;
            }
        }
    }
}
