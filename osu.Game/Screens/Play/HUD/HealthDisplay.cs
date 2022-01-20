// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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
        private readonly Bindable<bool> showHealthBar = new Bindable<bool>(true);

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

        [Resolved(canBeNull: true)]
        private HUDOverlay hudOverlay { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindTo(HealthProcessor.Health);
            HealthProcessor.NewJudgement += onNewJudgement;

            if (hudOverlay != null)
                showHealthBar.BindTo(hudOverlay.ShowHealthBar);

            // this probably shouldn't be operating on `this.`
            showHealthBar.BindValueChanged(healthBar => this.FadeTo(healthBar.NewValue ? 1 : 0, HUDOverlay.FADE_DURATION, HUDOverlay.FADE_EASING), true);
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
