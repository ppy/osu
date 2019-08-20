// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinnerTick : DrawableOsuHitObject
    {
        private readonly BindableDouble bonusSampleVolume = new BindableDouble();

        private bool hasBonusPoints;

        /// <summary>
        /// Whether this judgement has a bonus of 1,000 points additional to the numeric result.
        /// Should be set when a spin occured after the spinner has completed.
        /// </summary>
        public bool HasBonusPoints
        {
            get => hasBonusPoints;
            internal set
            {
                hasBonusPoints = value;

                bonusSampleVolume.Value = value ? 1 : 0;
                ((OsuSpinnerTickJudgement)Result.Judgement).HasBonusPoints = value;
            }
        }

        public override bool DisplayResult => false;

        public DrawableSpinnerTick(SpinnerTick spinnerTick)
            : base(spinnerTick)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Samples.AddAdjustment(AdjustableProperty.Volume, bonusSampleVolume);
        }

        public void TriggerResult(HitResult result) => ApplyResult(r => r.Type = result);
    }
}
