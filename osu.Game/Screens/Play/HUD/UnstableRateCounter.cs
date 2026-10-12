// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class UnstableRateCounter : RollingCounter<int>
    {
        public bool UsesFixedAnchor { get; set; }

        protected override double RollingDuration => 375;

        private HitEventExtensions.UnstableRateCalculationResult? unstableRateResult;

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        protected UnstableRateCounter()
        {
            Current.Value = 0;
        }

        public Bindable<bool> IsValid { get; } = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += updateDisplay;
            scoreProcessor.JudgementReverted += updateDisplay;
            updateDisplay();
        }

        private void updateDisplay(JudgementResult result)
        {
            if (HitEventExtensions.AffectsUnstableRate(result.HitObject, result.Type))
                Scheduler.AddOnce(updateDisplay);
        }

        private void updateDisplay()
        {
            unstableRateResult = scoreProcessor.HitEvents.CalculateUnstableRate(unstableRateResult);

            double? unstableRate = unstableRateResult?.Result;

            IsValid.Value = unstableRate != null;

            if (unstableRate != null)
                Current.Value = (int)Math.Round(unstableRate.Value);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor.IsNotNull())
            {
                scoreProcessor.NewJudgement -= updateDisplay;
                scoreProcessor.JudgementReverted -= updateDisplay;
            }
        }
    }
}
