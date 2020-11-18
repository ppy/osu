// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuSpinnerJudgementResult : OsuJudgementResult
    {
        /// <summary>
        /// The <see cref="Spinner"/>.
        /// </summary>
        public Spinner Spinner => (Spinner)HitObject;

        /// <summary>
        /// The total rotation performed on the spinner disc, disregarding the spin direction,
        /// adjusted for the track's playback rate.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value is always non-negative and is monotonically increasing with time
        /// (i.e. will only increase if time is passing forward, but can decrease during rewind).
        /// </para>
        /// <para>
        /// The rotation from each frame is multiplied by the clock's current playback rate.
        /// The reason this is done is to ensure that spinners give the same score and require the same number of spins
        /// regardless of whether speed-modifying mods are applied.
        /// </para>
        /// </remarks>
        /// <example>
        /// Assuming no speed-modifying mods are active,
        /// if the spinner is spun 360 degrees clockwise and then 360 degrees counter-clockwise,
        /// this property will return the value of 720 (as opposed to 0).
        /// If Double Time is active instead (with a speed multiplier of 1.5x),
        /// in the same scenario the property will return 720 * 1.5 = 1080.
        /// </example>
        public float RateAdjustedRotation;

        /// <summary>
        /// Time instant at which the spinner has been completed (the user has executed all required spins).
        /// Will be null if all required spins haven't been completed.
        /// </summary>
        public double? TimeCompleted;

        public OsuSpinnerJudgementResult(HitObject hitObject, Judgement judgement)
            : base(hitObject, judgement)
        {
        }
    }
}
