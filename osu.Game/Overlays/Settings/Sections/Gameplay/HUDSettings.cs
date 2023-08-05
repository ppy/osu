// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class HUDSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.HUDHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<HUDVisibilityMode>
                {
                    LabelText = GameplaySettingsStrings.HUDVisibilityMode,
                    Current = config.GetBindable<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode)
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.ShowReplaySettingsOverlay,
                    Current = config.GetBindable<bool>(OsuSetting.ReplaySettingsOverlay),
                    Keywords = new[] { "hide" },
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.AlwaysShowKeyOverlay,
                    Current = config.GetBindable<bool>(OsuSetting.KeyOverlay),
                    Keywords = new[] { "counter" },
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.AlwaysShowGameplayLeaderboard,
                    Current = config.GetBindable<bool>(OsuSetting.GameplayLeaderboard),
                },
                new SettingsCheckbox
                {
                    ClassicDefault = false,
                    LabelText = GameplaySettingsStrings.ShowHealthDisplayWhenCantFail,
                    Current = config.GetBindable<bool>(OsuSetting.ShowHealthDisplayWhenCantFail),
                    Keywords = new[] { "hp", "bar" }
                },
            };
        }
    }
}
