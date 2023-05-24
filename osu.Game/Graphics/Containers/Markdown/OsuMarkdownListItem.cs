// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.Containers.Markdown
{
    public abstract partial class OsuMarkdownListItem : CompositeDrawable
    {
        [Resolved]
        private IMarkdownTextComponent parentTextComponent { get; set; }

        public FillFlowContainer Content { get; private set; }

        protected OsuMarkdownListItem()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                CreateMarker(),
                Content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10, 10),
                }
            };
        }

        protected virtual SpriteText CreateMarker() => parentTextComponent.CreateSpriteText();
    }
}
