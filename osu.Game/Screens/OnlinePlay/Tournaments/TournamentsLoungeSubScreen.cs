// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class TournamentsLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Dropdown<TournamentsCategory> categoryDropdown = null!;

        protected override IEnumerable<Drawable> CreateFilterControls()
        {
            categoryDropdown = new SlimEnumDropdown<TournamentsCategory>
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
                case TournamentsCategory.Community:
                    criteria.Category = @"community";
                    break;

                case TournamentsCategory.Certified:
                    criteria.Category = @"certified";
                    break;

                case TournamentsCategory.Official:
                    criteria.Category = @"official";
                    break;
            }

            return criteria;
        }

        protected override OsuButton CreateNewRoomButton() => new CreateTournamentsRoomButton();

        protected override Room CreateNewRoom()
        {
            return new Room
            {
                Name = $"{api.LocalUser}'s awesome tournament",
                Type = MatchType.Tournaments
            };
        }

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new TournamentsRoomSubScreen(room);

        // TODO: Probably needs Tournaments implementation
        protected override ListingPollingComponent CreatePollingComponent() => new ListingPollingComponent();

        private enum TournamentsCategory
        {
            Any,
            Community,
            Certified,
            Official,
        }
    }
}
