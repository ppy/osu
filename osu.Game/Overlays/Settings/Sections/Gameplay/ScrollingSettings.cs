// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class ScrollingSettings : SettingsSubsection
    {
        protected override string Header => "Scrolling";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new SettingsEnumDropdown<ScrollingAlgorithmType>
                {
                    LabelText = "Scrolling algorithm",
                    Bindable = config.GetBindable<ScrollingAlgorithmType>(OsuSetting.ScrollingAlgorithm),
                }
            };
        }
    }
}
