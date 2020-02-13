// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users.Drawables;

namespace osu.Game.Users
{
    public class UserPanel : OsuClickableContainer, IHasContextMenu
    {
        private const float height = 100;
        private const float content_padding = 10;
        private const float status_height = 30;

        public readonly User User;

        [Resolved(canBeNull: true)]
        private OsuColour colours { get; set; }

        private Container statusBar;
        private Box statusBg;
        private OsuSpriteText statusMessage;

        private Container content;
        protected override Container<Drawable> Content => content;

        public readonly Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public readonly IBindable<UserActivity> Activity = new Bindable<UserActivity>();

        public new Action Action;

        protected Action ViewProfile;

        public UserPanel(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            User = user;

            Height = height - status_height;
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(UserProfileOverlay profile)
        {
            if (colours == null)
                throw new InvalidOperationException($"{nameof(colours)} not initialized!");

            FillFlowContainer infoContainer;

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.25f),
                    Radius = 4,
                },

                Children = new Drawable[]
                {
                    new DelayedLoadUnloadWrapper(() => new UserCoverBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        User = User,
                    }, 300, 5000)
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.7f),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Top = content_padding, Horizontal = content_padding },
                        Children = new Drawable[]
                        {
                            new UpdateableAvatar
                            {
                                Size = new Vector2(height - status_height - content_padding * 2),
                                User = User,
                                Masking = true,
                                CornerRadius = 5,
                                OpenOnClick = { Value = false },
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Type = EdgeEffectType.Shadow,
                                    Colour = Color4.Black.Opacity(0.25f),
                                    Radius = 4,
                                },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Left = height - status_height - content_padding },
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Text = User.Username,
                                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 18, italics: true),
                                    },
                                    infoContainer = new FillFlowContainer
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        AutoSizeAxes = Axes.X,
                                        Height = 20f,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(5f, 0f),
                                        Children = new Drawable[]
                                        {
                                            new UpdateableFlag(User.Country)
                                            {
                                                Width = 30f,
                                                RelativeSizeAxes = Axes.Y,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                    statusBar = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Alpha = 0f,
                        Children = new Drawable[]
                        {
                            statusBg = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.5f,
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Spacing = new Vector2(5f, 0f),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Icon = FontAwesome.Regular.Circle,
                                        Shadow = true,
                                        Size = new Vector2(14),
                                    },
                                    statusMessage = new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                    },
                                },
                            },
                        },
                    },
                }
            });

            if (User.IsSupporter)
            {
                infoContainer.Add(new SupporterIcon
                {
                    Height = 20f,
                    SupportLevel = User.SupportLevel
                });
            }

            Status.ValueChanged += status => displayStatus(status.NewValue, Activity.Value);
            Activity.ValueChanged += activity => displayStatus(Status.Value, activity.NewValue);

            base.Action = ViewProfile = () =>
            {
                Action?.Invoke();
                profile?.ShowUser(User);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Status.TriggerChange();
        }

        private void displayStatus(UserStatus status, UserActivity activity = null)
        {
            const float transition_duration = 500;

            if (status == null)
            {
                statusBar.ResizeHeightTo(0f, transition_duration, Easing.OutQuint);
                statusBar.FadeOut(transition_duration, Easing.OutQuint);
                this.ResizeHeightTo(height - status_height, transition_duration, Easing.OutQuint);
            }
            else
            {
                statusBar.ResizeHeightTo(status_height, transition_duration, Easing.OutQuint);
                statusBar.FadeIn(transition_duration, Easing.OutQuint);
                this.ResizeHeightTo(height, transition_duration, Easing.OutQuint);
            }

            if (status is UserStatusOnline && activity != null)
            {
                statusMessage.Text = activity.Status;
                statusBg.FadeColour(activity.GetAppropriateColour(colours), 500, Easing.OutQuint);
            }
            else
            {
                statusMessage.Text = status?.Message;
                statusBg.FadeColour(status?.GetAppropriateColour(colours) ?? colours.Gray5, 500, Easing.OutQuint);
            }
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("View Profile", MenuItemType.Highlighted, ViewProfile),
        };
    }
}
