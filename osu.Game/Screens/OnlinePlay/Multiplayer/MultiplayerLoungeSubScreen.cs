// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private Dropdown<RoomPermissionsFilter> roomAccessTypeDropdown = null!;
        private OsuCheckbox showInProgress = null!;

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            // Upon having left a room, we don't know whether we were the only participant, and whether the room is now closed as a result of leaving it.
            // To work around this, temporarily remove the room and trigger an immediate listing poll.
            if (e.Last is MultiplayerMatchSubScreen match)
            {
                RoomManager?.RemoveRoom(match.Room);
                ListingPollingComponent.PollImmediately();
            }
        }

        protected override IEnumerable<Drawable> CreateFilterControls()
        {
            foreach (var control in base.CreateFilterControls())
                yield return control;

            yield return roomAccessTypeDropdown = new SlimEnumDropdown<RoomPermissionsFilter>
            {
                RelativeSizeAxes = Axes.None,
                Current = Config.GetBindable<RoomPermissionsFilter>(OsuSetting.MultiplayerRoomFilter),
                Width = 160,
            };

            roomAccessTypeDropdown.Current.BindValueChanged(_ => UpdateFilter());

            yield return showInProgress = new OsuCheckbox
            {
                LabelText = "Show in-progress rooms",
                RelativeSizeAxes = Axes.None,
                Width = 220,
                Padding = new MarginPadding { Vertical = 5, },
                Current = Config.GetBindable<bool>(OsuSetting.MultiplayerShowInProgressFilter),
            };

            showInProgress.Current.BindValueChanged(_ => UpdateFilter());
            StatusDropdown.Current.BindValueChanged(_ => showInProgress.Alpha = StatusDropdown.Current.Value == RoomModeFilter.Open ? 1 : 0, true);
        }

        protected override FilterCriteria CreateFilterCriteria()
        {
            var criteria = base.CreateFilterCriteria();
            criteria.Category = @"realtime";
            criteria.Permissions = roomAccessTypeDropdown.Current.Value;
            criteria.Status = showInProgress.Current.Value && criteria.Mode == RoomModeFilter.Open ? null : RoomStatusFilter.Idle;
            return criteria;
        }

        protected override OsuButton CreateNewRoomButton() => new CreateMultiplayerMatchButton();

        protected override Room CreateNewRoom() => new Room
        {
            Name = $"{api.LocalUser}'s awesome room",
            Type = MatchType.HeadToHead,
        };

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new MultiplayerMatchSubScreen(room);

        protected override ListingPollingComponent CreatePollingComponent() => new MultiplayerListingPollingComponent();

        protected override void OpenNewRoom(Room room)
        {
            if (!client.IsConnected.Value)
            {
                Logger.Log("Not currently connected to the multiplayer server.", LoggingTarget.Runtime, LogLevel.Important);
                return;
            }

            base.OpenNewRoom(room);
        }

        private partial class MultiplayerListingPollingComponent : ListingPollingComponent
        {
            [Resolved]
            private MultiplayerClient client { get; set; } = null!;

            private readonly IBindable<bool> isConnected = new Bindable<bool>();

            [BackgroundDependencyLoader]
            private void load()
            {
                isConnected.BindTo(client.IsConnected);
                isConnected.BindValueChanged(_ => Scheduler.AddOnce(poll), true);
            }

            private void poll()
            {
                if (isConnected.Value && IsLoaded)
                    PollImmediately();
            }

            protected override Task Poll()
            {
                if (!isConnected.Value)
                    return Task.CompletedTask;

                if (client.Room != null)
                    return Task.CompletedTask;

                return base.Poll();
            }
        }
    }
}
