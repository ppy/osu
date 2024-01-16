// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuSpinnerJudgement : OsuJudgement
    {
        /// <summary>
        /// The <see cref="Spinner"/>.
        /// </summary>
        public Spinner Spinner => (Spinner)HitObject;

        /// <summary>
        /// The total amount that the spinner was rotated.
        /// </summary>
        public float TotalRotation => History.TotalRotation;

        /// <summary>
        /// Stores the spinning history of the spinner.<br />
        /// Instants of movement deltas may be added or removed from this in order to calculate the total rotation for the spinner.
        /// </summary>
        public readonly SpinnerSpinHistory History = new SpinnerSpinHistory();

        /// <summary>
        /// Time instant at which the spin was started (the first user input which caused an increase in spin).
        /// </summary>
        public double? TimeStarted;

        /// <summary>
        /// Time instant at which the spinner has been completed (the user has executed all required spins).
        /// Will be null if all required spins haven't been completed.
        /// </summary>
        public double? TimeCompleted;

        public OsuSpinnerJudgement(HitObject hitObject, JudgementInfo judgementInfo)
            : base(hitObject, judgementInfo)
        {
        }
    }
}
