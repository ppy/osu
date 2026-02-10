// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class UserActionsButton : ProfileActionsButton
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private UserReportPopoverTarget reportPopoverTarget = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            // This is a bit of a dirty hack. Because `ReportUserPopover` is spawned from `UserActionsPopover`,
            // and that they both share the same `PopoverContainer`, the former will get destroyed when the latter
            // is opened, causing it to get destroyed as well.
            //
            // This is worked around by having an additional dummy popover target on the actions button,
            // which is then passed to `UserActionsPopover` and the user report action. This way the popover
            // can remain attached to it once the actions popover is destroyed.
            reportPopoverTarget = new UserReportPopoverTarget
            {
                RelativeSizeAxes = Axes.Both
            };
            Add(reportPopoverTarget);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(_ =>
            {
                Alpha = User.Value?.User.OnlineID == api.LocalUser.Value.OnlineID ? 0 : 1;
                reportPopoverTarget.User = User.Value?.User;
            }, true);
        }

        public override Popover GetPopover() => new UserActionPopover(User.Value!.User, reportPopoverTarget);

        private partial class UserActionPopover : ProfileActionPopover
        {
            private readonly APIUser user;

            private readonly IHasPopover reportPopoverTarget;

            public UserActionPopover(APIUser user, IHasPopover reportPopoverTarget)
            {
                this.user = user;
                this.reportPopoverTarget = reportPopoverTarget;
            }

            [BackgroundDependencyLoader]
            private void load(IAPIProvider api, IDialogOverlay? dialogOverlay)
            {
                bool userBlocked = api.LocalUserState.Blocks.Any(b => b.TargetID == user.Id);

                Actions = new[]
                {
                    new ProfilePopoverAction(FontAwesome.Solid.Ban, userBlocked ? UsersStrings.BlocksButtonUnblock : UsersStrings.BlocksButtonBlock)
                    {
                        Action = () =>
                        {
                            dialogOverlay?.Push(userBlocked ? ConfirmBlockActionDialog.Unblock(user) : ConfirmBlockActionDialog.Block(user));
                            this.HidePopover();
                        }
                    },
                    new ProfilePopoverAction(FontAwesome.Solid.ExclamationTriangle, ReportStrings.UserButton)
                    {
                        Action = () =>
                        {
                            this.HidePopover();
                            reportPopoverTarget.ShowPopover();
                        }
                    },
                };
            }
        }

        private partial class UserReportPopoverTarget : Container, IHasPopover
        {
            public APIUser? User;

            public Popover? GetPopover() => User != null ? new ReportUserPopover(User) : null;
        }
    }
}
