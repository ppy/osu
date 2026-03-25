// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
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

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(_ => Alpha = User.Value?.User.OnlineID == api.LocalUser.Value.OnlineID ? 0 : 1, true);
        }

        public override Popover GetPopover() => new UserActionPopover(User.Value!.User);

        private partial class UserActionPopover : ProfileActionPopover
        {
            private readonly APIUser user;

            public UserActionPopover(APIUser user)
            {
                this.user = user;
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
                    }
                };
            }
        }
    }
}
