// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Logging;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

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

        protected override OnlinePlaySubScreen CreateRoomSubScreen(Room room) => new MultiplayerMatchSubScreen(room);

        protected override void JoinInternal(Room room, string? password, Action<Room> onSuccess, Action<string> onFailure)
        {
            client.JoinRoom(room, password).ContinueWith(result =>
            {
                if (result.IsCompletedSuccessfully)
                    onSuccess(room);
                else
                {
                    Exception? exception = result.Exception?.AsSingular();

                    if (exception?.GetHubExceptionMessage() is string message)
                        onFailure(message);
                    else
                    {
                        const string generic_failure_message = "Failed to join multiplayer room.";
                        if (result.Exception != null)
                            Logger.Error(result.Exception, generic_failure_message);
                        onFailure(generic_failure_message);
                    }
                }
            });
        }

        public override void Close(Room room)
            => throw new NotSupportedException("Cannot close multiplayer rooms.");

        protected override void OpenNewRoom(Room room)
        {
            if (!client.IsConnected.Value)
            {
                Logger.Log("Not currently connected to the multiplayer server.", LoggingTarget.Runtime, LogLevel.Important);
                return;
            }

            base.OpenNewRoom(room);
        }
    }
}
