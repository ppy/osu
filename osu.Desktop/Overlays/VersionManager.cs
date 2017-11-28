// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Notifications;
using osu.Game.Overlays;
using OpenTK;
using OpenTK.Graphics;
using Squirrel;

namespace osu.Desktop.Overlays
{
    public class VersionManager : OverlayContainer
    {
        private UpdateManager updateManager;
        private NotificationOverlay notificationOverlay;
        private OsuConfigManager config;
        private OsuGameBase game;
        private OsuColour osuColours;

        public override bool HandleInput => false;

        [BackgroundDependencyLoader]
        private void load(NotificationOverlay notification, OsuColour colours, TextureStore textures, OsuGameBase game, OsuConfigManager config)
        {
            notificationOverlay = notification;
            this.config = config;
            this.game = game;
            osuColours = colours;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Alpha = 0;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = @"Exo2.0-Bold",
                                    Text = game.Name
                                },
                                new OsuSpriteText
                                {
                                    Colour = DebugUtils.IsDebug ? colours.Red : Color4.White,
                                    Text = game.Version
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
                            Text = @"Development Build"
                        },
                        new Sprite
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Texture = textures.Get(@"Menu/dev-build-footer"),
                        },
                    }
                }
            };

            if (game.IsDeployedBuild)
                checkForUpdateAsync();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var version = game.Version;
            var lastVersion = config.Get<string>(OsuSetting.Version);
            if (!game.IsDeployedBuild || version == lastVersion) return;
            config.Set(OsuSetting.Version, version);

            // only show a notificationDrawable if we've previously saved a version to the config file (ie. not the first run).
            if (!string.IsNullOrEmpty(lastVersion))
                Scheduler.AddDelayed(() =>
                    notificationOverlay.Post(
                            new Notification(
                            $"You are now running osu!lazer {version}.\nClick to see what's new!",
                            onActivate: () =>
                            {
                                Process.Start($"https://github.com/ppy/osu/releases/tag/v{version}");
                            })
                        {
                            NotificationIcon = new NotificationIcon(FontAwesome.fa_check_square, osuColours.BlueDark)
                        }),
                    5000
                );
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            updateManager?.Dispose();
        }

        private async void checkForUpdateAsync(bool useDeltaPatching = true, ProgressNotification notificationContainer = null)
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

                if (notificationContainer == null)
                {
                    notificationContainer = new ProgressNotification("", FontAwesome.fa_upload)
                    {
                        FollowUpNotifications =
                        {
                            new Notification(@"Update ready to install. Click to restart!", onActivate: () =>
                            {
                                // Squirrel returns execution to us after the update process is started, so it's safe to use Wait() here
                                UpdateManager.RestartAppWhenExited().Wait();
                                game.Exit();
                            })
                            {
                                NotificationIcon = new NotificationIcon(backgroundColour: osuColours.Green)
                            }
                        },
                        NotificationIcon = new NotificationIcon(
                            FontAwesome.fa_upload,
                            ColourInfo.GradientVertical(osuColours.YellowDark, osuColours.Yellow)
                        )
                    };

                    notificationOverlay.Post(notificationContainer);
                }


                notificationContainer.Progress = 0;
                notificationContainer.Text = @"Downloading update...";

                try
                {
                    await updateManager.DownloadReleases(info.ReleasesToApply, p => notificationContainer.Progress = p / 100f);

                    notificationContainer.Progress = 0;
                    notificationContainer.State = ProgressNotificationState.Active;
                    notificationContainer.Text = @"Installing update...";

                    await updateManager.ApplyReleases(info, p => notificationContainer.Progress = p / 100f);

                    Schedule(() => notificationContainer.State = ProgressNotificationState.Completed);
                }
                catch (Exception e)
                {
                    if (useDeltaPatching)
                    {
                        Logger.Error(e, @"delta patching failed!");

                        //could fail if deltas are unavailable for full update path (https://github.com/Squirrel/Squirrel.Windows/issues/959)
                        //try again without deltas.
                        checkForUpdateAsync(false, notificationContainer);
                        scheduleRetry = false;
                    }
                    else
                    {
                        Logger.Error(e, @"update failed!");
                    }
                }
            }
            catch (Exception)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
            }
            finally
            {
                if (scheduleRetry)
                {
                    //check again in 30 minutes.
                    Scheduler.AddDelayed(() => checkForUpdateAsync(), 60000 * 30);
                    if (notificationContainer != null)
                        notificationContainer.State = ProgressNotificationState.Cancelled;
                }
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(1000);
        }

        protected override void PopOut()
        {
        }

    }
}
