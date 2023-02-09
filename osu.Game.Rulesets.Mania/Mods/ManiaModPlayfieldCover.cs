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
using osu.Game.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Mods
{
    public abstract class ManiaModPlayfieldCover : ModHidden, IApplicableToDrawableRuleset<ManiaHitObject>
    {
        private const float COMBO_TO_REACH_MAX_COVERAGE = 400;

        private PlayfieldCoveringWrapper CoveringWrapper = null!;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };

        /// <summary>
        /// The direction in which the cover should expand.
        /// </summary>
        protected abstract CoverExpandDirection ExpandDirection { get; }

        [SettingSource("Coverage", "The proportion of playfield height that notes will be hidden for.")]
        public abstract BindableNumber<float> Coverage { get; }

        [SettingSource("Change coverage based on combo", "Increase the coverage as combo increases.")]
        public BindableBool ComboBasedCoverage => new BindableBool(true);

        protected readonly BindableInt Combo = new BindableInt();

        public override void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            Combo.BindTo(scoreProcessor.Combo);

            // Default value of ScoreProcessor's Rank in Flashlight Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;
        }

        public virtual void ApplyToDrawableRuleset(DrawableRuleset<ManiaHitObject> drawableRuleset)
        {
            ManiaPlayfield maniaPlayfield = (ManiaPlayfield)drawableRuleset.Playfield;

            foreach (Column column in maniaPlayfield.Stages.SelectMany(stage => stage.Columns))
            {
                HitObjectContainer hoc = column.HitObjectArea.HitObjectContainer;
                Container hocParent = (Container)hoc.Parent;
                if (ComboBasedCoverage.Value)
                    Combo.ValueChanged += _ => UpdateCoverage();

                hocParent.Remove(hoc, false);

                CoveringWrapper = new PlayfieldCoveringWrapper(hoc).With(c =>
                {
                    c.RelativeSizeAxes = Axes.Both;
                    c.Direction = ExpandDirection;
                    c.Coverage = Coverage.Value;
                });
                hocParent.Add(CoveringWrapper);
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        private void UpdateCoverage()
        {
            CoveringWrapper.Coverage = GetComboBasedCoverage();
        }

        private float GetComboBasedCoverage()
        {
            return Coverage.Value + (Coverage.MaxValue - Coverage.Value) * Math.Min(1.0f, Combo.Value / COMBO_TO_REACH_MAX_COVERAGE);
        }
    }
}
