// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using Squirrel;
using System.Reflection;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using System.Net.Http;
using osu.Framework.Logging;

namespace osu.Desktop.Overlays
{
    public class VersionManager : OverlayContainer
    {
        private UpdateManager updateManager;
        private NotificationManager notificationManager;

        AssemblyName assembly = Assembly.GetEntryAssembly().GetName();

        public bool IsDeployedBuild => assembly.Version.Major > 0;

        protected override bool HideOnEscape => false;

        public override bool HandleInput => false;

        [BackgroundDependencyLoader]
        private void load(NotificationManager notification, OsuColour colours, TextureStore textures)
        {
            this.notificationManager = notification;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Alpha = 0;

            bool isDebug = false;
            Debug.Assert(isDebug = true);

            string version;
            if (!IsDeployedBuild)
            {
                version = @"local " + (isDebug ? @"debug" : @"release");
            }
            else
                version = $@"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}";

            Children = new Drawable[]
            {
                new FlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FlowDirections.Vertical,
                    Children = new Drawable[]
                    {
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FlowDirections.Horizontal,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = @"Exo2.0-Bold",
                                    Text = $@"osu!lazer"
                                },
                                new OsuSpriteText
                                {
                                    Colour = isDebug ? colours.Red : Color4.White,
                                    Text = version
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextSize = 12,
                            Colour = colours.Yellow,
                            Font = @"Venera",
                            Text = $@"Development Build"
                        },
                        new Sprite
                        {
                            Texture = textures.Get(@"Menu/dev-build-footer"),
                        },
                    }
                }
            };

            if (IsDeployedBuild)
                updateChecker();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            State = Visibility.Visible;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private async void updateChecker(bool useDeltaPatching = true, UpdateProgressNotification notification = null)
        {
            //should we schedule a retry on completion of this check?
            bool scheduleRetry = true;

            try
            {
                if (updateManager == null) updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/ppy/osu", @"osulazer", null, null, true);

                var info = await updateManager.CheckForUpdate(!useDeltaPatching);
                if (info.ReleasesToApply.Count == 0)
                    //no updates available. bail and retry later.
                    return;

                if (notification == null)
                {
                    notification = new UpdateProgressNotification { State = ProgressNotificationState.Active };
                    Schedule(() => notificationManager.Post(notification));
                }

                Schedule(() =>
                {
                    notification.Progress = 0;
                    notification.Text = @"Downloading update...";
                });

                try
                {
                    await updateManager.DownloadReleases(info.ReleasesToApply, p => Schedule(() => notification.Progress = p / 100f));

                    Schedule(() =>
                    {
                        notification.Progress = 0;
                        notification.Text = @"Installing update...";
                    });

                    await updateManager.ApplyReleases(info, p => Schedule(() => notification.Progress = p / 100f));

                    Schedule(() => notification.State = ProgressNotificationState.Completed);
                }
                catch (Exception)
                {
                    if (useDeltaPatching)
                    {
                        //could fail if deltas are unavailable for full update path (https://github.com/Squirrel/Squirrel.Windows/issues/959)
                        //try again without deltas.
                        updateChecker(false, notification);
                        scheduleRetry = false;
                    }
                }
            }
            catch (HttpRequestException)
            {
                //likely have no internet connection.
                //we'll ignore this and retry later.
            }
            finally
            {
                if (scheduleRetry)
                {
                    //check again in 30 minutes.
                    Scheduler.AddDelayed(() => updateChecker(), 60000 * 30);
                    if (notification != null)
                        notification.State = ProgressNotificationState.Cancelled;
                }
            }
        }

        protected override void PopIn()
        {
            FadeIn(1000);
        }

        protected override void PopOut()
        {
        }

        class UpdateProgressNotification : ProgressNotification
        {
            protected override Notification CreateCompletionNotification() => new ProgressCompletionNotification(this)
            {
                Text = @"Update ready to install. Click to restart!",
                Activated = () =>
                {
                    UpdateManager.RestartApp();
                    return true;
                }
            };

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconContent.Add(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColourInfo = ColourInfo.GradientVertical(colours.YellowDark, colours.Yellow)
                    },
                    new TextAwesome
                    {
                        Anchor = Anchor.Centre,
                        Icon = FontAwesome.fa_upload,
                        Colour = Color4.White,
                    }
                });
            }
        }
    }
}
