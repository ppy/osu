// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osu.Framework.Input.Events;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users
{
    public abstract class ExtendedUserPanel : UserPanel
    {
        public readonly Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public readonly IBindable<UserActivity> Activity = new Bindable<UserActivity>();

        protected TextFlowContainer LastVisitMessage { get; private set; }

        private SpriteIcon statusIcon;
        private OsuSpriteText statusMessage;

        protected ExtendedUserPanel(APIUser user)
            : base(user)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            BorderColour = ColourProvider?.Light1 ?? Colours.GreyVioletLighter;

            Status.ValueChanged += status => displayStatus(status.NewValue, Activity.Value);
            Activity.ValueChanged += activity => displayStatus(Status.Value, activity.NewValue);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Status.TriggerChange();

            // Colour should be applied immediately on first load.
            statusIcon.FinishTransforms();
        }

        protected UpdateableAvatar CreateAvatar() => new UpdateableAvatar(User, false);

        protected UpdateableFlag CreateFlag() => new UpdateableFlag(User.Country)
        {
            Size = new Vector2(39, 26)
        };

        protected SpriteIcon CreateStatusIcon() => statusIcon = new SpriteIcon
        {
            Icon = FontAwesome.Regular.Circle,
            Size = new Vector2(25)
        };

        protected FillFlowContainer CreateStatusMessage(bool rightAlignedChildren)
        {
            var statusContainer = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical
            };

            var alignment = rightAlignedChildren ? Anchor.CentreRight : Anchor.CentreLeft;

            statusContainer.Add(LastVisitMessage = new TextFlowContainer(t => t.Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold)).With(text =>
            {
                text.Anchor = alignment;
                text.Origin = alignment;
                text.AutoSizeAxes = Axes.Both;
                text.Alpha = 0;

                if (User.LastVisit.HasValue)
                {
                    text.AddText(@"Last seen ");
                    text.AddText(new DrawableDate(User.LastVisit.Value, italic: false)
                    {
                        Shadow = false
                    });
                }
            }));

            statusContainer.Add(statusMessage = new OsuSpriteText
            {
                Anchor = alignment,
                Origin = alignment,
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold)
            });

            return statusContainer;
        }

        private void displayStatus(UserStatus status, UserActivity activity = null)
        {
            if (status != null)
            {
                LastVisitMessage.FadeTo(status is UserStatusOffline && User.LastVisit.HasValue ? 1 : 0);

                // Set status message based on activity (if we have one) and status is not offline
                if (activity != null && !(status is UserStatusOffline))
                {
                    statusMessage.Text = activity.Status;
                    statusIcon.FadeColour(activity.GetAppropriateColour(Colours), 500, Easing.OutQuint);
                    return;
                }

                // Otherwise use only status
                statusMessage.Text = status.Message;
                statusIcon.FadeColour(status.GetAppropriateColour(Colours), 500, Easing.OutQuint);

                return;
            }

            // Fallback to web status if local one is null
            if (User.IsOnline)
            {
                Status.Value = new UserStatusOnline();
                return;
            }

            Status.Value = new UserStatusOffline();
        }

        protected override bool OnHover(HoverEvent e)
        {
            BorderThickness = 2;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            BorderThickness = 0;
            base.OnHoverLost(e);
        }
    }
}
