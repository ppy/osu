// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Profile.Sections.Recent
{
    public class SupporterIcon : Container
    {
        private SpriteIcon icon;
        private SupporterType type;

        public enum SupporterType
        {
            SupportFirst,
            SupportAgain,
            SupportGift
        }

        public SupporterIcon(SupporterType type)
        {
            this.type = type;
            Child = icon = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            icon.Icon = getIcon(type);
            icon.Colour = colours.Pink;
        }

        private IconUsage getIcon(SupporterType type)
        {
            switch (type)
            {
                case SupporterType.SupportFirst:

                case SupporterType.SupportAgain:
                    return FontAwesome.Solid.Heart;

                case SupporterType.SupportGift:
                    return FontAwesome.Solid.Gift;

                default:
                    return FontAwesome.Solid.Heart;
            }
        }
    }
}
