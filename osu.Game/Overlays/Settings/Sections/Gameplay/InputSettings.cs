// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class InputSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.InputHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = SkinSettingsStrings.GameplayCursorSize,
                    Current = config.GetBindable<float>(OsuSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f,
                    LabelFormat = v => $"{v:0.##}x"
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinSettingsStrings.AutoCursorSize,
                    Current = config.GetBindable<bool>(OsuSetting.AutoCursorSize)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = SkinSettingsStrings.GameplayCursorDuringTouch,
                    Current = config.GetBindable<bool>(OsuSetting.GameplayCursorDuringTouch)
                })
                {
                    Keywords = new[] { @"touchscreen" },
                },
            };

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
            {
                Add(new SettingsItemV2(new FormCheckBox
                {
                    Caption = GameplaySettingsStrings.DisableWinKey,
                    Current = config.GetBindable<bool>(OsuSetting.GameplayDisableWinKey)
                }));
            }
        }
    }
}
