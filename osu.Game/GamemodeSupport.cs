using System;
using System.Runtime.InteropServices;
using osu.Framework.Logging;

namespace osu.Game
{
    public static class GamemodeSupport
    {
        [DllImport("libgamemode.so")]
        private static extern int real_gamemode_request_start();

        [DllImport("libgamemode.so")]
        private static extern int real_gamemode_request_end();

        private static bool gamemodeActivated;

        private static bool thrownOnce;

        public static void RequestStart()
        {
            try
            {
                if (!gamemodeActivated)
                {
                    int result = real_gamemode_request_start();

                    if (result != 0)
                    {
                        Logger.Log($"无法激活Gamemode: {result}", LoggingTarget.Runtime, LogLevel.Important);
                    }

                    gamemodeActivated = true;
                }
            }
            catch (Exception e)
            {
                if (e is DllNotFoundException)
                    notifyForLibMissing(e);
                else
                    Logger.Error(e, "激活Gamemode时出现了异常");
            }
        }

        public static void RequestEnd()
        {
            try
            {
                if (gamemodeActivated)
                {
                    int result = real_gamemode_request_end();
                    if (result != 0)
                        Logger.Log($"无法结束Gamemode: {result}", LoggingTarget.Runtime, LogLevel.Important);
                    gamemodeActivated = false;
                }
            }
            catch (Exception e)
            {
                if (e is DllNotFoundException)
                    notifyForLibMissing(e);
                else
                    Logger.Error(e, "结束Gamemode时出现了异常");
            }
        }

        private static void notifyForLibMissing(Exception e)
        {
            var level = thrownOnce ? LogLevel.Verbose : LogLevel.Important;

            Logger.Log($"没有找到Gamemode, 请确保它已经被正确安装在你的设备上: {e.Message}", LoggingTarget.Runtime, level);
            thrownOnce = true;
        }
    }
}
