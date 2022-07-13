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
    /// An update manager that shows notifications if a newer release of is detected.
    /// Updates the AppImage and backups the previous version.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public class AppImageUpdateManager : UpdateManager
    {
        private INotificationOverlay notificationOverlay;
        private string version;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, INotificationOverlay notifications)
        {
            notificationOverlay = notifications;
            version = game.Version;
        }

        /// <summary>
        /// Whether an update has been downloaded but not yet applied.
        /// </summary>
        private bool updatePending;

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
                    if (updatePending)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new UpdateCompleteNotification(this));
                        return true;
                    }

                    notificationOverlay.Post(new SimpleNotification
                    {
                        Text = $"A newer release of osu! has been found ({currentTagName} → {latestTagName}).\n\n"
                                + "Click here to download the new version.",
                        Icon = FontAwesome.Solid.Upload,
                        Activated = () =>
                        {
                            try{
                                var appimageupdatetool = new Process();
                                appimageupdatetool.EnableRaisingEvents = true;

                                //TODO use proper paths / filenames
                                appimageupdatetool.StartInfo = new ProcessStartInfo{
                                    FileName = "appimageupdatetool-x86_64.AppImage",
                                    Arguments = $"{RuntimeInfo.StartupDirectory}/osu.AppImage",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                };

                                appimageupdatetool.Start();
                                appimageupdatetool.BeginOutputReadLine();

                                if (notification == null)
                                {
                                    notification = new UpdateProgressNotification(this) { State = ProgressNotificationState.Active };
                                    Schedule(() => notificationOverlay.Post(notification));
                                }

                                appimageupdatetool.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
                                    if(e.Data != null){
                                        var match = Regex.Match(e.Data, @"^(?:(\d*(?:\.\d+)?)% done)(?:\s\((\d*(?:\.\d+)?) of (\d*(?:\.\d+)?))", RegexOptions.IgnoreCase);

                                        if(match.Success){

                                            float progress = float.Parse(match.Groups[1].ToString()) / 100;
                                            float downloadedSize = float.Parse(match.Groups[2].ToString());
                                            float downloadSize = float.Parse(match.Groups[3].ToString());

                                            notification.Progress = progress;
                                        }
                                        else if(Regex.Match(e.Data, @"verifying", RegexOptions.IgnoreCase).Success)
                                        {
                                            notification.Text = @"Verifying...";
                                        }
                                    }
                                });
                                
                                appimageupdatetool.Exited += new EventHandler((sender, e )=> {
                                    if(appimageupdatetool.ExitCode == 0)
                                    {
                                        notification.State = ProgressNotificationState.Completed;
                                        updatePending = true;
                                    }
                                });

                                notification.Progress = 0;
                                notification.Text = @"Downloading update...";
                                notification.State = ProgressNotificationState.Active;

                            }catch(System.ComponentModel.Win32Exception){
                                // appimageupdatetool-x86_64.AppImage is not installed
                            }
                            
                            return true;
                        }
                    });
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

        private class UpdateCompleteNotification : ProgressCompletionNotification
        {
            [Resolved]
            private OsuGame game { get; set; }

            public UpdateCompleteNotification(AppImageUpdateManager updateManager)
            {
                Text = @"Update ready to install. Click to restart!";

                Activated = () =>
                {
                    //TODO implement proper restart functionality
                    game.CloseAllOverlays();
                    game.AttemptExit();
                    
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