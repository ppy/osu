// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public abstract class BeatmapCardIconButton : OsuHoverContainer
    {
        protected override IEnumerable<Drawable> EffectTargets => background.Yield();

        private readonly Box background;
        protected readonly SpriteIcon Icon;

        private float iconSize;

        public float IconSize
        {
            get => iconSize;
            set
            {
                iconSize = value;
                Icon.Size = new Vector2(iconSize);
            }
        }

        protected BeatmapCardIconButton()
        {
            Child = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    Icon = new SpriteIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre
                    }
                }
            };

            Size = new Vector2(24);
            IconSize = 12;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Anchor = Origin = Anchor.Centre;

            IdleColour = colourProvider.Background4;
            HoverColour = colourProvider.Background1;
            Icon.Colour = colourProvider.Content2;
        }
    }
}
