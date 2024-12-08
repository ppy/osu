// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class ExternalRoomButton : ExternalLinkButton
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly IBindable<long?> roomID = new Bindable<long?>();

        public ExternalRoomButton(IBindable<long?> roomID)
        {
            this.roomID.BindTo(roomID);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roomID.BindValueChanged(id =>
            {
                if (roomID.Value.HasValue)
                    Link = formatLink(roomID.Value.Value);
                else Link = null;
            }, true);
        }

        private string formatLink(long id)
        {
            return $@"{api.WebsiteRootUrl}/multiplayer/rooms/{id}";
        }
    }
}
