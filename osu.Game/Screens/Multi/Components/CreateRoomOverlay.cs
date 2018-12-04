// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Online.API;

namespace osu.Game.Screens.Multi.Components
{
    public class CreateRoomOverlay : RoomSettingsOverlay
    {
        [Resolved]
        private APIAccess api { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Room.Host.Value = api.LocalUser;
        }
    }
}
