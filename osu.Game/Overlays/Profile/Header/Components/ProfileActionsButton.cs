// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public abstract partial class ProfileActionsButton : OsuHoverContainer, IHasPopover
    {
        private Box background = null!;

        protected override IEnumerable<Drawable> EffectTargets => [background];

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background1;

            Size = new Vector2(40);
            Masking = true;
            CornerRadius = 20;

            Child = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteIcon
                    {
                        Size = new Vector2(12),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.EllipsisV,
                    },
                }
            };

            Action = this.ShowPopover;
        }

        public abstract Popover GetPopover();
    }
}
