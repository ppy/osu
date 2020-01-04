// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Extensions.IEnumerableExtensions;
using JetBrains.Annotations;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public abstract class PageSelectorItem : OsuClickableContainer
    {
        protected const int DURATION = 200;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        protected override Container<Drawable> Content => content;

        protected readonly Box Background;
        private readonly CircularContainer content;

        protected PageSelectorItem()
        {
            AutoSizeAxes = Axes.X;
            Height = PageSelector.HEIGHT;
            base.Content.Add(content = new CircularContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                Children = new Drawable[]
                {
                    Background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    CreateContent().With(content =>
                    {
                        content.Anchor = Anchor.Centre;
                        content.Origin = Anchor.Centre;
                        content.Margin = new MarginPadding { Horizontal = 10 };
                    })
                }
            });
        }

        [NotNull]
        protected abstract Drawable CreateContent();

        protected override bool OnHover(HoverEvent e)
        {
            UpdateHoverState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            UpdateHoverState();
        }

        protected abstract void UpdateHoverState();
    }
}
