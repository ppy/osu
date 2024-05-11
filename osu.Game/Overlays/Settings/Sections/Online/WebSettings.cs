// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class WebSettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.WebHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.ExternalLinkWarning,
                    Current = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.PreferNoVideo,
                    Keywords = new[] { "no-video" },
                    Current = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.AutomaticallyDownloadMissingBeatmaps,
                    Keywords = new[] { "spectator", "replay" },
                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps),
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.ShowExplicitContent,
                    Keywords = new[] { "nsfw", "18+", "offensive" },
                    Current = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent),
                }
            };
        }
    }
}
