// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using System;

namespace osu.Game.Overlays.Chat
{
    public class ChatTabControl : OsuTabControl<Channel>
    {
        private const float shear_width = 10;

        public Action<Channel> OnRequestLeave;

        public readonly Bindable<bool> ChannelSelectorActive = new Bindable<bool>();

        private readonly ChannelTabItem.ChannelSelectorTabItem selectorTab;

        public ChatTabControl()
        {
            TabContainer.Margin = new MarginPadding { Left = 50 };
            TabContainer.Spacing = new Vector2(-shear_width, 0);
            TabContainer.Masking = false;

            AddInternal(new SpriteIcon
            {
                Icon = FontAwesome.fa_comments,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Size = new Vector2(20),
                Margin = new MarginPadding(10),
            });

            AddTabItem(selectorTab = new ChannelTabItem.ChannelSelectorTabItem(new Channel { Name = "+" }));

            ChannelSelectorActive.BindTo(selectorTab.Active);
        }

        protected override TabItem<Channel> CreateTabItem(Channel value)
        {
             ChannelTabItem tab = new ChannelTabItem(value);
             tab.OnRequestClose = () => OnRequestLeave?.Invoke(value);

             return tab;
        }

        protected override void SelectTab(TabItem<Channel> tab)
        {
            if (tab is ChannelTabItem.ChannelSelectorTabItem)
            {
                tab.Active.Toggle();
                return;
            }

            selectorTab.Active.Value = false;

            base.SelectTab(tab);
        }

        private class ChannelTabItem : TabItem<Channel>
        {
            private Color4 backgroundInactive;
            private Color4 backgroundHover;
            private Color4 backgroundActive;

            protected override bool IsRemovable => !Pinned;

            private readonly SpriteText text;
            private readonly SpriteText textBold;
            private readonly Button closeButton;
            private readonly Box box;
            private readonly Box highlightBox;
            private readonly SpriteIcon icon;

            public Action OnRequestClose;

            private void updateState()
            {
                if (Active)
                    fadeActive();
                else
                    fadeInactive();
            }

            private const float transition_length = 400;

            private void fadeActive()
            {
                this.ResizeTo(new Vector2(Width, 1.1f), transition_length, Easing.OutQuint);

                closeButton?.MoveToX(-5f, 0.5f, Easing.OutElastic);
                box.FadeColour(backgroundActive, transition_length, Easing.OutQuint);
                highlightBox.FadeIn(transition_length, Easing.OutQuint);

                text.FadeOut(transition_length, Easing.OutQuint);
                textBold.FadeIn(transition_length, Easing.OutQuint);
            }

            private void fadeInactive()
            {
                this.ResizeTo(new Vector2(Width, 1), transition_length, Easing.OutQuint);

                closeButton?.MoveToX(5f, 0.5f, Easing.InElastic);
                box.FadeColour(backgroundInactive, transition_length, Easing.OutQuint);
                highlightBox.FadeOut(transition_length, Easing.OutQuint);

                text.FadeIn(transition_length, Easing.OutQuint);
                textBold.FadeOut(transition_length, Easing.OutQuint);
            }

            protected override bool OnHover(InputState state)
            {
                closeButton?.FadeIn(1f, Easing.InBounce);
                
                if (!Active)
                    box.FadeColour(backgroundHover, transition_length, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                closeButton?.FadeOut(1f, Easing.OutBounce);
                updateState();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                backgroundActive = colours.ChatBlue;
                backgroundInactive = colours.Gray4;
                backgroundHover = colours.Gray7;

                highlightBox.Colour = colours.Yellow;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            public ChannelTabItem(Channel value) : base(value)
            {
                Width = 150;

                RelativeSizeAxes = Axes.Y;

                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;

                Shear = new Vector2(shear_width / ChatOverlay.TAB_AREA_HEIGHT, 0);

                Masking = true;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 10,
                    Colour = Color4.Black.Opacity(0.2f),
                };

                Children = new Drawable[]
                {
                    box = new Box
                    {
                        EdgeSmoothness = new Vector2(1, 0),
                        RelativeSizeAxes = Axes.Both,
                    },
                    highlightBox = new Box
                    {
                        Width = 5,
                        Alpha = 0,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        EdgeSmoothness = new Vector2(1, 0),
                        RelativeSizeAxes = Axes.Y,
                    },
                    new Container
                    {
                        Shear = new Vector2(-shear_width / ChatOverlay.TAB_AREA_HEIGHT, 0),
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            icon = new SpriteIcon
                            {
                                Icon = FontAwesome.fa_hashtag,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Colour = Color4.Black,
                                X = -10,
                                Alpha = 0.2f,
                                Size = new Vector2(ChatOverlay.TAB_AREA_HEIGHT),
                            },
                            text = new OsuSpriteText
                            {
                                Margin = new MarginPadding(5),
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Text = value.ToString(),
                                TextSize = 18,
                            },
                            textBold = new OsuSpriteText
                            {
                                Alpha = 0,
                                Margin = new MarginPadding(5),
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Text = value.ToString(),
                                Font = @"Exo2.0-Bold",
                                TextSize = 18,
                            },
                        },
                    },
                };

                if (IsRemovable)
                {
                    Add(closeButton  = new Button
                    {
                        Alpha = 0,
                        Width = 20,
                        Height = 20,
                        Margin = new MarginPadding { Right = 10 },
                        Origin = Anchor.CentreRight,
                        Anchor = Anchor.CentreRight,
                        Text = @"x",
                        BackgroundColour = Color4.Transparent,
                        Action = () => OnRequestClose?.Invoke(),
                    });
                }
            }

            public class ChannelSelectorTabItem : ChannelTabItem
            {
                protected override bool IsRemovable => false;

                public ChannelSelectorTabItem(Channel value) : base(value)
                {
                    Depth = float.MaxValue;
                    Width = 45;

                    icon.Alpha = 0;

                    text.TextSize = 45;
                    textBold.TextSize = 45;
                }

                [BackgroundDependencyLoader]
                private new void load(OsuColour colour)
                {
                    backgroundInactive = colour.Gray2;
                    backgroundActive = colour.Gray3;
                }
            }

            protected override void OnActivated() => updateState();

            protected override void OnDeactivated() => updateState();
        }
    }
}
