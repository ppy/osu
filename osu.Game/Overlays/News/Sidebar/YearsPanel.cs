// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.News.Sidebar
{
    public class YearsPanel : CompositeDrawable
    {
        private readonly Bindable<APINewsSidebar> metadata = new Bindable<APINewsSidebar>();

        private Container gridContent;

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
                gridContent = new Container
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

                gridContent.Child = new YearsGridContainer(m.NewValue.Years, m.NewValue.CurrentYear);
                Show();
            }, true);
        }

        private class YearButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { text };

            private readonly OsuSpriteText text;
            private readonly bool isCurrent;

            public YearButton(int year, bool isCurrent)
            {
                this.isCurrent = isCurrent;

                RelativeSizeAxes = Axes.X;
                Height = 15;
                Padding = new MarginPadding { Vertical = 2.5f };

                Child = text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 12, weight: isCurrent ? FontWeight.SemiBold : FontWeight.Medium),
                    Text = year.ToString()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = isCurrent ? Color4.White : colourProvider.Light2;
                HoverColour = isCurrent ? Color4.White : colourProvider.Light1;
                Action = () => { }; // Avoid button being disabled since there's no proper action assigned.
            }
        }

        private class YearsGridContainer : GridContainer
        {
            private const int column_count = 4;

            private readonly int rowCount;

            public YearsGridContainer(int[] years, int currentYear)
            {
                rowCount = (years.Length + column_count - 1) / column_count;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                RowDimensions = Enumerable.Range(0, rowCount).Select(_ => new Dimension(GridSizeMode.AutoSize)).ToArray();
                Content = createContent(years, currentYear);
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

                            buttons[i][j] = new YearButton(year, isCurrent);
                        }
                    }
                }

                return buttons;
            }
        }
    }
}
