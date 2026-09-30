// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Framework.Utils;

namespace osu.Game.Audio
{
    public static class SamplePlaybackHelper
    {
        /// <summary>
        /// Plays the provided <see cref="Sample"/> with a randomised pitch.
        /// </summary>
        /// <param name="sample">The <see cref="Sample"/> to be played.</param>
        /// <param name="pitchVariation">The amount of pitch variation to allow.</param>
        /// <returns>The <see cref="SampleChannel"/> that was used for playback.</returns>
        public static SampleChannel? PlayWithRandomPitch(Sample? sample, double pitchVariation = 0.2f)
        {
            var chan = sample?.GetChannel();
            if (chan == null)
                return null;

            chan.Frequency.Value = RNG.NextDouble(1 - pitchVariation, 1 + pitchVariation);
            chan.Play();

            return chan;
        }

        /// <summary>
        /// Plays a random sample from the given <see cref="Sample"/> array, with a randomised pitch.
        /// </summary>
        /// <param name="samples">An array of <see cref="Sample"/> to play.</param>
        /// <param name="pitchVariation">The amount of pitch variation to allow.</param>
        /// <returns>The <see cref="SampleChannel"/> that was used for playback.</returns>
        public static SampleChannel? PlayWithRandomPitch(Sample?[]? samples, double pitchVariation = 0.2f) =>
            PlayWithRandomPitch(samples?[RNG.Next(0, samples.Length)], pitchVariation);
    }
}
