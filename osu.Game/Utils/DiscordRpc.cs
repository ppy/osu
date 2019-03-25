// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using DiscordRPC;

namespace osu.Game.Utils
{
    public static class DiscordRpc
    {
        private static readonly DiscordRpcClient client = new DiscordRpcClient("559391129716391967");

        /// <summary>Initializes and changes the Rich Presence in Discord.</summary>
        /// <param name="Presence">Value to set the presence</param>
        public static void UpdatePresence(RichPresence presence)
        {
            client.Initialize();
            client.SetPresence(presence);
            client.Invoke();
        }
    }
}
