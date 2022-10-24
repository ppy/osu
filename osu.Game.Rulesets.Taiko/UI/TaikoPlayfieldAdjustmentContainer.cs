// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class TaikoPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float default_relative_height = TaikoPlayfield.DEFAULT_HEIGHT / 768;
        private const float default_aspect = 16f / 9f;

        public readonly IBindable<bool> LockPlayfieldAspect = new BindableBool(true);

        protected override void Update()
        {
            base.Update();

            float height = default_relative_height;

            if (LockPlayfieldAspect.Value)
                height *= Math.Clamp(Parent.ChildSize.X / Parent.ChildSize.Y, 0.4f, 4) / default_aspect;

            Height = height;

            // Position the taiko playfield exactly one playfield from the top of the screen.
            RelativePositionAxes = Axes.Y;
            Y = height;
        }
    }
}
