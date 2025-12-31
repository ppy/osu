// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Mods.Input;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class SongSelectSettings : SettingsSubsection
    {
        private FormEnumDropdown<ModSelectHotkeyStyle> modSelectHotkeyStyle = null!;
        private FormCheckBox modSelectTextSearchStartsActive = null!;

        protected override LocalisableString Header => UserInterfaceStrings.SongSelectHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.ShowConvertedBeatmaps,
                    Current = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps),
                })
                {
                    Keywords = new[] { "converts", "converted" }
                },
                new SettingsItemV2(new FormEnumDropdown<RandomSelectAlgorithm>
                {
                    Caption = UserInterfaceStrings.RandomSelectionAlgorithm,
                    Current = config.GetBindable<RandomSelectAlgorithm>(OsuSetting.RandomSelectAlgorithm),
                }),
                new SettingsItemV2(modSelectHotkeyStyle = new FormEnumDropdown<ModSelectHotkeyStyle>
                {
                    Caption = UserInterfaceStrings.ModSelectHotkeyStyle,
                    Current = config.GetBindable<ModSelectHotkeyStyle>(OsuSetting.ModSelectHotkeyStyle),
                })
                {
                    ApplyClassicDefault = () => modSelectHotkeyStyle.Current.Value = ModSelectHotkeyStyle.Classic,
                },
                new SettingsItemV2(modSelectTextSearchStartsActive = new FormCheckBox
                {
                    Caption = UserInterfaceStrings.ModSelectTextSearchStartsActive,
                    Current = config.GetBindable<bool>(OsuSetting.ModSelectTextSearchStartsActive),
                })
                {
                    ApplyClassicDefault = () => modSelectTextSearchStartsActive.Current.Value = false,
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GameplaySettingsStrings.BackgroundBlur,
                    Current = config.GetBindable<bool>(OsuSetting.SongSelectBackgroundBlur),
                })
                {
                    ApplyClassicDefault = () => backgroundBlurCheckbox.Current.Value = false,
                },
            };
        }
    }
}
