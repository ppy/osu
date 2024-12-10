// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RoomLinkButton : ExternalLinkButton
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly long? roomID;

        public RoomLinkButton(long? roomID)
        {
            this.roomID = roomID;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (roomID.HasValue)
                Link = formatLink(roomID.Value);
        }

        private string formatLink(long id)
        {
            return $@"{api.WebsiteRootUrl}/multiplayer/rooms/{id}";
        }
    }
}
