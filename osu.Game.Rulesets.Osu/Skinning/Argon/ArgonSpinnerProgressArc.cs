// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonSpinnerProgressArc : CompositeDrawable
    {
        private const float arc_fill = 0.15f;
        private const float arc_radius = 0.12f;

        private CircularProgress fill = null!;

        private DrawableSpinner spinner = null!;

        private CircularProgress background = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableHitObject)
        {
            RelativeSizeAxes = Axes.Both;

            spinner = (DrawableSpinner)drawableHitObject;

            InternalChildren = new Drawable[]
            {
                background = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.White.Opacity(0.25f),
                    RelativeSizeAxes = Axes.Both,
                    Current = { Value = arc_fill },
                    Rotation = 90 - arc_fill * 180,
                    InnerRadius = arc_radius,
                    RoundedCaps = true,
                },
                fill = new CircularProgress
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    InnerRadius = arc_radius,
                    RoundedCaps = true,
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            background.Alpha = spinner.Progress >= 1 ? 0 : 1;

            fill.Alpha = (float)Interpolation.DampContinuously(fill.Alpha, spinner.Progress > 0 && spinner.Progress < 1 ? 1 : 0, 40f, (float)Math.Abs(Time.Elapsed));
            fill.Current.Value = (float)Interpolation.DampContinuously(fill.Current.Value, spinner.Progress >= 1 ? 0 : arc_fill * spinner.Progress, 40f, (float)Math.Abs(Time.Elapsed));

            fill.Rotation = (float)(90 - fill.Current.Value * 180);
        }
    }
}
