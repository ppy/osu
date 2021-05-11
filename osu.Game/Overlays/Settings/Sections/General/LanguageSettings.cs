// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class LanguageSettings : SettingsSubsection
    {
        protected override string Header => "Language";

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Prefer metadata in original language",
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowUnicode)
                },
            };
        }
    }
}
