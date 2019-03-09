// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header
{
    public class ProfileMessageButton : ProfileHeaderButton
    {
        public readonly Bindable<User> User = new Bindable<User>();

        [Resolved(CanBeNull = true)]
        private ChannelManager channelManager { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay userOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved]
        private APIAccess apiAccess { get; set; }

        public ProfileMessageButton()
        {
            Content.Alpha = 0;
            RelativeSizeAxes = Axes.Y;

            Child = new SpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = FontAwesome.fa_envelope,
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

            User.ValueChanged += e => Content.Alpha = !e.NewValue.PMFriendsOnly && apiAccess.LocalUser.Value.Id != e.NewValue.Id ? 1 : 0;
        }
    }
}
