// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSpunOut : Mod, IApplicableToDrawableHitObject
    {
        public override string Name => "Spun Out";
        public override string Acronym => "SO";
        public override IconUsage? Icon => OsuIcon.ModSpunOut;
        public override ModType Type => ModType.Automation;
        public override string Description => @"Spinners will be automatically completed.";
        public override double ScoreMultiplier => 0.9;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(OsuModAutopilot) };

        public void ApplyToDrawableHitObject(DrawableHitObject hitObject)
        {
            if (hitObject is DrawableSpinner spinner)
            {
                spinner.HandleUserInput = false;
                spinner.OnUpdate += onSpinnerUpdate;
            }
        }

        private void onSpinnerUpdate(Drawable drawable)
        {
            var spinner = (DrawableSpinner)drawable;

            spinner.RotationTracker.Tracking = true;

            // early-return if we were paused to avoid division-by-zero in the subsequent calculations.
            if (Precision.AlmostEquals(spinner.Clock.Rate, 0))
                return;

            // because the spinner is under the gameplay clock, it is affected by rate adjustments on the track;
            // for that reason using ElapsedFrameTime directly leads to fewer SPM with Half Time and more SPM with Double Time.
            // for spinners we want the real (wall clock) elapsed time; to achieve that, unapply the clock rate locally here.
            double rateIndependentElapsedTime = spinner.Clock.ElapsedFrameTime / spinner.Clock.Rate;
            spinner.RotationTracker.AddRotation(MathUtils.RadiansToDegrees((float)rateIndependentElapsedTime * 0.03f));
        }
    }
}
