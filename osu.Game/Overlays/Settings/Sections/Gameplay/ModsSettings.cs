// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class ModsSettings : SettingsSubsection
    {
        protected override string Header => "Mods";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Increase visibility of first object with \"Hidden\" mod",
                    Bindable = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility)
                },
            };
        }
    }
}
