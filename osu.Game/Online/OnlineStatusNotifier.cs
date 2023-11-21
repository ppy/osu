// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Online
{
    public partial class OnlineStatusNotifier : Component
    {
        private readonly Func<IScreen> getCurrentScreen;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        private IBindable<APIState> apiState = null!;
        private IBindable<bool> multiplayerState = null!;
        private IBindable<bool> spectatorState = null!;
        private bool forcedDisconnection;

        public OnlineStatusNotifier(Func<IScreen> getCurrentScreen)
        {
            this.getCurrentScreen = getCurrentScreen;
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            apiState = api.State.GetBoundCopy();
            multiplayerState = multiplayerClient.IsConnected.GetBoundCopy();
            spectatorState = spectatorClient.IsConnected.GetBoundCopy();

            multiplayerClient.Disconnecting += notifyAboutForcedDisconnection;
            spectatorClient.Disconnecting += notifyAboutForcedDisconnection;
        }

        private void notifyAboutForcedDisconnection()
        {
            if (forcedDisconnection)
                return;

            forcedDisconnection = true;
            notificationOverlay?.Post(new SimpleErrorNotification
            {
                Icon = FontAwesome.Solid.ExclamationCircle,
                Text = "You have been logged out on this device due to a login to your account on another device."
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiState.BindValueChanged(_ =>
            {
                if (apiState.Value == APIState.Online)
                    forcedDisconnection = false;

                Scheduler.AddOnce(updateState);
            });
            multiplayerState.BindValueChanged(_ => Scheduler.AddOnce(updateState));
            spectatorState.BindValueChanged(_ => Scheduler.AddOnce(updateState));
        }

        private void updateState()
        {
            if (forcedDisconnection)
                return;

            if (apiState.Value == APIState.Offline && getCurrentScreen() is OnlinePlayScreen)
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Icon = FontAwesome.Solid.ExclamationCircle,
                    Text = "API connection was lost. Can't continue with online play."
                });
                return;
            }

            if (!multiplayerClient.IsConnected.Value && multiplayerClient.Room != null)
            {
                notificationOverlay?.Post(new SimpleErrorNotification
                {
                    Icon = FontAwesome.Solid.ExclamationCircle,
                    Text = "Connection to the multiplayer server was lost. Exiting multiplayer."
                });
            }

            // TODO: handle spectator server failure somehow?
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient.IsNotNull())
                spectatorClient.Disconnecting -= notifyAboutForcedDisconnection;

            if (multiplayerClient.IsNotNull())
                multiplayerClient.Disconnecting -= notifyAboutForcedDisconnection;
        }
    }
}
