// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private FormEnumDropdown<PlaylistsCategory> categoryDropdown = null!;

        protected override IEnumerable<Drawable> CreateFilterControls()
        {
            foreach (var control in base.CreateFilterControls())
                yield return control;

            yield return new Container
            {
                Width = 160,
                AutoSizeAxes = Axes.Y,
                Child = categoryDropdown = new FormEnumDropdown<PlaylistsCategory>
                {
                    Caption = LoungeSubScreenStrings.PlaylistFilterCategory,
                }
            };

            categoryDropdown.Current.BindValueChanged(_ => UpdateFilter());
        }

        protected override LoungeFilterCriteria CreateFilterCriteria()
        {
            var criteria = base.CreateFilterCriteria();

            switch (categoryDropdown.Current.Value)
            {
                case PlaylistsCategory.Normal:
                    criteria.Category = @"normal";
                    break;

                case PlaylistsCategory.Spotlight:
                    criteria.Category = @"spotlight";
                    break;

                case PlaylistsCategory.FeaturedArtist:
                    criteria.Category = @"featured_artist";
                    break;
            }

            return criteria;
        }

        protected override void JoinInternal(Room room, string? password, Action<Room> onSuccess, Action<string, Exception?> onFailure)
        {
            var joinRoomRequest = new JoinRoomRequest(room, password);

            joinRoomRequest.Success += r => onSuccess(r);
            joinRoomRequest.Failure += exception =>
            {
                if (exception is not OperationCanceledException)
                    onFailure($"Failed to open playlist. {exception.Message}", exception);
            };

            api.Queue(joinRoomRequest);
        }

        public override void Close(Room room)
        {
            Debug.Assert(room.RoomID != null);

            var request = new ClosePlaylistRequest(room.RoomID.Value);
            request.Success += RefreshRooms;
            api.Queue(request);
        }

        protected override OsuButton CreateNewRoomButton() => new CreatePlaylistsRoomButton();

        protected override Room CreateNewRoom()
        {
            return new Room
            {
                Name = $"{api.LocalUser}'s awesome playlist",
                Type = MatchType.Playlists
            };
        }

        protected override OnlinePlaySubScreen CreateRoomSubScreen(Room room) => new PlaylistsRoomSubScreen(room);

        private enum PlaylistsCategory
        {
            Any,
            Normal,
            Spotlight,

            [Description("Featured Artist")]
            FeaturedArtist,
        }
    }
}
