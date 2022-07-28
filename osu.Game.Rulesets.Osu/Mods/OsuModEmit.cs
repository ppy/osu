// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModEmit : ModWithVisibilityAdjustment
    {
        public override string Name => "Emit";
        public override string Acronym => "EM";
        public override ModType Type => ModType.Fun;
        public override string Description => "Track them as they arrive";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModTransform), typeof(OsuModMagnetised), typeof(OsuModRepel) };

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => drawableOnApplyCustomUpdateState(hitObject, state);

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => drawableOnApplyCustomUpdateState(hitObject, state);

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            var osuObject = (OsuHitObject)drawable.HitObject;
            var h = (OsuHitObject)drawable.HitObject;
            Vector2 origin = drawable.Position;

            if (osuObject is SliderRepeat || osuObject is SliderTailCircle)
                return;

            void emit()
            {
                drawable.ScaleTo(.8f).Then().ScaleTo(1, h.TimePreempt - h.TimePreempt / 3);
                drawable.MoveTo(OsuPlayfield.BASE_SIZE / 2).Then().MoveTo(origin, (h.TimePreempt - h.TimePreempt / 2), Easing.Out);
            }

            for (int i = 0; i < 1; i++)
            {
                using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    emit();
            }
        }
    }
}
