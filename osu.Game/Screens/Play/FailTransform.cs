// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Screens.Play
{
    public class FailTransform : Transform<float, HitObjectContainer>
    {
        public override string TargetMember => "Alpha";

        private GameplayClockContainer clock;
        private AudioManager audio;

        public FailTransform(GameplayClockContainer clockContainer, AudioManager audioManager)
        {
            this.audio = audioManager;
            this.clock = clockContainer;
        }

        private float valueAt(double time)
            => Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);

        protected override void Apply(HitObjectContainer d, double time)
        {
            var currentValue = valueAt(time);

            clock.UserPlaybackRate.Value = currentValue;
            audio.Track.Frequency.Value = currentValue;
            d.Alpha = currentValue;

            bool posOrNeg = false; // Object falling direction
            foreach (DrawableHitObject obj in d.Objects)
            {
                obj.Rotation += RNG.NextSingle(0.2f);
                obj.Y += RNG.NextSingle(0.4f, 0.8f);
                obj.X += posOrNeg ? RNG.NextSingle(0.0f, 0.2f) : RNG.NextSingle(-0.2f, -0.0f);

                posOrNeg = !posOrNeg;
            }
        }

        protected override void ReadIntoStartValue(HitObjectContainer d) => StartValue = 1;
    }
}
