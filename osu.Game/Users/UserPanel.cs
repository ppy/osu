// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
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
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Users
{
    public class UserPanel : ClickableContainer, IHasContextMenu
    {
        private readonly User user;
        private const float height = 100;
        private const float content_padding = 10;
        private const float status_height = 30;

        private readonly Container statusBar;
        private readonly Box statusBg;
        private readonly OsuSpriteText statusMessage;

        public readonly Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public new Action Action;

        protected Action ViewProfile;

        public UserPanel(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            this.user = user;

            FillFlowContainer infoContainer;

            Height = height - status_height;
            Masking = true;
            CornerRadius = 5;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Radius = 4,
            };

            Children = new Drawable[]
            {
                new AsyncLoadWrapper(new UserCoverBackground(user)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    OnLoadComplete = d => d.FadeInFromZero(200),
                }) { RelativeSizeAxes = Axes.Both },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.7f),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Top = content_padding, Left = content_padding, Right = content_padding },
                    Children = new Drawable[]
                    {
                        new UpdateableAvatar
                        {
                            Size = new Vector2(height - status_height - content_padding * 2),
                            User = user,
                            Masking = true,
                            CornerRadius = 5,
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
                                    Text = user.Username,
                                    TextSize = 18,
                                    Font = @"Exo2.0-SemiBoldItalic",
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
                                        new DrawableFlag(user.Country?.FlagName)
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
                                    Icon = FontAwesome.fa_circle_o,
                                    Shadow = true,
                                    Size = new Vector2(14),
                                },
                                statusMessage = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = @"Exo2.0-Semibold",
                                },
                            },
                        },
                    },
                },
            };

            if (user.IsSupporter)
                infoContainer.Add(new SupporterIcon
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 20f,
                });
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, UserProfileOverlay profile)
        {
            if (colours == null)
                throw new ArgumentNullException(nameof(colours));

            Status.ValueChanged += displayStatus;
            Status.ValueChanged += status => statusBg.FadeColour(status?.GetAppropriateColour(colours) ?? colours.Gray5, 500, Easing.OutQuint);

            base.Action = ViewProfile = () =>
            {
                Action?.Invoke();
                profile?.ShowUser(user);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Status.TriggerChange();
        }

        private void displayStatus(UserStatus status)
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

                statusMessage.Text = status.Message;
            }
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("View Profile", MenuItemType.Highlighted, ViewProfile),
        };

        private class SupporterIcon : CircularContainer
        {
            private readonly Box background;

            public SupporterIcon()
            {
                Masking = true;
                Children = new Drawable[]
                {
                    new Box { RelativeSizeAxes = Axes.Both },
                    new CircularContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Scale = new Vector2(0.8f),
                        Masking = true,
                        Children = new Drawable[]
                        {
                            background = new Box { RelativeSizeAxes = Axes.Both },
                            new Triangles
                            {
                                TriangleScale = 0.2f,
                                ColourLight = OsuColour.FromHex(@"ff7db7"),
                                ColourDark = OsuColour.FromHex(@"de5b95"),
                                RelativeSizeAxes = Axes.Both,
                                Velocity = 0.3f,
                            },
                        }
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Icon = FontAwesome.fa_heart,
                        Scale = new Vector2(0.45f),
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Pink;
            }
        }
    }
}
