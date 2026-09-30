// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Logging;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private Dropdown<RoomPermissionsFilter> roomAccessTypeDropdown = null!;
        private FormCheckBox showInProgress = null!;
        private FormCheckBox showFull = null!;

        protected override IEnumerable<Drawable> CreateFilterControls()
        {
            foreach (var control in base.CreateFilterControls())
                yield return control;

            yield return new Container
            {
                Width = 160,
                AutoSizeAxes = Axes.Y,
                Child = roomAccessTypeDropdown = new FormEnumDropdown<RoomPermissionsFilter>
                {
                    Caption = LoungeSubScreenStrings.RoomFilterPrivacySetting,
                    Current = Config.GetBindable<RoomPermissionsFilter>(OsuSetting.MultiplayerRoomFilter),
                },
            };

            roomAccessTypeDropdown.Current.BindValueChanged(_ => UpdateFilter());

            yield return new Container
            {
                Width = 240,
                Child = showInProgress = new FormCheckBox
                {
                    ExtendedHeight = true,
                    HintText = LoungeSubScreenStrings.RoomFilterInProgressDescription,
                    Caption = LoungeSubScreenStrings.RoomFilterInProgress,
                    Current = Config.GetBindable<bool>(OsuSetting.MultiplayerShowInProgressFilter),
                }
            };

            yield return new Container
            {
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Child = showFull = new FormCheckBox
                {
                    ExtendedHeight = true,
                    HintText = LoungeSubScreenStrings.RoomFilterFullRoomsDescription,
                    Caption = LoungeSubScreenStrings.RoomFilterFullRooms,
                    Current = Config.GetBindable<bool>(OsuSetting.MultiplayerShowFullFilter),
                }
            };

            showInProgress.Current.BindValueChanged(_ => UpdateFilter());
            showFull.Current.BindValueChanged(_ => UpdateFilter());
            StatusDropdown.Current.BindValueChanged(_ =>
            {
                showFull.Alpha = showInProgress.Alpha = StatusDropdown.Current.Value == RoomModeFilter.Open ? 1 : 0;
            }, true);
        }

        protected override LoungeFilterCriteria CreateFilterCriteria()
        {
            var criteria = base.CreateFilterCriteria();
            criteria.Category = @"realtime";
            criteria.Permissions = roomAccessTypeDropdown.Current.Value;
            criteria.Full = showFull.Current.Value;
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

        protected override void JoinInternal(Room room, string? password, Action<Room> onSuccess, Action<string, Exception?> onFailure)
        {
            client.JoinRoom(room, password).ContinueWith(result =>
            {
                if (result.IsCompletedSuccessfully)
                    onSuccess(room);
                else
                {
                    Exception? exception = result.Exception?.AsSingular();

                    if (exception?.GetHubExceptionMessage() is string message)
                        onFailure(message, exception);
                    else
                        onFailure($"Failed to join multiplayer room. {exception?.Message}", exception);
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
