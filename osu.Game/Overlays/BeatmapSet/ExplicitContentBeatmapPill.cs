// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapSet
{
    public class ExplicitContentBeatmapPill : CompositeDrawable
    {
        public ExplicitContentBeatmapPill()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new BeatmapSetBadgePillContainer
            {
                Child = new OsuSpriteText
                {
                    Text = BeatmapsetsStrings.NsfwBadgeLabel.ToUpper(),
                    Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold),
                    Colour = OverlayColourProvider.Orange.Colour2,
                }
            };
        }
    }
}
