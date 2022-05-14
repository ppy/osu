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
        [Resolved]
        protected OsuColour Colours { get; private set; } = null!;

        [Resolved(canBeNull: true)]
        protected OverlayColourProvider? ColourProvider { get; private set; }

        /// <summary>
        /// The text displayed on the badge's label.
        /// </summary>
        public abstract LocalisableString BadgeText { get; }

        /// <summary>
        /// The colour of the badge's label.
        /// </summary>
        public abstract Colour4 BadgeColour { get; }

        // todo: add linking support, to allow redirecting featured artist badge to corresponding track and spotlight badge to wiki page.

        protected BeatmapBadge()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(true)]
        private void load()
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
                        Colour = ColourProvider?.Background5 ?? Colours.Gray2,
                    },
                    new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold),
                        Margin = new MarginPadding { Horizontal = 10, Vertical = 2 },
                        Text = BadgeText.ToUpper(),
                        Colour = BadgeColour,
                    }
                }
            };
        }
    }
}
