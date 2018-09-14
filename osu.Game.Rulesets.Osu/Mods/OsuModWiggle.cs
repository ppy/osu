// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModWiggle : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Wiggle";
        public override string ShortenedName => "WG";
        public override FontAwesome Icon => FontAwesome.fa_certificate;
        public override ModType Type => ModType.Fun;
        public override string Description => "They just won't stay still...";
        public override double ScoreMultiplier => 1;

        private const int wiggle_delay = 90; // (ms) Higher = fewer wiggles
        private const int wiggle_strength = 10; // Higher = stronger wiggles

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState;
        }

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            var hitObject = (OsuHitObject)drawable.HitObject;
            Vector2 origPos = drawable.Position;

            Random distRand = new Random(hitObject.ComboOffset);
            Random angleRand = new Random(hitObject.IndexInCurrentCombo);

            // Wiggle all objects during TimePreempt
            int amountWiggles = (int)hitObject.TimePreempt / wiggle_delay;

            for (int i = 0; i < amountWiggles; i++)
            {
                using (drawable.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimePreempt + i * wiggle_delay, true))
                {
                    float nextAngle = (float)(angleRand.NextDouble() * 2 * Math.PI);
                    float nextDist = (float)(distRand.NextDouble() * wiggle_strength);
                    Vector2 wiggledPos = new Vector2((float)(nextDist * Math.Cos(nextAngle) + origPos.X), (float)(nextDist * Math.Sin(nextAngle) + origPos.Y));
                    drawable.MoveTo(wiggledPos, wiggle_delay);
                }
            }

            // Keep wiggling sliders and spinners for their duration
            double objDuration;
            if (hitObject is Slider slider)
            {
                objDuration = slider.Duration;
            }
            else if (hitObject is Spinner spinner)
            {
                objDuration = spinner.Duration;
            }
            else
                return;

            amountWiggles = (int)(objDuration / wiggle_delay);

            for (int i = 0; i < amountWiggles; i++)
            {
                using (drawable.BeginAbsoluteSequence(hitObject.StartTime + i * wiggle_delay, true))
                {
                    float nextAngle = (float)(angleRand.NextDouble() * 2 * Math.PI);
                    float nextDist = (float)(distRand.NextDouble() * wiggle_strength);
                    Vector2 wiggledPos = new Vector2((float)(nextDist * Math.Cos(nextAngle) + origPos.X), (float)(nextDist * Math.Sin(nextAngle) + origPos.Y));
                    drawable.MoveTo(wiggledPos, wiggle_delay);
                }
            }
        }
    }
}
