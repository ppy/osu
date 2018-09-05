// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "osu!mania";

        public ManiaSettingsSubsection(ManiaRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<ManiaScrollingDirection>
                {
                    LabelText = "Scrolling direction",
                    Bindable = ((ManiaConfigManager)Config).GetBindable<ManiaScrollingDirection>(ManiaSetting.ScrollDirection)
                }
            };
        }
    }
}
