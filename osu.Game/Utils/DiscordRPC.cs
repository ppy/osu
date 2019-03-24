// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Logging;
using DiscordRPC;
using DiscordRPC.Logging;
using LogLevel = DiscordRPC.Logging.LogLevel;

namespace osu.Game.Utils
{
    public static class DiscordRpc
    {
        public static DiscordRpcClient client = new DiscordRpcClient("559391129716391967");

        /// <summary>Initializes and changes the Rich Presence in Discord.</summary>
        /// <param name="Presence">Value to set the presence</param>
        public static void updatePresence(RichPresence Presence)
        {
            client.Initialize();
            client.SetPresence(Presence);
            client.Invoke();
        }
    }
}
