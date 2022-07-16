// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu.Framework;

namespace osu.Desktop.Linux
{
    [SupportedOSPlatform("linux")]
    public class AppImageUpdateTool
    {
        /// <summary>
        /// Arguments passed to the appimageupdatetool
        /// </summary>
        public enum ToolArguments
        {
            None,
            Check,
            Update,
            Overwrite,
            ToolVersion,
            ToolHelp
        }

        /// TODO: Set commandFile to match the shipped binary to prevent searching and remove the unnecessary loop in the constructor
        private readonly string commandFile;

        /// <summary>
        /// Gets version information of the appimageupdatetool binary
        /// </summary>
        public readonly string Version;

        /// <summary>
        /// Checks if appimageupdatetool is installed
        /// </summary>
        public readonly bool IsInstalled;

        public States State { get; private set; } = States.NotChecked;

        public AppImageUpdateTool()
        {
            string[] toollookupfilenames =
            {
                "appimageupdatetool",
                "appimageupdatetool-x86_64.AppImage"
            };

            foreach (string file in toollookupfilenames)
            {
                commandFile = file;
                Version = getVersion();
                if (Version != null) break;
            }

            IsInstalled = Version != null;
        }

        private ProcessStartInfo startInfo(ToolArguments toolArguments = ToolArguments.None)
        {
            string arguments = "";

            switch (toolArguments)
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
            }

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = commandFile,
                Arguments = $"{arguments} \"{AppImagePath}\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            return info;
        }

        public enum States
        {
            NotChecked,
            NoUpdatesAvailable,
            UpdatesAvailable,
            Downloading,
            Verifying,
            Completed,
            Canceled
        }

        /// <summary>
        /// Absolute path to AppImage file (with symlinks resolved)<para />
        /// See: https://docs.appimage.org/packaging-guide/environment-variables.html<para />
        /// TODO: verify whether environment variable APPIMAGE is set in the deployed build<para />
        /// Will Assume osu.AppImage in the <see cref="RuntimeInfo.StartupDirectory" /> otherwise
        /// </summary>
        public static string AppImagePath => IsAppImage
            ? Environment.GetEnvironmentVariable("APPIMAGE")
            : $"{RuntimeInfo.StartupDirectory}osu.AppImage";

        /// <summary>
        /// Checks if osu was launched as an AppImage
        /// </summary>
        public static bool IsAppImage => Environment.GetEnvironmentVariable("APPIMAGE") != null;

        /// <summary>
        /// Checks if there is a new release of the AppImage with the embedded update information and updates the state
        /// </summary>
        public bool HasUpdates()
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = startInfo(ToolArguments.Check);
                    process.Start();

                    process.WaitForExit();
                    State = process.ExitCode == 1
                        ? States.UpdatesAvailable
                        : States.NoUpdatesAvailable;
                    return process.ExitCode == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        private string getOutput(ToolArguments arg)
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
                // In case the appimageupdatetool got uninstalled after calling IsInstalled
                return null;
            }
        }

        private string getVersion()
        {
            string output = getOutput(ToolArguments.ToolVersion);

            if (output == null) return null;

            var match = Regex.Match(output, @"version\s([^\s]*)\s.*$", RegexOptions.IgnoreCase);

            return match.Success
                ? match.Groups[1].ToString()
                : null;
        }

        /// <summary>
        /// Help information of the appimageupdatetool binary
        /// </summary>
        public string GetHelp()
        {
            return getOutput(ToolArguments.ToolHelp);
        }

        /// <summary>
        /// Fetches updated blocks via zsync and updates the appimage
        /// </summary>
        /// <remarks>
        /// Progress begins with the amount of usable data from the seed file
        /// </remarks>
        /// <param name="update">Delegate to handle state changes</param>
        /// <param name="overwrite">Whether to overwrite the AppImage</param>
        public void ApplyUpdate(Action<float, States> update = null, bool overwrite = false)
        {
            if (State == States.UpdatesAvailable)
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
                            // match.Groups[1]: progress 0-100
                            // match.Groups[2]: optional string containing file sizes
                            // match.Groups[3]: usable + downloaded size
                            // match.Groups[4]: final file size
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
                                State = States.Verifying;
                            }

                            update(progress, State);
                        }
                    };
                    update(0, States.Downloading);
                    process.Exited += (sender, e) =>
                    {
                        State = process.ExitCode == 0 ? States.Completed : States.Canceled;
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
        public Task ApplyUpdateAsync(Action<float, States> update = null, bool overwrite = false)
        {
            return Task.Run(() => ApplyUpdate(update, overwrite));
        }
    }
}
