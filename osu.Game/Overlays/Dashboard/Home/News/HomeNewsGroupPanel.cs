// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public class HomeNewsGroupPanel : HomePanel
    {
        private readonly List<APINewsPost> posts;

        public HomeNewsGroupPanel(List<APINewsPost> posts)
        {
            this.posts = posts;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Padding = new MarginPadding { Vertical = 5 };

            Child = new FillFlowContainer<CollapsedNewsPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = posts.Select(p => new CollapsedNewsPanel(p)).ToArray()
            };
        }

        private class CollapsedNewsPanel : HomeNewsPanelFooter
        {
            public CollapsedNewsPanel(APINewsPost post)
                : base(post)
            {
            }

            protected override Drawable CreateContent(APINewsPost post) => new NewsTitleLink(post);

            protected override Drawable CreateDate(DateTimeOffset date) => new Date(date);
        }

        private class Date : CompositeDrawable, IHasCustomTooltip
        {
            public ITooltip GetCustomTooltip() => new DateTooltip();

            public object TooltipContent => date;

            private readonly DateTimeOffset date;

            public Date(DateTimeOffset date)
            {
                this.date = date;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                TextFlowContainer textFlow;

                AutoSizeAxes = Axes.Both;
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;
                InternalChild = textFlow = new TextFlowContainer(t =>
                {
                    t.Colour = colourProvider.Light1;
                })
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Vertical = 5 }
                };

                textFlow.AddText($"{date:dd}", t =>
                {
                    t.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold);
                });

                textFlow.AddText($"{date: MMM}", t =>
                {
                    t.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Regular);
                });
            }
        }
    }
}
