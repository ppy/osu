// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.Shapes;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Overlays.News.Sidebar
{
    public class NewsSideBar : CompositeDrawable
    {
        public readonly Bindable<APINewsSidebar> Metadata = new Bindable<APINewsSidebar>();

        private YearsPanel yearsPanel;
        private FillFlowContainer<MonthPanel> monthsFlow;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.Y;
            Width = 250;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Top = 20,
                        Left = 50,
                        Right = 30
                    },
                    Child = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(0, 20),
                        Children = new Drawable[]
                        {
                            yearsPanel = new YearsPanel(),
                            monthsFlow = new FillFlowContainer<MonthPanel>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 10)
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Metadata.BindValueChanged(metadata =>
            {
                monthsFlow.Clear();

                if (metadata.NewValue == null)
                {
                    yearsPanel.Hide();
                    return;
                }

                yearsPanel.Years = metadata.NewValue.Years;
                yearsPanel.Show();

                if (metadata.NewValue != null)
                {
                    var dict = new Dictionary<int, List<APINewsPost>>();

                    foreach (var p in metadata.NewValue.NewsPosts)
                    {
                        var month = p.PublishedAt.Month;

                        if (dict.ContainsKey(month))
                            dict[month].Add(p);
                        else
                        {
                            dict.Add(month, new List<APINewsPost>(new[] { p }));
                        }
                    }

                    bool isFirst = true;

                    foreach (var keyValuePair in dict)
                    {
                        monthsFlow.Add(new MonthPanel(keyValuePair.Value)
                        {
                            IsOpen = { Value = isFirst }
                        });

                        isFirst = false;
                    }
                }
            }, true);
        }
    }
}
