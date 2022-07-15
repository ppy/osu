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
        private class AppImageUpdateTool
        {
            /// <summary>
            /// Arguments passed to the appimageupdatetool
            /// <list type="bullet">
            /// <item><see cref="CHECK"/><description></description></item>
            /// <item><see cref="UPDATE"/></item>
            /// <item><see cref="OVERWRITE"/></item>
            /// <item><see cref="TOOL_VERSION"/></item>
            /// <item><see cref="TOOL_HELP"/></item>
            /// </list>
            /// </summary>
            public enum ARGS
            {
                /// <summary>
                /// Used to check for available updates with the embedded update information within the AppImage
                /// </summary>
                CHECK,
                /// <summary>
                /// Used to update the AppImage and renaming the old file with a .zs-old suffix
                /// </summary>
                UPDATE,
                /// <summary>
                /// Used to update the AppImage without keeping a backup
                /// </summary>
                OVERWRITE,
                /// <summary>
                /// Used to get the appimageupdatetool version
                /// </summary>
                TOOL_VERSION,
                /// <summary>
                /// Used to get the appimageupdatetool cli help message
                /// </summary>
                TOOL_HELP
            }
            /// TODO: Set commandFile to match the shipped binary to prevent searching and remove the unnecessary code
            private string commandFile;
            private string[] lookFor =
            {
                "appimageupdatetool",
                "appimageupdatetool-x86_64.AppImage"
            };
            private string Command
            {
                get
                {
                    if (commandFile is default(string))
                    {
                        foreach (string file in lookFor)
                        {
                            commandFile = file;
                            if (IsInstalled()) break;
                        }
                    }
                    return commandFile;
                }
            }
            private ProcessStartInfo StartInfo(ARGS args = ARGS.CHECK)
            {
                string arguments = "";
                string argument_spacer = " ";
                switch (args)
                {
                    case ARGS.CHECK:
                        arguments = "--check-for-update";
                        break;
                    case ARGS.OVERWRITE:
                        arguments = "--overwrite --remove-old";
                        break;
                    case ARGS.UPDATE:
                        arguments = "--overwrite";
                        break;
                    case ARGS.TOOL_VERSION:
                        arguments = "--version";
                        break;
                    case ARGS.TOOL_HELP:
                        arguments = "--help";
                        break;
                    default:
                        argument_spacer = "";
                        break;
                }

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = Command,
                    Arguments = $"{arguments}{argument_spacer}\"{AppImagePath}\"",
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
            private static AppImageUpdateTool instance;
            public static AppImageUpdateTool Instance
            {
                get
                {
                    if (instance is default(AppImageUpdateTool))
                    {
                        instance = new AppImageUpdateTool();
                    }
                    return instance;
                }
            }
            private States state;
            public static States State
            {
                get
                {
                    if (Instance.state is default(States))
                    {
                        Instance.state = Instance.hasUpdates() ?
                            States.UPDATESAVAILABLE :
                            States.NOUPDATESAVAILABLE;
                    }
                    return Instance.state;
                }
                private set
                {
                    Instance.state = value;
                }
            }
            /// <summary>
            /// Absolute path to AppImage file (with symlinks resolved)<para />
            /// See: https://docs.appimage.org/packaging-guide/environment-variables.html
            /// </summary>
            public static string AppImagePath
            {
                get
                {
                    return IsAppImage ?
                            // TODO: verify whether environment variable APPIMAGE is set in the deployed build
                            Environment.GetEnvironmentVariable("APPIMAGE") :
                            //Assume osu.AppImage otherwise (use this for debugging a test image)
                            $"{RuntimeInfo.StartupDirectory}osu.AppImage";
                }
            }
            /// <summary>
            /// Checks if osu was launched as an AppImage
            /// </summary>
            public static bool IsAppImage
            {
                get
                {
                    return Environment.GetEnvironmentVariable("APPIMAGE") is string;
                }
            }
            private bool hasUpdates()
            {
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = StartInfo(ARGS.CHECK);
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
            public static bool HasUpdates
            {
                get
                {
                    return State == States.UPDATESAVAILABLE;
                }
            }
            private string getOutput(ARGS arg)
            {
                try
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = StartInfo(arg);
                        process.Start();

                        var output = process.StandardError.ReadToEnd();
                        process.WaitForExit();
                        return output;
                    }
                }
                catch
                {
                    return null;
                }
            }
            private string version;
            /// <summary>
            /// Gets version information of the appimageupdatetool binary
            /// </summary>
            public static string Version
            {
                get
                {
                    if (Instance.version is default(string))
                    {
                        var output = Instance.getOutput(ARGS.TOOL_VERSION);
                        if (output != null)
                        {
                            var match = Regex.Match(output, @"version\s([a-zA-Z0-9\._-]*).*$", RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                output = match.Groups[1].ToString();
                            }
                        }
                        Instance.version = output;
                    }
                    return Instance.version;
                }
            }
            /// <summary>
            /// Help information of the appimageupdatetool binary
            /// </summary>
            public static string GetHelp()
            {
                return Instance.getOutput(ARGS.TOOL_HELP);
            }
            /// <summary>
            /// Checks if appimageupdatetool is installed
            /// </summary>
            public static bool IsInstalled()
            {
                return Version is string;
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
                if (State == States.UPDATESAVAILABLE)
                {
                    var process = new Process();
                    process.EnableRaisingEvents = true;
                    process.StartInfo = StartInfo(overwrite ? ARGS.OVERWRITE : ARGS.UPDATE);

                    process.Start();
                    process.BeginOutputReadLine();

                    float progress = 0;

                    if (update != null)
                    {
                        process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                        {
                            if (e.Data != null)
                            {
                                var match = Regex.Match(e.Data, @"^(?:(\d*(?:\.\d+)?)% done)((?:\s\((\d*(?:\.\d+)?) of (\d*(?:\.\d+)?)))?", RegexOptions.IgnoreCase);

                                if (match.Success)
                                {
                                    progress = float.Parse(match.Groups[1].ToString()) / 100;

                                    if (match.Groups[3].Success && match.Groups[4].Success)
                                    {
                                        float downloadedSize = float.Parse(match.Groups[3].ToString());
                                        float downloadSize = float.Parse(match.Groups[4].ToString());
                                    }
                                }
                                else if (Regex.Match(e.Data, @"verifying", RegexOptions.IgnoreCase).Success)
                                {
                                    State = States.VERIFYING;
                                }
                                update(progress, State);
                            }
                        });
                        update(0, States.DOWNLOADING);
                        process.Exited += new EventHandler((sender, e) =>
                        {
                            if (process.ExitCode == 0)
                            {
                                State = States.COMPLETED;
                            }
                            else
                            {
                                State = States.CANCELLED;
                            }
                            update(progress, State);
                            process.Dispose();
                        });
                    }
                }
                else
                {
                    update(1, State);
                }
            }
            /// <inheritdoc cref="ApplyUpdate"/>
            /// <param name="update">Delegate to handle state changes</param>
            /// <param name="overwrite">Whether to overwrite the AppImage</param>
            public Task ApplyUpdateAsync(Action<float, States> update = null, bool overwrite = false)
            {
                return Task.Run(() => Instance.ApplyUpdate(update, overwrite));
            }
        }
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
                                AppImageUpdateTool.Instance.ApplyUpdateAsync((progress, state) =>
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
        private Task PrepareUpdateAsync() =>
            Task.Run(() =>
            {
                if (preparedToRestart is default(bool))
                {
                    AppDomain.CurrentDomain.ProcessExit += delegate (object sender, EventArgs e)
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
        }
        /// <inheritdoc cref="AppImageUpdateTool.IsInstalled"/>
        public static bool IsInstalled
        {
            get
            {
                return AppImageUpdateTool.IsInstalled();
            }
        }

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