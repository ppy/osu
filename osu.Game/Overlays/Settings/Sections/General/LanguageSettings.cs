// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public partial class LanguageSettings : SettingsSubsection
    {
        private SettingsDropdown<Language> languageSelection = null!;
        private Bindable<string> frameworkLocale = null!;
        private IBindable<LocalisationParameters> localisationParameters = null!;

        protected override LocalisableString Header => GeneralSettingsStrings.LanguageHeader;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig, OsuConfigManager config, LocalisationManager localisation)
        {
            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            localisationParameters = localisation.CurrentParameters.GetBoundCopy();

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

            localisationParameters.BindValueChanged(p
                => languageSelection.Current.Value = LanguageExtensions.GetLanguageFor(frameworkLocale.Value, p.NewValue), true);

            languageSelection.Current.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());
        }
    }
}
