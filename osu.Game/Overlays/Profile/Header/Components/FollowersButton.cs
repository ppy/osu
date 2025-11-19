// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
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

        // Because it is impossible to update the number of friends after the operation,
        // the number of friends obtained is stored and modified locally.
        private int followerCount;

        public override LocalisableString TooltipText
        {
            get
            {
                switch (status.Value)
                {
                    case FriendStatus.Self:
                        return FriendsStrings.ButtonsDisabled;

                    case FriendStatus.None:
                        return FriendsStrings.ButtonsAdd;

                    case FriendStatus.NotMutual:
                    case FriendStatus.Mutual:
                        return FriendsStrings.ButtonsRemove;
                }

                return FriendsStrings.TitleCompact;
            }
        }

        protected override IconUsage Icon => FontAwesome.Solid.User;

        private readonly IBindableList<APIRelation> apiFriends = new BindableList<APIRelation>();
        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();

        private readonly Bindable<FriendStatus> status = new Bindable<FriendStatus>();

        [Resolved]
        private OsuColour colour { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay? notifications)
        {
            localUser.BindTo(api.LocalUser);

            status.BindValueChanged(_ =>
            {
                updateIcon();
                updateColor();
            });

            Action += () =>
            {
                if (User.Value == null)
                    return;

                if (status.Value == FriendStatus.Self)
                    return;

                ShowLoadingLayer();

                APIRequest req = status.Value == FriendStatus.None ? new AddFriendRequest(User.Value.User.OnlineID) : new DeleteFriendRequest(User.Value.User.OnlineID);

                req.Success += () =>
                {
                    if (req is AddFriendRequest addedRequest)
                    {
                        SetValue(++followerCount);
                        status.Value = addedRequest.Response?.UserRelation.Mutual == true ? FriendStatus.Mutual : FriendStatus.NotMutual;
                    }
                    else
                    {
                        SetValue(--followerCount);
                        status.Value = FriendStatus.None;
                    }

                    api.LocalUserState.UpdateFriends();
                    HideLoadingLayer();
                };

                req.Failure += e =>
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = e.Message,
                        Icon = FontAwesome.Solid.Times,
                    });

                    HideLoadingLayer();
                };

                api.Queue(req);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            apiFriends.BindTo(api.LocalUserState.Friends);
            apiFriends.BindCollectionChanged((_, _) => Schedule(updateStatus));

            User.BindValueChanged(u =>
            {
                followerCount = u.NewValue?.User.FollowerCount ?? 0;
                updateStatus();
            }, true);
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
            SetValue(followerCount);

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
            // https://github.com/ppy/osu-web/blob/0a5367a4a68a6cdf450eb483251b3cf03b3ac7d2/resources/css/bem/user-action-button.less

            switch (status.Value)
            {
                case FriendStatus.Self:
                case FriendStatus.None:
                    IdleColour = colourProvider.Background6;
                    HoverColour = colourProvider.Background5;
                    break;

                case FriendStatus.NotMutual:
                    IdleColour = colour.Green.Opacity(0.7f);
                    HoverColour = IdleColour.Value.Lighten(0.1f);
                    break;

                case FriendStatus.Mutual:
                    IdleColour = colour.Pink.Opacity(0.7f);
                    HoverColour = IdleColour.Value.Lighten(0.1f);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            EffectTargets.ForEach(d => d.FadeColour(IsHovered ? HoverColour.Value : IdleColour.Value, FADE_DURATION, Easing.OutQuint));
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
