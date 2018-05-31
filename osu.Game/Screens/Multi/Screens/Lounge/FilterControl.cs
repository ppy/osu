// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Screens.Lounge
{
    public class FilterControl : SearchableListFilterControl<LoungeTab, LoungeTab>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"362e42");
        protected override LoungeTab DefaultTab => LoungeTab.Public;

        public FilterControl()
        {
            DisplayStyleControl.Hide();
        }
    }

    public enum LoungeTab
    {
        Public = RoomAvailability.Public,
        Private = RoomAvailability.FriendsOnly,
    }
}
