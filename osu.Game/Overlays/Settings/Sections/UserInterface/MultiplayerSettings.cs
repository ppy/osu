// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public partial class MultiplayerSettings : SettingsSubsection
    {
        protected override LocalisableString Header => UserInterfaceStrings.MultiplayerHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            // This entire subsection is hidden on mobile platforms.
            // If any settings are added which aren't available on all platforms, this must be undone in `UserInterfaceSection`.
            if (RuntimeInfo.IsDesktop)
            {
                Add(new SettingsItemV2(new FormCheckBox
                {
                    Caption = UserInterfaceStrings.RequestFocusOnMultiplayerGameplayStart,
                    Current = config.GetBindable<bool>(OsuSetting.RequestFocusOnMultiplayerGameplayStart),
                })
                {
                    Keywords = new[] { "multiplayer", "match", "request", "focus", "window" },
                    ShowRevertToDefaultButton = true,
                });
            }
        }
    }
}
