// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
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
        public override string TargetMember => "Alpha, GameplayClockContainer.UserPlaybackRate, AudioManager.Track.Frequency";

        private BindableDouble clockRate = new BindableDouble();
        private BindableDouble trackFreq = new BindableDouble();

        private SampleChannel sampleFail;

        private int seed;

        public FailTransform(GameplayClockContainer clockContainer, AudioManager audioManager)
        {
            clockRate.BindTo(clockContainer.UserPlaybackRate);
            trackFreq.BindTo(audioManager.Track.Frequency);

            // Save the rates to be used if completed or aborted
            clockRate.Default = clockRate.Value;
            trackFreq.Default = trackFreq.Value;

            sampleFail = audioManager.Sample.Get(@"Gameplay/failsound");
            sampleFail?.Play();

            base.OnComplete = onCompletion;
            base.OnAbort = onAbort;
            seed = RNG.Next();
        }

        private void onCompletion()
        {
            sampleFail?.Stop();

            clockRate.SetDefault();
            trackFreq.SetDefault();
            OnComplete?.Invoke();
        }

        private void onAbort()
        {
            sampleFail?.Stop();

            clockRate.SetDefault();
            trackFreq.SetDefault();
            OnAbort?.Invoke();
        }

        private float valueAt(double time)
            => Interpolation.ValueAt(time, StartValue, EndValue, StartTime, EndTime, Easing);

        protected override void Apply(HitObjectContainer d, double time)
        {
            var currentValue = valueAt(time);

            clockRate.Value = currentValue;
            trackFreq.Value = currentValue;
            d.Alpha = currentValue;

            // TODO: Use RNG with 'seed' if it was implemented.
            Random randObj = new Random(seed);
            foreach (DrawableHitObject obj in d.Objects)
            {
                obj.Rotation += RNG.NextSingle(0.2f);
                obj.Y += RNG.NextSingle(0.4f, 0.8f);
                obj.X += (randObj.NextDouble() > 0.5) ? RNG.NextSingle(0.0f, 0.4f) : RNG.NextSingle(-0.4f, -0.0f);
            }
        }

        protected override void ReadIntoStartValue(HitObjectContainer d) => StartValue = (float)(clockRate.Default + trackFreq.Default) / 2;
    }
}
