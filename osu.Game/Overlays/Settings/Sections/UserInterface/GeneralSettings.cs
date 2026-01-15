// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class GeneralSettings : SettingsSubsection
    {
        protected override LocalisableString Header => CommonStrings.General;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.CursorRotation,
                    Current = config.GetBindable<bool>(OsuSetting.CursorRotation)
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = UserInterfaceStrings.MenuCursorSize,
                    Current = config.GetBindable<float>(OsuSetting.MenuCursorSize),
                    KeyboardStep = 0.01f,
                    LabelFormat = v => $"{v:0.##}x"
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.Parallax,
                    Current = config.GetBindable<bool>(OsuSetting.MenuParallax)
                }),
                new SettingsItemV2(new FormSliderBar<double>
                {
                    Caption = UserInterfaceStrings.HoldToConfirmActivationTime,
                    Current = config.GetBindable<double>(OsuSetting.UIHoldActivationDelay),
                    KeyboardStep = 50,
                    LabelFormat = v => $"{v:N0} ms",
                })
                {
                    Keywords = new[] { @"delay" },
                    ApplyClassicDefault = c => ((IHasCurrentValue<double>)c).Current.Value = 0,
                },
            };
        }
    }
}
