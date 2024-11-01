// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Notifications;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class FollowersButton : ProfileHeaderStatisticsButton
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        public override LocalisableString TooltipText => FriendsStrings.ButtonsDisabled;

        protected override IconUsage Icon => FontAwesome.Solid.User;

        private readonly IBindableList<APIRelation> apiFriends = new BindableList<APIRelation>();
        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        private readonly Bindable<FriendStatus> status = new Bindable<FriendStatus>();

        [Resolved]
        private OsuColour colour { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, INotificationOverlay? notifications)
        {
            localUser.BindTo(api.LocalUser);

            status.BindValueChanged(_ =>
            {
                updateIcon();
                updateColor();
            });

            User.BindValueChanged(_ => updateStatus(), true);

            apiFriends.BindTo(api.Friends);
            apiFriends.BindCollectionChanged((_, _) => Schedule(updateStatus));

            Action += () =>
            {
                if (User.Value == null)
                    return;

                if (status.Value == FriendStatus.Self)
                    return;

                ShowLodingLayer();

                APIRequest req = status.Value == FriendStatus.None ? new FriendAddRequest(User.Value.User.OnlineID) : new FriendDeleteRequest(User.Value.User.OnlineID);

                req.Success += () =>
                {
                    api.UpdateLocalFriends();
                    HideLodingLayer();
                };

                req.Failure += e =>
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = e.Message,
                        Icon = FontAwesome.Solid.Times,
                    });

                    HideLodingLayer();
                };

                api.Queue(req);
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (status.Value > FriendStatus.None)
            {
                SetIcon(FontAwesome.Solid.UserTimes);
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            updateIcon();
        }

        private void updateStatus()
        {
            SetValue(User.Value?.User.FollowerCount ?? 0);

            if (localUser.Value.OnlineID == User.Value?.User.OnlineID)
            {
                status.Value = FriendStatus.Self;
                return;
            }

            var friend = apiFriends.FirstOrDefault(u => User.Value?.User.OnlineID == u.TargetID);

            if (friend != null)
            {
                status.Value = friend.Mutual ? FriendStatus.Mutual : FriendStatus.NotMutual;
            }
            else
            {
                status.Value = FriendStatus.None;
            }
        }

        private void updateIcon()
        {
            switch (status.Value)
            {
                case FriendStatus.Self:
                    SetIcon(FontAwesome.Solid.User);
                    break;

                case FriendStatus.None:
                    SetIcon(FontAwesome.Solid.UserPlus);
                    break;

                case FriendStatus.NotMutual:
                    SetIcon(FontAwesome.Solid.User);
                    break;

                case FriendStatus.Mutual:
                    SetIcon(FontAwesome.Solid.UserFriends);
                    break;
            }
        }

        private void updateColor()
        {
            switch (status.Value)
            {
                case FriendStatus.Self:
                case FriendStatus.None:
                    IdleColour = colourProvider.Background6;
                    HoverColour = colourProvider.Background5;
                    SetBackGroundColour(colourProvider.Background6, 200);
                    break;

                case FriendStatus.NotMutual:
                    IdleColour = colour.Green;
                    HoverColour = colour.Green.Lighten(0.1f);
                    SetBackGroundColour(colour.Green, 200);
                    break;

                case FriendStatus.Mutual:
                    IdleColour = colour.Pink;
                    HoverColour = colour.Pink1.Lighten(0.1f);
                    SetBackGroundColour(colour.Pink, 200);
                    break;
            }
        }

        private enum FriendStatus
        {
            Self,
            None,
            NotMutual,
            Mutual,
        }
    }
}
