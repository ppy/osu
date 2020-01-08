// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    /// <summary>
    /// A component that tracks spinner spins and add bonus score for it.
    /// </summary>
    public class SpinnerBonusComponent : CompositeDrawable
    {
        private readonly DrawableSpinner drawableSpinner;
        private readonly Container<DrawableSpinnerTick> ticks;
        private readonly OsuSpriteText bonusCounter;

        public SpinnerBonusComponent(DrawableSpinner drawableSpinner, Container<DrawableSpinnerTick> ticks)
        {
            this.drawableSpinner = drawableSpinner;
            this.ticks = ticks;

            drawableSpinner.OnNewResult += onNewResult;

            AutoSizeAxes = Axes.Both;
            InternalChild = bonusCounter = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Numeric.With(size: 24),
                Alpha = 0,
            };
        }

        private int currentSpins;

        public void UpdateRotation(double rotation)
        {
            if (ticks.Count == 0)
                return;

            int spinsRequired = ((Spinner)drawableSpinner.HitObject).SpinsRequired;

            int newSpins = Math.Clamp((int)(rotation / 360), 0, ticks.Count - 1);
            int direction = Math.Sign(newSpins - currentSpins);

            while (currentSpins != newSpins)
            {
                var tick = ticks[currentSpins];

                if (direction >= 0)
                {
                    tick.HasBonusPoints = currentSpins > spinsRequired;
                    tick.TriggerResult(true);
                }

                if (tick.HasBonusPoints)
                {
                    bonusCounter.Text = $"{1000 * (currentSpins - spinsRequired)}";
                    bonusCounter.FadeOutFromOne(1500);
                    bonusCounter.ScaleTo(1.5f).Then().ScaleTo(1f, 1000, Easing.OutQuint);
                }

                currentSpins += direction;
            }
        }

        private void onNewResult(DrawableHitObject hitObject, JudgementResult result)
        {
            if (!result.HasResult || hitObject != drawableSpinner)
                return;

            // Trigger a miss result for remaining ticks to avoid infinite gameplay.
            foreach (var tick in ticks.Where(t => !t.IsHit))
                tick.TriggerResult(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            drawableSpinner.OnNewResult -= onNewResult;
        }
    }
}