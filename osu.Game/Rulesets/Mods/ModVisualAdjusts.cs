// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModVisualAdjusts<TObject, TDrawableRuleset> : Mod, IApplicableToDrawableRuleset<TObject>, IApplicableToDrawableHitObject
        where TObject : HitObject where TDrawableRuleset : DrawableRuleset<TObject>
    {
        public override string Name => "Visual Adjusts";
        public override LocalisableString Description => "Adjust some gameplay elements that can bring some visual challenge.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "VA";
        public override ModType Type => ModType.Conversion;

        private void triggerAdjustsForType<TArgs>(TArgs args)
        {
            foreach (var (_, property) in this.GetOrderedSettingsSourceProperties())
            {
                if (property.GetValue(this) is VisualAdjustSetting<TArgs> bindable && bindable.Value)
                    bindable.ApplyAdjusts(args);
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            triggerAdjustsForType((TDrawableRuleset)drawableRuleset);
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            triggerAdjustsForType(drawable);
        }

        public abstract class VisualAdjustSetting<TArgs> : BindableBool
        {
            public readonly Action<TArgs> ApplyAdjusts;

            protected VisualAdjustSetting(Action<TArgs> applyAdjusts)
            {
                ApplyAdjusts = applyAdjusts;
            }
        }

        public class DrawableRulesetVisualAdjustSetting : VisualAdjustSetting<TDrawableRuleset>
        {
            public DrawableRulesetVisualAdjustSetting(Action<TDrawableRuleset> applyAdjusts)
                : base(applyAdjusts)
            {
            }
        }

        public class DrawableHitObjectVisualAdjustSetting : VisualAdjustSetting<DrawableHitObject>
        {
            public DrawableHitObjectVisualAdjustSetting(Action<DrawableHitObject> applyAdjusts)
                : base(applyAdjusts)
            {
            }
        }
    }
}
