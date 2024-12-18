// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Home.News
{
    public partial class NewsItemGroupPanel : HomePanel
    {
        private readonly List<APINewsPost> posts;

        public NewsItemGroupPanel(List<APINewsPost> posts)
        {
            this.posts = posts;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Padding = new MarginPadding { Vertical = 5 };

            Child = new FillFlowContainer<NewsGroupItem>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = posts.Select(p => new NewsGroupItem(p)).ToArray()
            };
        }
    }
}
