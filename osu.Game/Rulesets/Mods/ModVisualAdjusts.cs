// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModVisualAdjusts<T, R> : Mod, IApplicableToDrawableRuleset<T> where T : HitObject where R : DrawableRuleset<T>
    {
        public override string Name => "Visual Adjusts";
        public override LocalisableString Description => "Override some gameplay elements that can bring some challenge for other mods.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "VA";
        public override ModType Type => ModType.Conversion;

        private void triggerAdjustsForType<B, A>(A args) where B : VisualAdjustSetting<A>
        {
            foreach (var (_, property) in this.GetOrderedSettingsSourceProperties())
            {
                if (property.GetValue(this) is B bindable && bindable.Value)
                    bindable.ApplyAdjusts(args);
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            triggerAdjustsForType<DrawableRulesetVisualAdjustSetting, R>((R)drawableRuleset);
        }

        public abstract class VisualAdjustSetting<A> : BindableBool
        {
            public readonly Action<A> ApplyAdjusts;

            protected VisualAdjustSetting(Action<A> applyAdjusts)
            {
                ApplyAdjusts = applyAdjusts;
            }
        }

        public class DrawableRulesetVisualAdjustSetting : VisualAdjustSetting<R>
        {
            public DrawableRulesetVisualAdjustSetting(Action<R> applyAdjusts)
                : base(applyAdjusts)
            {
            }
        }
    }
}
