// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using System;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics.Containers;

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

        protected override void AddTabItem(TabItem<Channel> item, bool addToDropdown = true)
        {
            if (item != selectorTab && TabContainer.GetLayoutPosition(selectorTab) < float.MaxValue)
                // performTabSort might've made selectorTab's position wonky, fix it
                TabContainer.SetLayoutPosition(selectorTab, float.MaxValue);

            base.AddTabItem(item, addToDropdown);

            if (SelectedTab == null)
                SelectTab(item);
        }

        protected override TabItem<Channel> CreateTabItem(Channel value) => new ChannelTabItem(value) { OnRequestClose = tabCloseRequested };

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

        private void tabCloseRequested(TabItem<Channel> tab)
        {
            int totalTabs = TabContainer.Count - 1; // account for selectorTab
            int currentIndex = MathHelper.Clamp(TabContainer.IndexOf(tab), 1, totalTabs);

            if (tab == SelectedTab && totalTabs > 1)
                // Select the tab after tab-to-be-removed's index, or the tab before if current == last
                SelectTab(TabContainer[currentIndex == totalTabs ? currentIndex - 1 : currentIndex + 1]);
            else if (totalTabs == 1 && !selectorTab.Active)
                // Open channel selection overlay if all channel tabs will be closed after removing this tab
                SelectTab(selectorTab);

            OnRequestLeave?.Invoke(tab.Value);
        }

        private class ChannelTabItem : TabItem<Channel>
        {
            private Color4 backgroundInactive;
            private Color4 backgroundHover;
            private Color4 backgroundActive;

            public override bool IsRemovable => !Pinned;

            private readonly SpriteText text;
            private readonly SpriteText textBold;
            private readonly ClickableContainer closeButton;
            private readonly Box box;
            private readonly Box highlightBox;
            private readonly SpriteIcon icon;

            public Action<ChannelTabItem> OnRequestClose;

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

                box.FadeColour(backgroundActive, transition_length, Easing.OutQuint);
                highlightBox.FadeIn(transition_length, Easing.OutQuint);

                text.FadeOut(transition_length, Easing.OutQuint);
                textBold.FadeIn(transition_length, Easing.OutQuint);
            }

            private void fadeInactive()
            {
                this.ResizeTo(new Vector2(Width, 1), transition_length, Easing.OutQuint);

                box.FadeColour(backgroundInactive, transition_length, Easing.OutQuint);
                highlightBox.FadeOut(transition_length, Easing.OutQuint);

                text.FadeIn(transition_length, Easing.OutQuint);
                textBold.FadeOut(transition_length, Easing.OutQuint);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                if (args.Button == MouseButton.Middle)
                {
                    closeButton.Action();
                    return true;
                }

                return false;
            }

            protected override bool OnHover(InputState state)
            {
                if (IsRemovable)
                    closeButton.FadeIn(200, Easing.OutQuint);

                if (!Active)
                    box.FadeColour(backgroundHover, transition_length, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                closeButton.FadeOut(200, Easing.OutQuint);
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
                            closeButton = new CloseButton
                            {
                                Alpha = 0,
                                Margin = new MarginPadding { Right = 20 },
                                Origin = Anchor.CentreRight,
                                Anchor = Anchor.CentreRight,
                                Action = delegate
                                {
                                    if (IsRemovable) OnRequestClose?.Invoke(this);
                                },
                            },
                        },
                    },
                };
            }

            public class CloseButton : OsuClickableContainer
            {
                private readonly SpriteIcon icon;

                public CloseButton()
                {
                    Size = new Vector2(20);

                    Child = icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(0.75f),
                        Icon = FontAwesome.fa_close,
                        RelativeSizeAxes = Axes.Both,
                    };
                }

                protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
                {
                    icon.ScaleTo(0.5f, 1000, Easing.OutQuint);
                    return base.OnMouseDown(state, args);
                }

                protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
                {
                    icon.ScaleTo(0.75f, 1000, Easing.OutElastic);
                    return base.OnMouseUp(state, args);
                }

                protected override bool OnHover(InputState state)
                {
                    icon.FadeColour(Color4.Red, 200, Easing.OutQuint);
                    return base.OnHover(state);
                }

                protected override void OnHoverLost(InputState state)
                {
                    icon.FadeColour(Color4.White, 200, Easing.OutQuint);
                    base.OnHoverLost(state);
                }
            }

            public class ChannelSelectorTabItem : ChannelTabItem
            {
                public override bool IsRemovable => false;

                public override bool IsSwitchable => false;

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
