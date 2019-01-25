// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class MainMenuSettings : SettingsSubsection
    {
        protected override string Header => "User Interface";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Parallax",
                    Bindable = config.GetBindable<bool>(OsuSetting.MenuParallax)
                },
            };
        }
    }
}
