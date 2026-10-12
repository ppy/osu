// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class AlertsAndPrivacySettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.AlertsAndPrivacyHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.NotifyOnMentioned,
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnUsernameMentioned)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.NotifyOnPrivateMessage,
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnPrivateMessage)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.NotifyOnFriendPresenceChange,
                    HintText = OnlineSettingsStrings.NotifyOnFriendPresenceChangeTooltip,
                    Current = config.GetBindable<bool>(OsuSetting.NotifyOnFriendPresenceChange),
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = AccountsStrings.PrivacyFriendsOnly.ToSentence(),
                    HintText = AccountsStrings.PrivacyFriendsOnlyInfo.ToSentence(),
                    Current = config.GetBindable<bool>(OsuSetting.PMFriendsOnly),
                }),
            };
        }
    }
}
