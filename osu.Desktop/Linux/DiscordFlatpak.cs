// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using osu.Framework.Logging;

namespace osu.Desktop.Linux
{
    [SupportedOSPlatform("linux")]
    public static class DiscordFlatpak
    {
        /// <summary>
        /// Discord Flatpak packages names
        /// Examples: com.discordapp.Discord, com.discordapp.DiscordCanary
        /// </summary>
        private static readonly string[] discord_package_names =
        {
            "com.discordapp.Discord", "com.discordapp.DiscordCanary"
        };

        /// <summary>
        /// Get Discord IPC Path for Flatpak installation.
        /// </summary>
        private static string? getDiscordFlatpakIpcPath()
        {
            string xdgRuntimeDir = XdgUtils.GetXdgRuntimeDir();

            foreach (string discordPackageName in discord_package_names)
            {
                string discordDirectory = Path.Combine(xdgRuntimeDir, ".flatpak/" + discordPackageName + "/xdg-run");

                if (!Path.Exists(discordDirectory))
                {
                    continue;
                }

                for (int socketNum = 0; socketNum < 10; socketNum++)
                {
                    string socketPath = Path.Combine(discordDirectory, $"discord-ipc-{socketNum}");
                    if (File.Exists(socketPath))
                    {
                        return socketPath;
                    }
                }
            }

            return null;
        }

        private static void TryCreateLinuxSymlink(string filePath, string targetPath)
        {
            Process symlinkProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ln",
                    Arguments = $"-s \"{targetPath}\" \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            try
            {
                symlinkProcess.Start();
                symlinkProcess.WaitForExit();

                if (symlinkProcess.ExitCode != 0)
                {
                    string error = symlinkProcess.StandardError.ReadToEnd();
                    Logger.Error(new ArgumentException(), $"Unable to create symbolic link: {error}");
                    return;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Unable to create symbolic link: {e.Message}");
            }
        }

        /// <summary>
        /// Creates symlink for Discord's socket from Discord's Flatpak installation.
        /// </summary>
        public static void TryCreateLinuxDiscordFlatpakSymlink()
        {
            // When Discord is installed as Flatpak,
            // it usually creates the UNIX socket file in
            // `/run/user/{uid}/.flatpak/com.discordapp.Discord/xdg-run/discord-ipc-{number}`

            string xdgRuntimeDir = XdgUtils.GetXdgRuntimeDir();
            string defaultDiscordIpcPath = Path.Combine(xdgRuntimeDir, "discord-ipc-0");

            if (Path.Exists(defaultDiscordIpcPath))
            {
                var linkInfo = new FileInfo(defaultDiscordIpcPath);

                if (linkInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) && Path.Exists(linkInfo.LinkTarget))
                {
                    File.Delete(defaultDiscordIpcPath);
                }
                else
                {
                    Logger.Log(@"Discord IPC already exists, skipping");
                    return;
                }
            }

            string? discordFlatpakIpcPath = getDiscordFlatpakIpcPath();
            Logger.LogPrint($"Got Discord Flatpak IPC Path: {discordFlatpakIpcPath}");

            if (discordFlatpakIpcPath == null)
            {
                Logger.LogPrint(@"Could not find Discord Flatpak IPC Socket.");
                return;
            }

            Logger.LogPrint($"Trying to create a symlink: {defaultDiscordIpcPath} -> {discordFlatpakIpcPath}");
            TryCreateLinuxSymlink(defaultDiscordIpcPath, discordFlatpakIpcPath);
        }
    }
}
