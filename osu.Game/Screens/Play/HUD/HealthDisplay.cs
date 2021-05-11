// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A container for components displaying the current player health.
    /// Gets bound automatically to the <see cref="Rulesets.Scoring.HealthProcessor"/> when inserted to <see cref="DrawableRuleset.Overlays"/> hierarchy.
    /// </summary>
    public abstract class HealthDisplay : CompositeDrawable
    {
        [Resolved]
        protected HealthProcessor HealthProcessor { get; private set; }

        public Bindable<double> Current { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1
        };

        protected virtual void Flash(JudgementResult result)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Current.BindTo(HealthProcessor.Health);

            HealthProcessor.NewJudgement += onNewJudgement;
        }

        private void onNewJudgement(JudgementResult judgement)
        {
            if (judgement.IsHit && judgement.Type != HitResult.IgnoreHit)
                Flash(judgement);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (HealthProcessor != null)
                HealthProcessor.NewJudgement -= onNewJudgement;
        }
    }
}
