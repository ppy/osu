// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A container for components displaying the current player health.
    /// Gets bound automatically to the <see cref="HealthProcessor"/> when inserted to <see cref="DrawableRuleset.Overlays"/> hierarchy.
    /// </summary>
    public abstract class HealthDisplay : Container, IHealthDisplay
    {
        public Bindable<double> Current { get; } = new BindableDouble(1)
        {
            MinValue = 0,
            MaxValue = 1
        };

        public virtual void Flash(JudgementResult result)
        {
        }

        /// <summary>
        /// Bind the tracked fields of <see cref="HealthProcessor"/> to this health display.
        /// </summary>
        public virtual void BindHealthProcessor(HealthProcessor processor)
        {
            Current.BindTo(processor.Health);
        }
    }
}
