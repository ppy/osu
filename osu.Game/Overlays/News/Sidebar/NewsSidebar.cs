// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.Shapes;
using osuTK;
using System.Linq;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.News.Sidebar
{
    public class NewsSidebar : CompositeDrawable
    {
        [Cached]
        public readonly Bindable<APINewsSidebar> Metadata = new Bindable<APINewsSidebar>();

        private FillFlowContainer<MonthSection> monthsFlow;

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
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = OsuScrollContainer.SCROLL_BAR_HEIGHT,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = colourProvider.Background3,
                    Alpha = 0.5f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Right = -3 }, // Compensate for scrollbar margin
                    Child = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = 3 }, // Addeded 3px back
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding
                                {
                                    Vertical = 20,
                                    Left = 50,
                                    Right = 30
                                },
                                Child = new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(0, 20),
                                    Children = new Drawable[]
                                    {
                                        new YearsPanel(),
                                        monthsFlow = new FillFlowContainer<MonthSection>
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(0, 10)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Metadata.BindValueChanged(onMetadataChanged, true);
        }

        private void onMetadataChanged(ValueChangedEvent<APINewsSidebar> metadata)
        {
            monthsFlow.Clear();

            if (metadata.NewValue == null)
                return;

            var allPosts = metadata.NewValue.NewsPosts;

            if (allPosts?.Any() != true)
                return;

            var lookup = metadata.NewValue.NewsPosts.ToLookup(post => post.PublishedAt.Month);

            var keys = lookup.Select(kvp => kvp.Key);
            var sortedKeys = keys.OrderByDescending(k => k).ToList();

            var year = metadata.NewValue.CurrentYear;

            for (int i = 0; i < sortedKeys.Count; i++)
            {
                var month = sortedKeys[i];
                var posts = lookup[month];

                monthsFlow.Add(new MonthSection(month, year, posts)
                {
                    Expanded = { Value = i == 0 }
                });
            }
        }
    }
}
