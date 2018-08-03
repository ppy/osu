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
        public override string Name => "Arrange";
        public override string ShortenedName => "Arrange";
        public override FontAwesome Icon => FontAwesome.fa_arrows;
        public override ModType Type => ModType.Fun;
        public override string Description => "Everything rotates. EVERYTHING";
        public override double ScoreMultiplier => 1.05;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            drawables.ForEach(drawable => drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState);
        }

        private float theta = 0;

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            var hitObject = (OsuHitObject) drawable.HitObject;

            // repeat points get their position data from the slider.
            if (hitObject is RepeatPoint)
                return;

            Vector2 originalPosition = drawable.Position;

            // avoiding that the player can see the abroupt move.
            const int pre_time_offset = 1000;

            using (drawable.BeginAbsoluteSequence(hitObject.StartTime - hitObject.TimeFadeIn - pre_time_offset, true))
            {
                drawable
                    .MoveToOffset(new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * 250)
                    .MoveTo(originalPosition, hitObject.TimeFadeIn + pre_time_offset, Easing.InOutSine);
            }

            // That way slider ticks come all from the same direction.
            if (hitObject is HitCircle || hitObject is Slider)
                theta += 0.4f;
        }
    }
}
