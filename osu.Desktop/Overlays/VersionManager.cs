// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
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

namespace osu.Desktop.Overlays
{
    public class VersionManager : OverlayContainer
    {
        private UpdateManager updateManager;
        private NotificationManager notification;

        protected override bool HideOnEscape => false;

        public override bool HandleInput => false;

        [BackgroundDependencyLoader]
        private void load(NotificationManager notification, OsuColour colours, TextureStore textures)
        {
            this.notification = notification;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Alpha = 0;

            bool isDebug = false;
            Debug.Assert(isDebug = true);

            var asm = Assembly.GetEntryAssembly().GetName();
            string version;
            if (asm.Version.Major == 0)
            {
                version = @"local " + (isDebug ? @"debug" : @"release");
            }
            else
                version = $@"{asm.Version.Major}.{asm.Version.Minor}.{asm.Version.Build}";

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

        private async void updateChecker()
        {
            updateManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/ppy/osu", @"osulazer", null, null, true);

            if (!updateManager.IsInstalledApp)
                return;

            var info = await updateManager.CheckForUpdate();
            if (info.ReleasesToApply.Count > 0)
            {
                ProgressNotification n = new UpdateProgressNotification
                {
                    Text = @"Downloading update..."
                };
                Schedule(() => notification.Post(n));
                Schedule(() => n.State = ProgressNotificationState.Active);
                await updateManager.DownloadReleases(info.ReleasesToApply, (int p) => Schedule(() => n.Progress = p / 100f));
                Schedule(() => n.Text = @"Installing update...");
                await updateManager.ApplyReleases(info, (int p) => Schedule(() => n.Progress = p / 100f));
                Schedule(() => n.State = ProgressNotificationState.Completed);

            }
            else
            {
                //check again every 30 minutes.
                Scheduler.AddDelayed(updateChecker, 60000 * 30);
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
        }
    }
}
