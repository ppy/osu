// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu.Framework;

namespace osu.Desktop.Updater
{
    [SupportedOSPlatform("linux")]
    public static class AppImageUpdateTool
    {
        /// <summary>
        /// Arguments passed to the appimageupdatetool
        /// <list type="bullet">
        /// <item><see cref="Check"/><description></description></item>
        /// <item><see cref="Update"/></item>
        /// <item><see cref="Overwrite"/></item>
        /// <item><see cref="ToolVersion"/></item>
        /// <item><see cref="ToolHelp"/></item>
        /// </list>
        /// </summary>
        public enum ToolArguments
        {
            /// <summary>
            /// Used to check for available updates with the embedded update information within the AppImage
            /// </summary>
            Check,

            /// <summary>
            /// Used to update the AppImage and renaming the old file with a .zs-old suffix
            /// </summary>
            Update,

            /// <summary>
            /// Used to update the AppImage without keeping a backup
            /// </summary>
            Overwrite,

            /// <summary>
            /// Used to get the appimageupdatetool version
            /// </summary>
            ToolVersion,

            /// <summary>
            /// Used to get the appimageupdatetool cli help message
            /// </summary>
            ToolHelp
        }

        /// TODO: Set commandFile to match the shipped binary to prevent searching and remove the unnecessary code
        private static string commandFile;

        private static readonly string[] toollookupfilenames =
        {
            "appimageupdatetool",
            "appimageupdatetool-x86_64.AppImage"
        };

        private static string command
        {
            get
            {
                if (commandFile is default(string))
                {
                    foreach (string file in toollookupfilenames)
                    {
                        commandFile = file;
                        if (IsInstalled) break;
                    }
                }

                return commandFile;
            }
        }

        private static ProcessStartInfo startInfo(ToolArguments args = ToolArguments.Check)
        {
            string arguments = "";
            string argumentSpacer = " ";

            switch (args)
            {
                case ToolArguments.Check:
                    arguments = "--check-for-update";
                    break;

                case ToolArguments.Overwrite:
                    arguments = "--overwrite --remove-old";
                    break;

                case ToolArguments.Update:
                    arguments = "--overwrite";
                    break;

                case ToolArguments.ToolVersion:
                    arguments = "--version";
                    break;

                case ToolArguments.ToolHelp:
                    arguments = "--help";
                    break;

                default:
                    argumentSpacer = "";
                    break;
            }

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = command,
                Arguments = $"{arguments}{argumentSpacer}\"{AppImagePath}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            return info;
        }

        public enum States
        {
            NOUPDATESAVAILABLE,
            UPDATESAVAILABLE,
            DOWNLOADING,
            VERIFYING,
            COMPLETED,
            CANCELLED
        }

        private static States state;

        public static States State
        {
            get
            {
                if (state is default(States))
                {
                    state = hasUpdates() ? States.UPDATESAVAILABLE : States.NOUPDATESAVAILABLE;
                }

                return state;
            }
            private set => state = value;
        }

        /// <summary>
        /// Absolute path to AppImage file (with symlinks resolved)<para />
        /// See: https://docs.appimage.org/packaging-guide/environment-variables.html<para />
        /// TODO: verify whether environment variable APPIMAGE is set in the deployed build<para />
        /// Will Assume osu.AppImage in the <see cref="RuntimeInfo.StartupDirectory" /> otherwise
        /// </summary>
        public static string AppImagePath => IsAppImage ? Environment.GetEnvironmentVariable("APPIMAGE") : $"{RuntimeInfo.StartupDirectory}osu.AppImage";

        /// <summary>
        /// Checks if osu was launched as an AppImage
        /// </summary>
        public static bool IsAppImage => Environment.GetEnvironmentVariable("APPIMAGE") != null;

        private static bool hasUpdates()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = startInfo();
                    process.Start();

                    process.WaitForExit();
                    return process.ExitCode == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if there is a new release of the AppImage with the embedded update information
        /// </summary>
        public static bool HasUpdates => State == States.UPDATESAVAILABLE;

        private static string getOutput(ToolArguments arg)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = startInfo(arg);
                    process.Start();

                    string output = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return output;
                }
            }
            catch
            {
                return null;
            }
        }

        private static string version;

        /// <summary>
        /// Gets version information of the appimageupdatetool binary
        /// </summary>
        public static string Version
        {
            get
            {
                if (version is default(string))
                {
                    string output = getOutput(ToolArguments.ToolVersion);

                    if (output != null)
                    {
                        var match = Regex.Match(output, @"version\s([a-zA-Z0-9\._-]*).*$", RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            output = match.Groups[1].ToString();
                        }
                    }

                    version = output;
                }

                return version;
            }
        }

        /// <summary>
        /// Help information of the appimageupdatetool binary
        /// </summary>
        public static string GetHelp()
        {
            return getOutput(ToolArguments.ToolHelp);
        }

        /// <summary>
        /// Checks if appimageupdatetool is installed
        /// </summary>
        public static bool IsInstalled => Version != null;

        /// <summary>
        /// Fetches updated blocks via zsync and updates the appimage
        /// </summary>
        /// <remarks>
        /// Progress begins with the amount of usable data from the seed file
        /// </remarks>
        /// <param name="update">Delegate to handle state changes</param>
        /// <param name="overwrite">Whether to overwrite the AppImage</param>
        public static void ApplyUpdate(Action<float, States> update = null, bool overwrite = false)
        {
            if (State == States.UPDATESAVAILABLE)
            {
                var process = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = startInfo(overwrite ? ToolArguments.Overwrite : ToolArguments.Update)
                };

                process.Start();
                process.BeginOutputReadLine();

                float progress = 0;

                if (update != null)
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            var match = Regex.Match(e.Data, @"^(?:(\d*(?:\.\d+)?)% done)((?:\s\((\d*(?:\.\d+)?) of (\d*(?:\.\d+)?)))?", RegexOptions.IgnoreCase);

                            if (match.Success)
                            {
                                progress = float.Parse(match.Groups[1].ToString()) / 100;

                                if (match.Groups[3].Success && match.Groups[4].Success)
                                {
                                    //float downloadedSize = float.Parse(match.Groups[3].ToString());
                                    //float downloadSize = float.Parse(match.Groups[4].ToString());
                                }
                            }
                            else if (Regex.Match(e.Data, @"verifying", RegexOptions.IgnoreCase).Success)
                            {
                                State = States.VERIFYING;
                            }

                            update(progress, State);
                        }
                    };
                    update(0, States.DOWNLOADING);
                    process.Exited += (sender, e) =>
                    {
                        State = process.ExitCode == 0 ? States.COMPLETED : States.CANCELLED;
                        update(progress, State);
                        process.Dispose();
                    };
                }
            }
            else
            {
                update?.Invoke(1, State);
            }
        }

        /// <inheritdoc cref="ApplyUpdate"/>
        /// <param name="update">Delegate to handle state changes</param>
        /// <param name="overwrite">Whether to overwrite the AppImage</param>
        public static Task ApplyUpdateAsync(Action<float, States> update = null, bool overwrite = false)
        {
            return Task.Run(() => ApplyUpdate(update, overwrite));
        }
    }
}
