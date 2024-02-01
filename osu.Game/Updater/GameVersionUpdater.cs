// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updater
{
    /// <summary>
    /// This component is responsible for updating/comparing the osu! version string in the config.
    /// This will show a notification suggesting users to read the changelogs if the game is on a newer version than last run.
    /// </summary>
    public partial class GameVersionUpdater : Component
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        protected INotificationOverlay Notifications { get; private set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            string version = game.Version;

            string lastVersion = config.Get<string>(OsuSetting.Version);

            if (game.IsDeployedBuild && version != lastVersion)
            {
                // only show a notification if we've previously saved a version to the config file (ie. not the first run).
                if (!string.IsNullOrEmpty(lastVersion))
                    Notifications.Post(new UpdateCompleteNotification(version));
            }

            // debug / local compilations will reset to a non-release string.
            // can be useful to check when an install has transitioned between release and otherwise (see OsuConfigManager's migrations).
            config.SetValue(OsuSetting.Version, version);
        }

        public partial class UpdateCompleteNotification : SimpleNotification
        {
            private readonly string version;

            public UpdateCompleteNotification(string version)
            {
                this.version = version;
                Text = $"You are now running osu! {version}.\nClick to see what's new!";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, ChangelogOverlay changelog, INotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.CheckSquare;
                IconContent.Colour = colours.BlueDark;

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
