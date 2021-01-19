// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum DiscordRichPresenceMode
    {
        [Description("settings.integration.discordRPC.Off")]
        Off,

        [Description("settings.integration.discordRPC.Limited")]
        Limited,

        [Description("settings.integration.discordRPC.Full")]
        Full
    }
}
