// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float default_relative_height = TaikoPlayfield.DEFAULT_HEIGHT / 768;
        private const float default_aspect = 16f / 9f;

        public readonly IBindable<bool> LockPlayfieldMaxAspect = new BindableBool(true);

        protected override void Update()
        {
            base.Update();

            float height = default_relative_height;

            // Players coming from stable expect to be able to change the aspect ratio regardless of the window size.
            // We originally wanted to limit this more, but there was considerable pushback from the community.
            //
            // As a middle-ground, the aspect ratio can still be adjusted in the downwards direction but has a maximum limit.
            // This is still a bit weird, because readability changes with window size, but it is what it is.
            if (LockPlayfieldMaxAspect.Value && Parent.ChildSize.X / Parent.ChildSize.Y > default_aspect)
                height *= Math.Clamp(Parent.ChildSize.X / Parent.ChildSize.Y, 0.4f, 4) / default_aspect;

            Height = height;

            // Position the taiko playfield exactly one playfield from the top of the screen.
            RelativePositionAxes = Axes.Y;
            Y = height;
        }
    }
}
