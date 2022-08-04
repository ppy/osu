// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Methods of which <see cref="TaikoPlayfieldAdjustmentContainer"/> adjusts the aspect ratio visually. This does not
    /// affect time range directly.
    /// </summary>
    public enum AspectRatioAdjustmentMethod
    {
        /// <summary>
        /// Do not adjust the aspect ratio of the playfield visually.
        /// </summary>
        None,
        /// <summary>
        /// Adjust the playfield's size proportionally keeping the aspect ratio, This is the default lazer behaviour.
        /// </summary>
        Scale,
        /// <summary>
        /// Trim the playfield to fit the aspect ratio.
        /// </summary>
        Trim,
    }

    public class TaikoPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float default_relative_height = TaikoPlayfield.DEFAULT_HEIGHT / 768;
        public const float default_aspect = 16f / 9f;

        public Bindable<float> AspectRatioLimit = new Bindable<float>(default_aspect);
        public Bindable<AspectRatioAdjustmentMethod> AdjustmentMethod = new Bindable<AspectRatioAdjustmentMethod>(AspectRatioAdjustmentMethod.Scale);

        protected override void Update()
        {
            base.Update();

            float height = default_relative_height;
            float parentAspectRatio = Parent.ChildSize.X / Parent.ChildSize.Y;
            switch (AdjustmentMethod.Value)
            {
                case AspectRatioAdjustmentMethod.Scale:
                    height *= Math.Clamp(parentAspectRatio, 0.4f, 4) / AspectRatioLimit.Value;
                    break;
                case AspectRatioAdjustmentMethod.Trim:
                    Width = AspectRatioLimit.Value / parentAspectRatio;
                    break;
            }

            Height = height;

            // Position the taiko playfield exactly one playfield from the top of the screen.
            RelativePositionAxes = Axes.Y;
            Y = height;
        }
    }
}
