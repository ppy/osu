// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class MainMenuSettings : SettingsSubsection
    {
        protected override string Header => "User Interface";

        private SettingsEnumDropdown<MainMenuBackgroundMode> backgroundDropdown;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Parallax",
                    Bindable = config.GetBindable<bool>(OsuSetting.MenuParallax)
                },
                backgroundDropdown = new SettingsEnumDropdown<MainMenuBackgroundMode>
                {
                    LabelText = "Main menu background",
                    Bindable = config.GetBindable<MainMenuBackgroundMode>(OsuSetting.MenuBackgroundMode)
                }
            };

            api.LocalUser.BindValueChanged(onUserChanged, true);
        }

        private void onUserChanged(ValueChangedEvent<User> user)
        {
            if ((!user.NewValue?.IsSupporter) ?? true)
            {
                backgroundDropdown.Bindable.Value = MainMenuBackgroundMode.Default;
                backgroundDropdown.Bindable.Disabled = true;
            }
            else
                backgroundDropdown.Bindable.Disabled = false;
        }
    }

    public enum MainMenuBackgroundMode
    {
        Default,
        Skin,
        Beatmap
    }
}
