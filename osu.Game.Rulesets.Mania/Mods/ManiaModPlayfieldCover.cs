// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;
using System.Collections.Generic;
using osu.Framework.Timing;

namespace osu.Game.Rulesets.Mania.Mods
{
    public abstract class ManiaModPlayfieldCover : ModHidden, IApplicableToDrawableRuleset<ManiaHitObject>
    {
        private const float combo_to_reach_max_coverage = 480;

        private float currentCoverage;
        private float coverageRange;

        private List<PlayfieldCoveringWrapper> coveringWrappers = null!;
        private IFrameBasedClock clock = null!;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };

        /// <summary>
        /// The direction in which the cover should expand.
        /// </summary>
        protected abstract CoverExpandDirection ExpandDirection { get; }

        [SettingSource("Coverage", "The proportion of playfield height that notes will be hidden for.")]
        public abstract BindableNumber<float> Coverage { get; }

        [SettingSource("Change coverage based on combo", "Increase the coverage as combo increases.")]
        public BindableBool ComboBasedCoverage => new BindableBool(true);

        private readonly BindableInt combo = new BindableInt();

        public override void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            combo.BindTo(scoreProcessor.Combo);

            base.ApplyToScoreProcessor(scoreProcessor);
        }

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            ManiaPlayfield maniaPlayfield = (ManiaPlayfield)drawableRuleset.Playfield;
            coveringWrappers = new List<PlayfieldCoveringWrapper>();
            clock = maniaPlayfield.Clock;
            currentCoverage = Coverage.Value;
            coverageRange = Coverage.MaxValue - Coverage.Value;

            if (ComboBasedCoverage.Value)
            {
                maniaPlayfield.OnUpdate += _ => updateCoverage();
            }

            foreach (Column column in maniaPlayfield.Stages.SelectMany(stage => stage.Columns))
            {
                HitObjectContainer hoc = column.HitObjectArea.HitObjectContainer;
                Container hocParent = (Container)hoc.Parent;

                hocParent.Remove(hoc, false);

                PlayfieldCoveringWrapper coveringWrapper = new PlayfieldCoveringWrapper(hoc).With(c =>
                {
                    c.RelativeSizeAxes = Axes.Both;
                    c.Direction = ExpandDirection;
                    c.Coverage = Coverage.Value;
                });

                hocParent.Add(coveringWrapper);
                coveringWrappers.Add(coveringWrapper);
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        private void updateCoverage()
        {
            float targetCoverage = getComboBasedCoverageAmount();

            if (targetCoverage != currentCoverage)
            {
                currentCoverage = targetCoverage;
                foreach (PlayfieldCoveringWrapper wrapper in coveringWrappers)
                    wrapper.Coverage = currentCoverage;
            }
        }

        private float getComboBasedCoverageAmount()
        {
            float targetCoverage = Coverage.Value + coverageRange * Math.Min(1.0f, combo.Value / combo_to_reach_max_coverage);
            if (currentCoverage > targetCoverage)
                return currentCoverage - coverageRange * (float)clock.ElapsedFrameTime / 500;
            else
                return targetCoverage;
        }
    }
}
