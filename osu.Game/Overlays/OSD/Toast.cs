// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Overlays.OSD
{
    public abstract class Toast : Container
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected Toast()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Width = 240;

            // A toast's height is decided (and transformed) by the containing OnScreenDisplay.
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.7f
                },
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }
    }
}
