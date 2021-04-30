// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownListItem : FillFlowContainer
    {
        private const float default_left_padding = 20;

        public OsuMarkdownListItem()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Direction = FillDirection.Vertical;
            Spacing = new Vector2(10, 10);
            Padding = new MarginPadding { Left = default_left_padding };
        }
    }
}
