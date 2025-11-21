// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class UserActionsButton : OsuHoverContainer, IHasPopover
    {
        public readonly Bindable<UserProfileData?> User = new Bindable<UserProfileData?>();

        private Box background = null!;

        protected override IEnumerable<Drawable> EffectTargets => [background];

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background1;

            Size = new Vector2(40);
            Masking = true;
            CornerRadius = 20;

            Child = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new SpriteIcon
                    {
                        Size = new Vector2(12),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.EllipsisV,
                    },
                }
            };

            Action = this.ShowPopover;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.BindValueChanged(_ => Alpha = User.Value?.User.OnlineID == api.LocalUser.Value.OnlineID ? 0 : 1, true);
        }

        public Popover GetPopover() => new UserActionPopover(User.Value!.User);

        private partial class UserActionPopover : OsuPopover
        {
            private readonly APIUser user;

            public UserActionPopover(APIUser user)
                : base(false)
            {
                this.user = user;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider, IAPIProvider api, IDialogOverlay? dialogOverlay)
            {
                Background.Colour = colourProvider.Background6;

                bool userBlocked = api.LocalUserState.Blocks.Any(b => b.TargetID == user.Id);

                AllowableAnchors = [Anchor.BottomCentre, Anchor.TopCentre];

                Child = new FillFlowContainer
                {
                    Width = 160,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
                    Children = new Drawable[]
                    {
                        new UserAction(FontAwesome.Solid.Ban, userBlocked ? UsersStrings.BlocksButtonUnblock : UsersStrings.BlocksButtonBlock)
                        {
                            Action = () =>
                            {
                                dialogOverlay?.Push(userBlocked ? ConfirmBlockActionDialog.Unblock(user) : ConfirmBlockActionDialog.Block(user));
                                this.HidePopover();
                            }
                        }
                    }
                };
            }
        }

        private partial class UserAction : OsuClickableContainer
        {
            private readonly IconUsage icon;
            private readonly LocalisableString caption;

            private Box background = null!;
            private CircularContainer indicator = null!;

            public UserAction(IconUsage icon, LocalisableString caption)
            {
                this.icon = icon;
                this.caption = caption;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Content.RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Content.AutoSizeAxes = Axes.Y;

                Masking = true;
                CornerRadius = 4;
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                        Alpha = 0,
                    },
                    indicator = new Circle
                    {
                        Width = 4,
                        Height = 14,
                        X = 10,
                        Colour = colourProvider.Highlight1,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.Centre,
                        Alpha = 0,
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Padding = new MarginPadding { Horizontal = 25, Vertical = 5 },
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5, 0),
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Icon = icon,
                                Size = new Vector2(11),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            new OsuSpriteText
                            {
                                Text = caption,
                                Font = OsuFont.Style.Body,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                UseFullGlyphHeight = false,
                            }
                        }
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            private void updateState()
            {
                background.Alpha = indicator.Alpha = IsHovered ? 1 : 0;
            }
        }
    }
}
