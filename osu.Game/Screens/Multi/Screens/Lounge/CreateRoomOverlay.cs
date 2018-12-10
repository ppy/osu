// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Screens.Lounge
{
    public class CreateRoomOverlay : RoomSettingsOverlay
    {
        [Resolved]
        private APIAccess api { get; set; }

        public CreateRoomOverlay()
        {
            MaxParticipantsField.ReadOnly = true;
            AvailabilityPicker.ReadOnly.Value = true;
            TypePicker.ReadOnly.Value = true;
            PasswordField.ReadOnly = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Room.Host.Value = api.LocalUser;
        }

        public override Room Room
        {
            get => base.Room;
            set
            {
                base.Room = value;

                if (api != null && value != null)
                    value.Host.Value = api.LocalUser;
            }
        }
    }
}
