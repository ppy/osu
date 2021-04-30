// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownListItem : CompositeDrawable
    {
        private const float default_left_padding = 20;

        public FillFlowContainer Content { get; }

        public OsuMarkdownListItem()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Padding = new MarginPadding { Left = default_left_padding };

            InternalChildren = new Drawable[]
            {
                Content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10, 10),
                }
            };
        }
    }
}
