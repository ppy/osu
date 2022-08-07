// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Configuration
{
    public enum DiscordRichPresenceMode
    {
        [LocalisableDescription(typeof(OnlineSettingsStrings), nameof(OnlineSettingsStrings.Off))]
        Off,

        [LocalisableDescription(typeof(OnlineSettingsStrings), nameof(OnlineSettingsStrings.HideIdentifiableInformation))]
        Limited,

        [LocalisableDescription(typeof(OnlineSettingsStrings), nameof(OnlineSettingsStrings.Full))]
        Full
    }
}
