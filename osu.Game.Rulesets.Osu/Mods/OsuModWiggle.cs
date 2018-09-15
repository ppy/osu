// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
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
        public override Type[] IncompatibleMods => new[] { typeof(OsuModTransform) };

        private const int wiggle_duration = 90; // (ms) Higher = fewer wiggles
        private const int wiggle_strength = 10; // Higher = stronger wiggles

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState;
        }

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            var osuObject = (OsuHitObject)drawable.HitObject;
            Vector2 origin = drawable.Position;

            Random objRand = new Random((int)osuObject.StartTime);

            // Wiggle all objects during TimePreempt
            int amountWiggles = (int)osuObject.TimePreempt / wiggle_duration;

            void wiggle()
            {
                float nextAngle = (float)(objRand.NextDouble() * 2 * Math.PI);
                float nextDist = (float)(objRand.NextDouble() * wiggle_strength);
                drawable.MoveTo(new Vector2((float)(nextDist * Math.Cos(nextAngle) + origin.X), (float)(nextDist * Math.Sin(nextAngle) + origin.Y)), wiggle_duration);
            }

            for (int i = 0; i < amountWiggles; i++)
                using (drawable.BeginAbsoluteSequence(osuObject.StartTime - osuObject.TimePreempt + i * wiggle_duration, true))
                    wiggle();

            // Keep wiggling sliders and spinners for their duration
            if (!(osuObject is IHasEndTime endTime))
                return;

            amountWiggles = (int)(endTime.Duration / wiggle_duration);

            for (int i = 0; i < amountWiggles; i++)
                using (drawable.BeginAbsoluteSequence(osuObject.StartTime + i * wiggle_duration, true))
                    wiggle();
        }
    }
}
