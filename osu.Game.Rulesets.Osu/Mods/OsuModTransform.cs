// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTransform : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Transform";
        public override string Acronym => "TR";
        public override IconUsage Icon => FontAwesome.Solid.ArrowsAlt;
        public override ModType Type => ModType.Fun;
        public override string Description => "Everything rotates. EVERYTHING.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModWiggle) };

        private float theta;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                var hitObject = (OsuHitObject)drawable.HitObject;

                float appearDistance = (float)(hitObject.TimePreempt - hitObject.TimeFadeIn) / 2;

                Vector2 originalPosition = drawable.Position;
                Vector2 appearOffset = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * appearDistance;

                //the - 1 and + 1 prevents the hit objects to appear in the wrong position.
                double appearTime = hitObject.StartTime - hitObject.TimePreempt - 1;
                double moveDuration = hitObject.TimePreempt + 1;

                using (drawable.BeginAbsoluteSequence(appearTime, true))
                {
                    drawable
                        .MoveToOffset(appearOffset)
                        .MoveTo(originalPosition, moveDuration, Easing.InOutSine);
                }

                theta += (float)hitObject.TimeFadeIn / 1000;
            }
        }
    }
}
