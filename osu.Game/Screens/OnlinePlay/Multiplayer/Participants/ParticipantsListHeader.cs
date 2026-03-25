// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantsListHeader : SectionHeader
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        public ParticipantsListHeader()
            : base(RankingsStrings.SpotlightParticipants)
        {
        }

        protected override void Update()
        {
            base.Update();

            var room = client.Room;
            if (room == null)
                return;

            DetailsText.Value = $"{room.Users.Count}";
        }
    }
}
