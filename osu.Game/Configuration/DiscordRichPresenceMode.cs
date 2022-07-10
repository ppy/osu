// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum DiscordRichPresenceMode
    {
        [Description("禁用")]
        Off,

        [Description("开启(隐藏可供辨识的信息)")]
        Limited,

        [Description("开启(全部)")]
        Full
    }
}
