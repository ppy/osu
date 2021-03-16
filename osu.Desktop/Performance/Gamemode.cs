// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using osu.Framework.Logging;

namespace osu.Desktop.Performance
{
    public static class GamemodeRequest
    {
        [DllImport("libgamemode.so.0")]
        private static extern int real_gamemode_request_start();

        [DllImport("libgamemode.so.0")]
        private static extern int real_gamemode_request_end();

        public static void RequestStart()
        {
            try
            {
                int gamemodeOutput = real_gamemode_request_start();

                Logger.Log(gamemodeOutput < 0 ? "Gamemode \"Start\" request failed" : "Gamemode \"Start\" request was successfull");
            }
            catch
            {
                Logger.Log("Gamemode is not present on the system");
            }
        }

        public static void RequestEnd()
        {
            try
            {
                int gamemodeOutput = real_gamemode_request_end();

                Logger.Log(gamemodeOutput < 0 ? "Gamemode \"End\" request failed" : "Gamemode \"End\" request was successfull");
            }
            catch
            {
                Logger.Log("Gamemode is not present on the system");
            }
        }
    }
}
