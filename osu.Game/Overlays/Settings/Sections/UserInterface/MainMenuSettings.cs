// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public class MainMenuSettings : SettingsSubsection
    {
        protected override string Header => "主界面";

        private IBindable<User> user;

        private SettingsEnumDropdown<BackgroundSource> backgroundSourceDropdown;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, IAPIProvider api)
        {
            user = api.LocalUser.GetBoundCopy();

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "开场语音",
                    Current = config.GetBindable<bool>(OsuSetting.MenuVoice)
                },
                new SettingsCheckbox
                {
                    LabelText = "osu！主题音乐",
                    Current = config.GetBindable<bool>(OsuSetting.MenuMusic)
                },
                new SettingsEnumDropdown<IntroSequence>
                {
                    LabelText = "开场样式",
                    Current = config.GetBindable<IntroSequence>(OsuSetting.IntroSequence),
                },
                backgroundSourceDropdown = new SettingsEnumDropdown<BackgroundSource>
                {
                    LabelText = "背景来源(需要osu!supporter)",
                    Current = config.GetBindable<BackgroundSource>(OsuSetting.MenuBackgroundSource),
                },
                new SettingsEnumDropdown<SeasonalBackgroundMode>
                {
                    LabelText = "季节背景",
                    Current = config.GetBindable<SeasonalBackgroundMode>(OsuSetting.SeasonalBackgroundMode),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            user.BindValueChanged(u =>
            {
                const string not_supporter_note = "该设置需要osu!supporter。";

                backgroundSourceDropdown.WarningText = u.NewValue?.IsSupporter != true ? not_supporter_note : string.Empty;
            }, true);
        }
    }
}
