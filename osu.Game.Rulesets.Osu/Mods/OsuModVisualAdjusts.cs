// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModVisualAdjusts : Mod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Visual Adjusts";
        public override LocalisableString Description => "Override some gameplay elements that can bring some challenge for other mods.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "VA";
        public override ModType Type => ModType.Conversion;

        [SettingSource("Disable follow points", "No more hints for where to follow...")]
        public DrawableRulesetVisualAdjustSetting DisableFollowPoints { get; } = new DrawableRulesetVisualAdjustSetting(ruleset => ruleset.Playfield.FollowPoints.Hide());

        private void triggerAdjustsForType<B, T>(T args) where B : VisualAdjustSetting<T>
        {
            foreach (var (_, property) in this.GetOrderedSettingsSourceProperties())
            {
                if (property.GetValue(this) is B bindable && bindable.Value)
                    bindable.ApplyAdjusts(args);
            }
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            triggerAdjustsForType<DrawableRulesetVisualAdjustSetting, DrawableOsuRuleset>((DrawableOsuRuleset)drawableRuleset);
        }

        public abstract class VisualAdjustSetting<T> : Bindable<bool>
        {
            public readonly Action<T> ApplyAdjusts;

            protected VisualAdjustSetting(Action<T> applyAdjusts, bool defaultValue = false)
                : base(defaultValue)
            {
                ApplyAdjusts = applyAdjusts;
            }
        }

        public class DrawableRulesetVisualAdjustSetting : VisualAdjustSetting<DrawableOsuRuleset>
        {
            public DrawableRulesetVisualAdjustSetting(Action<DrawableOsuRuleset> applyAdjusts, bool defaultValue = false)
                : base(applyAdjusts, defaultValue)
            {
            }
        }
    }
}
