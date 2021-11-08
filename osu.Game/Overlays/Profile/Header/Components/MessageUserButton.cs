// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class MessageUserButton : ProfileHeaderButton
    {
        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        public override LocalisableString TooltipText => UsersStrings.CardSendMessage;

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay userOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved]
        private IAPIProvider apiProvider { get; set; }

        public MessageUserButton()
        {
            Content.Alpha = 0;

            Child = new SpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = FontAwesome.Solid.Envelope,
                FillMode = FillMode.Fit,
                Size = new Vector2(50, 14)
            };

            Action = () =>
            {
                if (!Content.IsPresent) return;

                channelManager?.OpenPrivateChannel(User.Value);
                userOverlay?.Hide();
                chatOverlay?.Show();
            };

            User.ValueChanged += e => Content.Alpha = !e.NewValue.PMFriendsOnly && apiProvider.LocalUser.Value.Id != e.NewValue.Id ? 1 : 0;
        }
    }
}
