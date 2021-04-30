// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown
{
    public class OsuMarkdownListItem : CompositeDrawable
    {
        private readonly int level;
        private readonly int order;
        private readonly bool isOrdered;
        private const float default_left_padding = 20;

        [Resolved]
        private IMarkdownTextComponent parentTextComponent { get; set; }

        public FillFlowContainer Content { get; }

        public OsuMarkdownListItem(int level, int order)
        {
            this.level = level;
            this.order = order;
            isOrdered = order != 0;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            var marker = parentTextComponent.CreateSpriteText();
            marker.Text = createTextMarker();
            marker.Font = OsuFont.GetFont(size: marker.Font.Size / 2);
            marker.Origin = Anchor.Centre;
            marker.X = -default_left_padding / 2;
            marker.Y = marker.Font.Size;

            AddInternal(marker);
        }

        private string createTextMarker()
        {
            if (isOrdered)
            {
                return $"{order}.";
            }

            switch (level)
            {
                case 1:
                    return "●";

                case 2:
                    return "○";

                default:
                    return "■";
            }
        }
    }
}
