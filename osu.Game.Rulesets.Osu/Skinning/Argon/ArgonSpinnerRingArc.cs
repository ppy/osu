// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSpinnerRingArc : CompositeDrawable
    {
        private const float arc_fill = 0.31f;
        private const float arc_fill_complete = 0.50f;

        private const float arc_radius = 0.02f;

        private DrawableSpinner spinner = null!;
        private CircularProgress fill = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;

            spinner = (DrawableSpinner)drawableHitObject;
            InternalChild = fill = new CircularProgress
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Progress = arc_fill,
                Rotation = -arc_fill * 180,
                InnerRadius = arc_radius,
                RoundedCaps = true,
            };
        }

        protected override void Update()
        {
            base.Update();

            fill.Progress = (float)Interpolation.DampContinuously(fill.Progress, spinner.Progress >= 1 ? arc_fill_complete : arc_fill, 40f, (float)Math.Abs(Time.Elapsed));
            fill.InnerRadius = (float)Interpolation.DampContinuously(fill.InnerRadius, spinner.Progress >= 1 ? arc_radius * 2.2f : arc_radius, 40f, (float)Math.Abs(Time.Elapsed));

            fill.Rotation = (float)(-fill.Progress * 180);
        }
    }
}
