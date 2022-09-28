// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModVisualAdjusts<TObject, TDrawableRuleset> : ModWithVisibilityAdjustment, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject where TDrawableRuleset : DrawableRuleset<TObject>
    {
        public override string Name => "Visual Adjusts";
        public override LocalisableString Description => "Adjust some gameplay elements that can bring some visual challenge.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "VA";
        public override ModType Type => ModType.Conversion;

        private readonly IList<BindableBool> activeVisualAdjustSettings = new List<BindableBool>();

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            var tDrawableRuleset = (TDrawableRuleset)drawableRuleset;

            foreach (var (_, property) in this.GetOrderedSettingsSourceProperties())
            {
                if (property.GetValue(this) is BindableBool bindableBool && bindableBool.Value)
                {
                    switch (bindableBool)
                    {
                        case DrawableRulesetVisualAdjustSetting drawableRulesetVisualAdjustSetting:
                            drawableRulesetVisualAdjustSetting.ApplyAdjusts(tDrawableRuleset);
                            break;

                        default:
                            activeVisualAdjustSettings.Add(bindableBool);
                            break;
                    }
                }
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            foreach (var hitObjectVisibilityVisualAdjustSetting in activeVisualAdjustSettings.OfType<HitObjectVisibilityVisualAdjustSetting>())
                hitObjectVisibilityVisualAdjustSetting.ApplyIncreasedVisibilityAdjusts(hitObject);
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
            foreach (var normalVisibilityVisualAdjustSetting in activeVisualAdjustSettings.OfType<HitObjectVisibilityVisualAdjustSetting>())
                normalVisibilityVisualAdjustSetting.ApplyAdjusts(hitObject);
        }

        /// <summary>
        /// Represents an <see cref="BindableBool"/> which is used as a Visual Setting for <see cref="ModVisualAdjusts{TObject,TDrawableRuleset}"/>.
        /// </summary>
        /// <typeparam name="TArgs">The arguments used to call <see cref="ApplyAdjusts"/> and apply the visual adjusts of this setting.</typeparam>
        public abstract class VisualAdjustSetting<TArgs> : BindableBool
        {
            /// <summary>
            /// Applies the visual adjusts for this <see cref="VisualAdjustSetting{TArgs}"/>.
            /// </summary>
            public readonly Action<TArgs> ApplyAdjusts;

            protected VisualAdjustSetting(Action<TArgs> applyAdjusts)
            {
                ApplyAdjusts = applyAdjusts;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents an <see cref="VisualAdjustSetting{TArgs}" /> which is triggered when <see cref="ModVisualAdjusts{TObject,TDrawableRuleset}.ApplyToDrawableRuleset" /> is called.
        /// </summary>
        public class DrawableRulesetVisualAdjustSetting : VisualAdjustSetting<TDrawableRuleset>
        {
            public DrawableRulesetVisualAdjustSetting(Action<TDrawableRuleset> applyAdjusts)
                : base(applyAdjusts)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents an <see cref="VisualAdjustSetting{TArgs}" /> which is triggered when either <see cref="ModVisualAdjusts{TObject,TDrawableRuleset}.ApplyNormalVisibilityState" />
        /// or <see cref="ApplyIncreasedVisibilityAdjusts" /> is called, passing down the given <see cref="DrawableHitObject" /> to apply adjusted visibility into.
        /// </summary>
        public class HitObjectVisibilityVisualAdjustSetting : VisualAdjustSetting<DrawableHitObject>
        {
            /// <summary>
            /// Applies the increased visibility state for the current <see cref="DrawableHitObject"/>.
            /// </summary>
            public readonly Action<DrawableHitObject> ApplyIncreasedVisibilityAdjusts;

            public HitObjectVisibilityVisualAdjustSetting(Action<DrawableHitObject> applyNormalVisibilityAdjusts, Action<DrawableHitObject> applyIncreasedVisibilityAdjusts)
                : base(applyNormalVisibilityAdjusts)
            {
                ApplyIncreasedVisibilityAdjusts = applyIncreasedVisibilityAdjusts;
            }
        }
    }
}
