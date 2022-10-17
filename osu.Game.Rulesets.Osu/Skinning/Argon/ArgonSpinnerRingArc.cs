// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public class ArgonSpinnerRingArc : CompositeDrawable
    {
        private const float arc_fill = 0.31f;
        private const float arc_radius = 0.02f;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = new CircularProgress
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Current = { Value = arc_fill },
                Rotation = -arc_fill * 180,
                InnerRadius = arc_radius,
                RoundedCaps = true,
            };
        }
    }
}
