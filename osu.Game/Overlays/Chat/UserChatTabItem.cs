// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Screens.Menu;
using osu.Game.Users;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class UserChatTabItem : TabItem<UserChat>
    {
        private static readonly Vector2 shear = new Vector2(1f / 5f, 0);

        public override bool IsRemovable => true;

        private readonly Box highlightBox;
        private readonly Container backgroundContainer;
        private readonly Box backgroundBox;
        private readonly OsuSpriteText username;
        private readonly ChatTabItemCloseButton closeButton;
       
        public UserChatTabItem(UserChat value)
            : base(value)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Origin = Anchor.BottomRight;
            Anchor = Anchor.BottomRight;
            EdgeEffect = deactivateEdgeEffect;
            Masking = false;
            Shear = shear;

            Children = new Drawable[]
            {
                new Container()
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        backgroundBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.BottomLeft,
                            Anchor = Anchor.BottomLeft,
                            EdgeSmoothness = new Vector2(1, 0),
                        },
                    }
                },
                highlightBox = new Box
                {
                    Width = 5,
                    BypassAutoSizeAxes = Axes.X,
                    Alpha = 0,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    EdgeSmoothness = new Vector2(1, 0),
                    RelativeSizeAxes = Axes.Y,
                    Colour = new OsuColour().Yellow
                },
                new Container
                {
                    Masking = true,
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Child = new FlowContainerWithOrigin
                    {
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        X = -5,
                        Direction = FillDirection.Horizontal,
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Shear = -shear,
                        
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Margin = new MarginPadding
                                {
                                    Horizontal = 5
                                },
                                Origin = Anchor.BottomLeft,
                                Anchor = Anchor.BottomLeft,
                                Children = new Drawable[]
                                {

                                    new SpriteIcon
                                    {
                                        Icon = FontAwesome.fa_eercast,
                                        Origin = Anchor.Centre,
                                        Scale = new Vector2(1.2f),
                                        X = -5,
                                        Y = 5,
                                        Anchor = Anchor.Centre,
                                        Colour = new OsuColour().BlueDarker,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new CircularContainer
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        Scale = new Vector2(0.95f),
                                        AutoSizeAxes = Axes.X,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Masking = true,
                                        Child = new Avatar(value.User)
                                        {
                                            Size = new Vector2(ChatOverlay.TAB_AREA_HEIGHT),
                                        }
                                    },
                                }
                            },
                            username = new OsuSpriteText
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Text = value.User.Username,
                                Margin = new MarginPadding(1),
                                TextSize = 18,
                            },
                            closeButton = new ChatTabItemCloseButton
                            {
                                Height = 1,
                                Origin = Anchor.BottomLeft,
                                Anchor = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.Y,

                                Action = delegate
                                {
                                    if (IsRemovable) OnRequestClose?.Invoke(this);
                                },
                            },
                    }
                    }
                }
            };
        }

        public Action<UserChatTabItem> OnRequestClose;

        private readonly EdgeEffectParameters activateEdgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 30,
            Colour = Color4.Black.Opacity(0.3f),
        };

        protected override void OnActivated()
        {
            const int activate_length = 1000;

            backgroundBox.ResizeHeightTo(1.1f, activate_length, Easing.OutQuint);
            highlightBox.ResizeHeightTo(1.1f, activate_length, Easing.OutQuint);
            highlightBox.FadeIn(activate_length, Easing.OutQuint);
            username.FadeIn(activate_length, Easing.OutQuint);
            username.ScaleTo(new Vector2(1, 1), activate_length, Easing.OutQuint);
            closeButton.ScaleTo(new Vector2(1, 1), activate_length, Easing.OutQuint);
            closeButton.FadeIn(activate_length, Easing.OutQuint);
            TweenEdgeEffectTo(activateEdgeEffect, activate_length);
        }

        private readonly EdgeEffectParameters deactivateEdgeEffect = new EdgeEffectParameters
        {
            Colour = Color4.Black.Opacity(0.0f),
        };

        protected override void OnDeactivated()
        {
            const int deactivate_length = 500;

            backgroundBox.ResizeHeightTo(1, deactivate_length, Easing.OutQuint);
            highlightBox.ResizeHeightTo(1, deactivate_length, Easing.OutQuint);
            highlightBox.FadeOut(deactivate_length, Easing.OutQuint);
            username.FadeOut(deactivate_length, Easing.OutQuint);
            username.ScaleTo(new Vector2(0, 1), deactivate_length, Easing.OutQuint);
            closeButton.FadeOut(deactivate_length, Easing.OutQuint);
            closeButton.ScaleTo(new Vector2(0, 1), deactivate_length, Easing.OutQuint);
            TweenEdgeEffectTo(deactivateEdgeEffect, deactivate_length);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            backgroundBox.Colour = Value.User.Colour != null ? OsuColour.FromHex(Value.User.Colour) : colours.BlueDark;
        }
    }
}
