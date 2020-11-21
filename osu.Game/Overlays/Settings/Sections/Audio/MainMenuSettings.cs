// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class MainMenuSettings : SettingsSubsection
    {
        protected override string Header => "Main Menu";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Interface voices",
                    Current = config.GetBindable<bool>(OsuSetting.MenuVoice)
                },
                new SettingsCheckbox
                {
                    LabelText = "osu! music theme",
                    Current = config.GetBindable<bool>(OsuSetting.MenuMusic)
                },
                new SettingsDropdown<IntroSequence>
                {
                    LabelText = "Intro sequence",
                    Current = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence),
                    Items = Enum.GetValues(typeof(IntroSequence)).Cast<IntroSequence>()
                },
                new SettingsDropdown<BackgroundSource>
                {
                    LabelText = "Background source",
                    Current = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource),
                    Items = Enum.GetValues(typeof(BackgroundSource)).Cast<BackgroundSource>()
                },
                new SettingsDropdown<SeasonalBackgroundMode>
                {
                    LabelText = "Seasonal backgrounds",
                    Current = config.GetBindable<SeasonalBackgroundMode>(OsuSetting.SeasonalBackgroundMode),
                    Items = Enum.GetValues(typeof(SeasonalBackgroundMode)).Cast<SeasonalBackgroundMode>()
                }
            };
        }
    }
}
