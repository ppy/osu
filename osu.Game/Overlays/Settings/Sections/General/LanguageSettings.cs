// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class LanguageSettings : SettingsSubsection
    {
        private SettingsDropdown<Language> languageSelection;
        private Bindable<string> frameworkLocale;

        protected override string Header => "语言";

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);

            Children = new Drawable[]
            {
                languageSelection = new SettingsEnumDropdown<Language>
                {
                    LabelText = "界面语言",
                },
                new SettingsCheckbox
                {
                    LabelText = "以原语言显示歌曲信息",
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowUnicode)
                },
            };

            if (!Enum.TryParse<Language>(frameworkLocale.Value, out var locale))
                locale = Language.en;
            languageSelection.Current.Value = locale;

            languageSelection.Current.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToString());
        }
    }
}
