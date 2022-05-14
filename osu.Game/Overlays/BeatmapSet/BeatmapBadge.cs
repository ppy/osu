// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

#nullable enable

namespace osu.Game.Overlays.BeatmapSet
{
    public abstract class BeatmapBadge : CompositeDrawable
    {
        /// <summary>
        /// The text displayed on the badge's label.
        /// </summary>
        public LocalisableString BadgeText
        {
            set => badgeLabel.Text = value.ToUpper();
        }

        /// <summary>
        /// The colour of the badge's label.
        /// </summary>
        public Colour4 BadgeColour
        {
            set => badgeLabel.Colour = value;
        }

        private OsuSpriteText badgeLabel = null!;

        protected BeatmapBadge()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OverlayColourProvider? colourProvider)
        {
            InternalChild = new CircularContainer
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider?.Background5 ?? colours.Gray2,
                    },
                    badgeLabel = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold),
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 2 },
                    }
                }
            };
        }
    }
}
