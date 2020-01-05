// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    /// <summary>
    /// An update manager which only shows notifications after an update completes.
    /// </summary>
    public class UpdateManager : CompositeDrawable
    {
        [Resolved]
        private OsuConfigManager config { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        protected NotificationOverlay Notifications { get; private set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var version = game.Version;
            var lastVersion = config.Get<string>(OsuSetting.Version);

            if (game.IsDeployedBuild && version != lastVersion)
            {
                config.Set(OsuSetting.Version, version);

                // only show a notification if we've previously saved a version to the config file (ie. not the first run).
                if (!string.IsNullOrEmpty(lastVersion))
                    Notifications.Post(new UpdateCompleteNotification(version));
            }
        }

        private class UpdateCompleteNotification : SimpleNotification
        {
            private readonly string version;

            public UpdateCompleteNotification(string version)
            {
                this.version = version;
                Text = $"osu!lazer已更新到 {version}!\n点击这里查看最新的变更日志!";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChangelogOverlay changelog, NotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.CheckSquare;
                IconBackgound.Colour = colours.BlueDark;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    changelog.ShowBuild(OsuGameBase.CLIENT_STREAM_NAME, version);
                    return true;
                };
            }
        }
    }
}
