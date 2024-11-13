// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Dropdown<PlaylistsCategory> categoryDropdown = null!;

        protected override IEnumerable<Drawable> CreateFilterControls()
        {
            categoryDropdown = new SlimEnumDropdown<PlaylistsCategory>
            {
                RelativeSizeAxes = Axes.None,
                Width = 160,
            };

            categoryDropdown.Current.BindValueChanged(_ => UpdateFilter());

            return base.CreateFilterControls().Append(categoryDropdown);
        }

        protected override FilterCriteria CreateFilterCriteria()
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

        protected override OsuButton CreateNewRoomButton() => new CreatePlaylistsRoomButton();

        protected override Room CreateNewRoom()
        {
            return new Room
            {
                Name = $"{api.LocalUser}'s awesome playlist",
                Type = MatchType.Playlists
            };
        }

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new PlaylistsRoomSubScreen(room);

        protected override ListingPollingComponent CreatePollingComponent() => new ListingPollingComponent();

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
