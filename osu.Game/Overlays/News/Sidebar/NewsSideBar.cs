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

namespace osu.Game.Overlays.News.Sidebar
{
    public class NewsSideBar : CompositeDrawable
    {
        [Cached]
        public readonly Bindable<APINewsSidebar> Metadata = new Bindable<APINewsSidebar>();

        private FillFlowContainer<MonthDropdown> monthsFlow;

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
                            monthsFlow = new FillFlowContainer<MonthDropdown>
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
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

            Metadata.BindValueChanged(onMetadataChanged, true);
        }

        private void onMetadataChanged(ValueChangedEvent<APINewsSidebar> metadata)
        {
            monthsFlow.Clear();

            if (metadata.NewValue == null)
                return;

            var allPosts = metadata.NewValue.NewsPosts;

            if (!allPosts?.Any() ?? false)
                return;

            var lookup = metadata.NewValue.NewsPosts.ToLookup(post => post.PublishedAt.Month);

            var keys = lookup.Select(kvp => kvp.Key);
            var sortedKeys = keys.OrderByDescending(k => k).ToList();

            for (int i = 0; i < sortedKeys.Count; i++)
            {
                var posts = lookup[sortedKeys[i]];

                monthsFlow.Add(new MonthDropdown(posts)
                {
                    IsOpen = { Value = i == 0 }
                });
            }
        }
    }
}
