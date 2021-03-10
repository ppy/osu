// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;

namespace Gamemode
{
    public static class GamemodeRequest {

        [DllImport("libgamemode.so.0")]
        public static extern int real_gamemode_request_start();

        [DllImport("libgamemode.so.0")]
        public static extern int real_gamemode_request_end();

        public static int RequestStart()
        {
            try {
                return real_gamemode_request_start();
            } catch {
                return -1;
            }
        }

        public static int RequestEnd()
        {
            try {
                return real_gamemode_request_end();
            } catch {
                return -1;
            }
        }
    }
}
