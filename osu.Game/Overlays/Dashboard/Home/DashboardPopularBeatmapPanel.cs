// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Overlays.Dashboard.Home
{
    public class DashboardPopularBeatmapPanel : DashboardBeatmapPanel
    {
        public DashboardPopularBeatmapPanel(APIBeatmapSet beatmapSet)
            : base(beatmapSet)
        {
        }

        protected override Drawable CreateInfo() => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(3, 0),
            Colour = ColourProvider.Foreground1,
            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Size = new Vector2(10),
                    Icon = FontAwesome.Solid.Heart
                },
                new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 10, weight: FontWeight.Regular),
                    Text = BeatmapSet.FavouriteCount.ToString()
                }
            }
        };
    }
}
