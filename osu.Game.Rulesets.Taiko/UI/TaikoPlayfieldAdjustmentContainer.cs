// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        public const float MAXIMUM_ASPECT = 16f / 9f;
        public const float MINIMUM_ASPECT = 5f / 4f;

        public readonly IBindable<bool> LockPlayfieldAspectRange = new BindableBool(true);

        public TaikoPlayfieldAdjustmentContainer()
        {
            RelativeSizeAxes = Axes.X;
            RelativePositionAxes = Axes.Y;
            Height = TaikoPlayfield.BASE_HEIGHT;
        }

        protected override void Update()
        {
            base.Update();

            const float base_relative_height = TaikoPlayfield.BASE_HEIGHT / 768;

            float relativeHeight = base_relative_height;

            // Players coming from stable expect to be able to change the aspect ratio regardless of the window size.
            // We originally wanted to limit this more, but there was considerable pushback from the community.
            //
            // As a middle-ground, the aspect ratio can still be adjusted in the downwards direction but has a maximum limit.
            // This is still a bit weird, because readability changes with window size, but it is what it is.
            if (LockPlayfieldAspectRange.Value)
            {
                float currentAspect = Parent!.ChildSize.X / Parent!.ChildSize.Y;

                if (currentAspect > MAXIMUM_ASPECT)
                    relativeHeight *= currentAspect / MAXIMUM_ASPECT;
                else if (currentAspect < MINIMUM_ASPECT)
                    relativeHeight *= currentAspect / MINIMUM_ASPECT;
            }

            // Limit the maximum relative height of the playfield to one-third of available area to avoid it masking out on extreme resolutions.
            relativeHeight = Math.Min(relativeHeight, 1f / 3f);

            // Position the taiko playfield exactly one playfield from the top of the screen, if there is enough space for it.
            // Note that the relative height cannot exceed one-third - if that limit is hit, the playfield will be exactly centered.
            Y = relativeHeight;

            Scale = new Vector2(Math.Max((Parent!.ChildSize.Y / 768f) * (relativeHeight / base_relative_height), 1f));
            Width = 1 / Scale.X;
        }
    }
}
