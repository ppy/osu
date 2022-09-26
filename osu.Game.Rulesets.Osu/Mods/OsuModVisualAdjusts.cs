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
    public class OsuModVisualAdjusts : Mod, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override string Name => "Visual Adjusts";
        public override LocalisableString Description => "Override some gameplay elements that can bring some challenge for other mods.";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "VA";
        public override ModType Type => ModType.Conversion;

        [SettingSource("Disable follow points", "No more hints for where to follow...")]
        public VisualAdjustSetting DisableFollowPoints { get; } = new VisualAdjustSetting(ruleset => ruleset.Playfield.FollowPoints.Hide());

        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            var drawableOsuRuleset = (DrawableOsuRuleset)drawableRuleset;

            foreach (var (_, property) in this.GetOrderedSettingsSourceProperties())
            {
                if (property.GetValue(this) is not VisualAdjustSetting bindable || !bindable.Value) continue;

                bindable.ApplyAdjusts(drawableOsuRuleset);
            }
        }

        public class VisualAdjustSetting : Bindable<bool>
        {
            public readonly Action<DrawableOsuRuleset> ApplyAdjusts;

            public VisualAdjustSetting(Action<DrawableOsuRuleset> applyAdjusts)
            {
                ApplyAdjusts = applyAdjusts;
            }
        }
    }
}
