// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyBarLine : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            float skinHeight = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.BarLineHeight)?.Value ?? 1;

            RelativeSizeAxes = Axes.X;
            Height = 1.2f * skinHeight;
            Colour = skin.GetManiaSkinConfig<Color4>(LegacyManiaSkinConfigurationLookups.BarLineColour)?.Value ?? Color4.White;

            // Avoid flickering due to no anti-aliasing of boxes by default.
            var edgeSmoothness = new Vector2(0.3f);

            AddInternal(new Box
            {
                Name = "Bar line",
                EdgeSmoothness = edgeSmoothness,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
            });
        }
    }
}
