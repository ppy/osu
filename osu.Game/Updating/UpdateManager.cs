// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Updating
{
    /// <summary>
    /// Represents a manager for interacting with the provided <see cref="Updater"/> to check and perform updates,
    /// or without an <see cref="Updater"/> to at least perform required action for finalizing a previous update.
    /// </summary>
    public class UpdateManager : CompositeDrawable
    {
        private readonly Updater updater;

        /// <summary>
        /// Whether this <see cref="UpdateManager"/> has an updater provided to it.
        /// </summary>
        public bool HasUpdater => updater != null;

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        [Resolved]
        private NotificationOverlay notifications { get; set; }

        /// <summary>
        /// Constructs a new <see cref="UpdateManager"/> with the provided <paramref name="updater"/>.
        /// </summary>
        /// <param name="updater">The <see cref="Updater"/> component.</param>
        public UpdateManager([NotNull] Updater updater)
        {
            InternalChild = this.updater = updater;
        }

        /// <summary>
        /// Constructs a new <see cref="UpdateManager"/> with no updater.
        /// </summary>
        public UpdateManager()
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            finalizeAnyPreviousUpdate();

            // Attempt automatically checking for any new updates.
            if (HasUpdater)
                Schedule(() => Task.Run(updater.CheckAndPrepareAsync));
        }

        /// <summary>
        /// Finalizes any previous update if did and sets up configuration to current game instance build.
        /// </summary>
        private void finalizeAnyPreviousUpdate()
        {
            var version = game.Version;
            var lastVersion = config.Get<string>(OsuSetting.Version);

            if (game.IsDeployedBuild && version != lastVersion)
            {
                // only show a notification if we've previously saved a version to the config file (ie. not the first run).
                if (!string.IsNullOrEmpty(lastVersion))
                    notifications.Post(new UpdateCompleteNotification(version));
            }

            // debug / local compilations will reset to a non-release string.
            // can be useful to check when an install has transitioned between release and otherwise (see OsuConfigManager's migrations).
            config.Set(OsuSetting.Version, version);
        }

        private class UpdateCompleteNotification : SimpleNotification
        {
            private readonly string version;

            public UpdateCompleteNotification(string version)
            {
                this.version = version;
                Text = $"You are now running osu!lazer {version}.\nClick to see what's new!";
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
