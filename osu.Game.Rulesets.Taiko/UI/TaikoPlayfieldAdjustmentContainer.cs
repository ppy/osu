// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class TaikoPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        private const float stable_gamefield_height = 480f;

        public BindableFloat MaximumAspect = new BindableFloat(16f / 9f);

        public BindableFloat MinimumAspect = new BindableFloat(5f / 4f);

        public BindableFloat CurrentAspect = new BindableFloat(16f / 9f);

        public BindableBool TrimOnOverflow = new BindableBool(false);

        public TaikoPlayfieldAdjustmentContainer()
        {
            RelativeSizeAxes = Axes.X;
            RelativePositionAxes = Axes.Y;
            Height = TaikoPlayfield.BASE_HEIGHT;

            // Matches stable, see https://github.com/peppy/osu-stable-reference/blob/7519cafd1823f1879c0d9c991ba0e5c7fd3bfa02/osu!/GameModes/Play/Rulesets/Taiko/RulesetTaiko.cs#L514
            Y = 135f / stable_gamefield_height;
        }

        protected override void Update()
        {
            base.Update();

            const float base_relative_height = TaikoPlayfield.BASE_HEIGHT / 768;

            float relativeHeight = base_relative_height;

            float widthScale = 1.0f;

            // Players coming from stable expect to be able to change the aspect ratio regardless of the window size.
            // We originally wanted to limit this more, but there was considerable pushback from the community.
            //
            // As a middle-ground, the aspect ratio can still be adjusted in the downwards direction but has a maximum limit.
            // This is still a bit weird, because readability changes with window size, but it is what it is.
            CurrentAspect.Value = Parent!.ChildSize.X / Parent!.ChildSize.Y;

            if (CurrentAspect.Value > MaximumAspect.Value)
            {
                if (TrimOnOverflow.Value)
                {
                    widthScale = MaximumAspect.Value / CurrentAspect.Value;
                }
                else
                {
                    relativeHeight *= CurrentAspect.Value / MaximumAspect.Value;
                }
            }
            else if (CurrentAspect.Value < MinimumAspect.Value)
            {
                relativeHeight *= CurrentAspect.Value / MinimumAspect.Value;
            }


            // Limit the maximum relative height of the playfield to one-third of available area to avoid it masking out on extreme resolutions.
            relativeHeight = Math.Min(relativeHeight, 1f / 3f);

            Scale = new Vector2(Math.Max((Parent!.ChildSize.Y / 768f) * (relativeHeight / base_relative_height), 1f));
            Width = 1 / Scale.X * widthScale;
        }

        public double ComputeTimeRange()
        {
            float currentAspect = Parent!.ChildSize.X / Parent!.ChildSize.Y;

            currentAspect = Math.Clamp(currentAspect, MinimumAspect.Value, MaximumAspect.Value);

            // in a game resolution of 1024x768, stable's scrolling system consists of objects being placed 600px (widthScaled - 40) away from their hit location.
            // however, the point at which the object renders at the end of the screen is exactly x=640, but stable makes the object start moving from beyond the screen instead of the boundary point.
            // therefore, in lazer we have to adjust the "in length" so that it's in a 640px->160px fashion before passing it down as a "time range".
            // see stable's "in length": https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManagerTaiko.cs#L168
            const float stable_hit_location = 160f;
            float widthScaled = currentAspect * stable_gamefield_height;
            float inLength = widthScaled - stable_hit_location;

            // also in a game resolution of 1024x768, stable makes hit objects scroll from 760px->160px at a duration of 6000ms, divided by slider velocity (i.e. at a rate of 0.1px/ms)
            // compare: https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManagerTaiko.cs#L218
            // note: the variable "sv", in the linked reference, is equivalent to MultiplierControlPoint.Multiplier * 100, but since time range is agnostic of velocity, we replace "sv" with 100 below.
            float inMsLength = inLength / 100 * 1000;

            // stable multiplies the slider velocity by 1.4x for certain reasons, divide the time range by that factor to achieve similar result.
            // for references on how the factor is applied to the time range, see:
            //  1. https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManagerTaiko.cs#L79 (DifficultySliderMultiplier multiplied by 1.4x)
            //  2. https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManager.cs#L468-L470 (DifficultySliderMultiplier used to calculate SliderScoringPointDistance)
            //  3. https://github.com/peppy/osu-stable-reference/blob/013c3010a9d495e3471a9c59518de17006f9ad89/osu!/GameplayElements/HitObjectManager.cs#L248-L250 (SliderScoringPointDistance used to calculate slider velocity, i.e. the "sv" variable from above)
            inMsLength /= TaikoBeatmapConverter.VELOCITY_MULTIPLIER;

            return inMsLength;
        }
    }
}
