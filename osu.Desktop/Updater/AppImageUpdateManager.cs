// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Updater;
using osuTK;
using osuTK.Graphics;

namespace osu.Desktop.Updater
{
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

    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.<para />
    /// Updates the AppImage and backups the previous version with a .zs-old suffix.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public class AppImageUpdateManager : UpdateManager
    {
        /// <summary>
        /// Implements appimageupdatetool functionality via cli
        /// </summary>
        private INotificationOverlay notificationOverlay;

        private string version;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, INotificationOverlay notifications)
        {
            notificationOverlay = notifications;
            version = game.Version;
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync(UpdateProgressNotification notification = null)
        {
            // should we schedule a retry on completion of this check?
            bool scheduleRecheck = true;

            try
            {
                var releases = new OsuJsonWebRequest<GitHubRelease>("https://api.github.com/repos/ppy/osu/releases/latest");

                await releases.PerformAsync().ConfigureAwait(false);

                var latest = releases.ResponseObject;

                // avoid any discrepancies due to build suffixes for now.
                // eventually we will want to support release streams and consider these.
                string currentTagName = version.Split('-').First();
                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != version)
                {
                    if (AppImageUpdateTool.State == AppImageUpdateTool.States.COMPLETED)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new UpdateCompleteNotification(this));
                        return true;
                    }

                    if (AppImageUpdateTool.HasUpdates)
                    {
                        notificationOverlay.Post(new SimpleNotification
                        {
                            Text = $"A newer release of osu! has been found ({currentTagName} → {latestTagName}).\n\n"
                                   + "Click here to download the new version.",
                            Icon = FontAwesome.Solid.Upload,
                            Activated = () =>
                            {
                                if (notification == null)
                                {
                                    notification = new UpdateProgressNotification(this);
                                    Schedule(() => notificationOverlay.Post(notification));
                                }

                                notification.Text = @"Downloading update...";
                                AppImageUpdateTool.ApplyUpdateAsync((progress, state) =>
                                {
                                    notification.Progress = progress;

                                    switch (state)
                                    {
                                        case AppImageUpdateTool.States.DOWNLOADING:
                                            notification.State = ProgressNotificationState.Active;
                                            break;

                                        case AppImageUpdateTool.States.VERIFYING:
                                            notification.Text = @"Installing update...";
                                            notification.State = ProgressNotificationState.Active;
                                            break;

                                        case AppImageUpdateTool.States.COMPLETED:
                                            notification.State = ProgressNotificationState.Completed;
                                            break;

                                        case AppImageUpdateTool.States.CANCELLED:
                                            notification.State = ProgressNotificationState.Cancelled;
                                            break;
                                    }
                                });
                                return true;
                            }
                        });
                    }

                    return true;
                }
                else
                {
                    scheduleRecheck = false;
                }
            }
            catch (Exception)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
                scheduleRecheck = true;
            }
            finally
            {
                if (scheduleRecheck)
                {
                    // check again in 30 minutes.
                    Scheduler.AddDelayed(() => Task.Run(async () => await checkForUpdateAsync().ConfigureAwait(false)), 60000 * 30);
                }
            }

            return true;
        }

        private bool preparedToRestart;

        public Task PrepareUpdateAsync() =>
            Task.Run(() =>
            {
                if (!preparedToRestart)
                {
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        using (Process process = new Process())
                        {
                            process.StartInfo = new ProcessStartInfo
                            {
                                FileName = AppImageUpdateTool.AppImagePath,
                                UseShellExecute = false
                            };
                            // NOTE: throws an Exception if the debugged AppImage is not made executable
                            process.Start();
                        }
                    };
                    preparedToRestart = true;
                }
            });

        /// <inheritdoc cref="AppImageUpdateTool.IsInstalled"/>
        public static bool IsInstalled => AppImageUpdateTool.IsInstalled;

        private class UpdateCompleteNotification : ProgressCompletionNotification
        {
            [Resolved]
            private OsuGame game { get; set; }

            public UpdateCompleteNotification(AppImageUpdateManager updateManager)
            {
                Text = @"Update ready to install. Click to restart!";

                Activated = () =>
                {
                    updateManager.PrepareUpdateAsync()
                                 .ContinueWith(_ => updateManager.Schedule(() => game?.AttemptExit()));
                    return true;
                };
            }
        }

        private class UpdateProgressNotification : ProgressNotification
        {
            private readonly AppImageUpdateManager updateManager;

            public UpdateProgressNotification(AppImageUpdateManager updateManager)
            {
                this.updateManager = updateManager;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new UpdateCompleteNotification(updateManager);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconContent.AddRange(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(colours.YellowDark, colours.Yellow)
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.Upload,
                        Colour = Color4.White,
                        Size = new Vector2(20),
                    }
                });
            }

            public override void Close()
            {
                // cancelling updates is not currently supported by the underlying updater.
                // only allow dismissing for now.

                switch (State)
                {
                    case ProgressNotificationState.Cancelled:
                        base.Close();
                        break;
                }
            }
        }
    }
}
