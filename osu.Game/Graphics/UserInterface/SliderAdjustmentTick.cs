// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Utils;

namespace osu.Game.Graphics.UserInterface
{
    public partial class SliderAdjustmentTick<T> : Component
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        private Sample sample = null!;

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio)
        {
            sample = audio.Samples.Get(@"UI/notch-tick");
        }

        /// <summary>
        /// the value by which to adjust the slider tick noise based on the location of the nub on the slider, normalized to be from 0 to 1.
        /// </summary>
        /// <param name="normalizedValue"></param>
        public void PlaySample(float normalizedValue)
        {
            var channel = sample.GetChannel();

            channel.Frequency.Value = 0.99f + RNG.NextDouble(0.02f) + normalizedValue * 0.2f;

            // intentionally pitched down, even when hitting max.
            if (normalizedValue == 0 || normalizedValue == 1)
                channel.Frequency.Value -= 0.5f;

            channel.Play();
        }
    }
}
