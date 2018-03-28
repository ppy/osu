// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multiplayer
{
    public class LoungeFilterControl : SearchableListFilterControl<RoomAvailability, RoomAvailability>
    {
        // todo: special lounge filter tabs instead of RoomAvailability

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"362e42");
        protected override RoomAvailability DefaultTab => RoomAvailability.Public;

        public LoungeFilterControl()
        {
            DisplayStyleControl.Hide();
        }
    }
}
