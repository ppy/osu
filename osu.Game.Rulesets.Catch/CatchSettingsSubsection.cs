// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Catch.Configuration;

namespace osu.Game.Rulesets.Catch
{
    public partial class CatchSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!catch";

        public CatchSettingsSubsection(CatchRuleset ruleset) : base(ruleset)
        {
        }
        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (CatchRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show cursor during gameplay",
                    Current = config.GetBindable<bool>(CatchRulesetSetting.ShowCursorDuringPlay),
                }
            };
        }
    }
}
