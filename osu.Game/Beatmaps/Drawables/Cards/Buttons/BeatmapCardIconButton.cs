// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public abstract class BeatmapCardIconButton : OsuHoverContainer
    {
        protected readonly SpriteIcon Icon;

        private float size;

        public new float Size
        {
            get => size;
            set
            {
                size = value;
                Icon.Size = new Vector2(size);
            }
        }

        protected BeatmapCardIconButton()
        {
            Add(Icon = new SpriteIcon());

            AutoSizeAxes = Axes.Both;
            Size = 12;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Anchor = Origin = Anchor.Centre;

            IdleColour = colourProvider.Light1;
            HoverColour = colourProvider.Content1;
        }
    }
}
