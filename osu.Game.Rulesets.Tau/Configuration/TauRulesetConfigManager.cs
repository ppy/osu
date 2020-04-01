// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Tau.Configuration
{
    public class TauRulesetConfigManager : RulesetConfigManager<TauRulesetSettings>
    {
        public TauRulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(TauRulesetSettings.ShowVisualizer, true);
            Set(TauRulesetSettings.PlayfieldDim, 0.3f, 0, 1, 0.01f);
            Set(TauRulesetSettings.BeatSize, 10f, 5, 25, 1f);
        }
    }

    public enum TauRulesetSettings
    {
        ShowVisualizer,
        PlayfieldDim,
        BeatSize
    }
}
