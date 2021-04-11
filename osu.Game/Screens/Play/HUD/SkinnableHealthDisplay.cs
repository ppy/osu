// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableHealthDisplay : SkinnableDrawable, IHealthDisplay
    {
        public Bindable<double> Current { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1
        };

        public void Flash(JudgementResult result) => skinnedCounter?.Flash(result);

        private HealthProcessor processor;

        public void BindHealthProcessor(HealthProcessor processor)
        {
            if (this.processor != null)
                throw new InvalidOperationException("Can't bind to a processor more than once");

            this.processor = processor;

            Current.BindTo(processor.Health);
        }

        public SkinnableHealthDisplay()
            : base(new HUDSkinComponent(HUDSkinComponents.HealthDisplay), _ => new DefaultHealthDisplay())
        {
            CentreComponent = false;
        }

        private IHealthDisplay skinnedCounter;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            skinnedCounter = Drawable as IHealthDisplay;
            skinnedCounter?.Current.BindTo(Current);
        }
    }
}
