// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModArrange : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Transform";
        public override string ShortenedName => "TR";
        public override FontAwesome Icon => FontAwesome.fa_arrows;
        public override ModType Type => ModType.Fun;
        public override string Description => "Everything rotates. EVERYTHING.";
        public override double ScoreMultiplier => 1;

        private readonly IReadOnlyList<Type> TargetHitObjectTypes = new List<Type>() {
            typeof(HitCircle),
            typeof(Slider),
            typeof(Spinner),
        };

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            drawables.ForEach(drawable => 
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState
            );
        }

        private float theta;

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            var hitObject = (OsuHitObject) drawable.HitObject;

            if (!TargetHitObjectTypes.Contains(hitObject.GetType()))
                return;

            float appear_distance = (float)hitObject.TimePreempt * 0.5f;

            Vector2 originalPosition = drawable.Position;
            Vector2 appearOffset = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * appear_distance;

            //the - 1 and + 1 prevents the hit explosion to appear in the wrong position.
            double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
            double moveDuration = hitObject.TimePreempt + 1;

            using (drawable.BeginAbsoluteSequence(appearTime, true))
            {
                drawable
                    .MoveToOffset(appearOffset)
                    .MoveTo(originalPosition, moveDuration, Easing.InOutSine);
            }

            theta += 0.4f;
        }
    }
}
