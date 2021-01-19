// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class IntegrationSettings : SettingsSubsection
    {
        protected override string Header => "settings.integration.header";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<DiscordRichPresenceMode>
                {
                    LabelText = "settings.integration.discordRPC",
                    Current = config.GetBindable<DiscordRichPresenceMode>(OsuSetting.DiscordRichPresence)
                }
            };
        }
    }
}
