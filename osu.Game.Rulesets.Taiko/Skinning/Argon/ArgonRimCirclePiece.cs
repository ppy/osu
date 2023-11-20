// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonRimCirclePiece : ArgonCirclePiece
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            ArgonAccentColour = ColourInfo.GradientVertical(
                new Color4(0, 161, 241, 255),
                new Color4(0, 111, 167, 255)
            );

            AddInternal(new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.Solid.AngleLeft,
                Size = new Vector2(ICON_SIZE),
                Scale = new Vector2(0.8f, 1)
            });
        }
    }
}
