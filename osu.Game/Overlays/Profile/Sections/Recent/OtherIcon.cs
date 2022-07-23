// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class OtherIcon : Container
    {
        public OtherIcon(IconUsage inputIcon)
        {
            {
                Child = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = inputIcon,

                };
            }
        }
    }
}
