// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using System.Linq;

namespace osu.Game.Overlays.News.Sidebar
{
    public class NewsSidebar : OverlaySidebar
    {
        [Cached]
        public readonly Bindable<APINewsSidebar> Metadata = new Bindable<APINewsSidebar>();

        private FillFlowContainer<MonthSection> monthsFlow;

        protected override Drawable CreateContent() => new FillFlowContainer
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
        };

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

            int year = metadata.NewValue.CurrentYear;

            for (int i = 0; i < sortedKeys.Count; i++)
            {
                int month = sortedKeys[i];
                var posts = lookup[month];

                monthsFlow.Add(new MonthSection(month, year, posts)
                {
                    Expanded = { Value = i == 0 }
                });
            }
        }
    }
}
