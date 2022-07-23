// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class LanguageSettings : SettingsSubsection
    {
        private SettingsDropdown<Language> languageSelection;
        private Bindable<string> frameworkLocale;

        protected override LocalisableString Header => GeneralSettingsStrings.LanguageHeader;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig, OsuConfigManager config)
        {
            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);

            Children = new Drawable[]
            {
                languageSelection = new SettingsEnumDropdown<Language>
                {
                    LabelText = GeneralSettingsStrings.LanguageDropdown,
                },
                new SettingsCheckbox
                {
                    LabelText = GeneralSettingsStrings.PreferOriginalMetadataLanguage,
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowUnicode)
                },
                new SettingsCheckbox
                {
                    LabelText = GeneralSettingsStrings.Prefer24HourTimeDisplay,
                    Current = config.GetBindable<bool>(OsuSetting.Prefer24HourTime)
                },
            };

            if (!LanguageExtensions.TryParseCultureCode(frameworkLocale.Value, out var locale))
                locale = Language.en;
            languageSelection.Current.Value = locale;

            languageSelection.Current.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());
        }
    }
}
