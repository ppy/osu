// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class FilterControl : SearchableListFilterControl<LoungeTab, LoungeTab>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"362e42");
        protected override LoungeTab DefaultTab => LoungeTab.Public;

        public FilterControl()
        {
            DisplayStyleControl.Hide();
        }

        public FilterCriteria CreateCriteria() => new FilterCriteria { Availability = availability };

        private RoomAvailability availability
        {
            get
            {
                switch (Tabs.Current.Value)
                {
                    default:
                    case LoungeTab.Public:
                        return RoomAvailability.Public;
                    case LoungeTab.Private:
                        return RoomAvailability.FriendsOnly;
                }
            }
        }
    }

    public enum LoungeTab
    {
        Create,
        Public,
        Private,
    }
}
