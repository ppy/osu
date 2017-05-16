// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class ChatTabControl : OsuTabControl<Channel>
    {
        protected override TabItem<Channel> CreateTabItem(Channel value) => new ChannelTabItem(value);

        private const float shear_width = 10;

        public ChatTabControl()
        {
            TabContainer.Margin = new MarginPadding { Left = 50 };
            TabContainer.Spacing = new Vector2(-shear_width, 0);
            TabContainer.Masking = false;

            AddInternal(new TextAwesome
            {
                Icon = FontAwesome.fa_comments,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                TextSize = 20,
                Padding = new MarginPadding(10),
            });
        }

        private class ChannelTabItem : TabItem<Channel>
        {
            private Color4 backgroundInactive;
            private Color4 backgroundHover;
            private Color4 backgroundActive;

            private readonly SpriteText text;
            private readonly SpriteText textBold;
            private readonly Box box;
            private readonly Box highlightBox;

            public override bool Active
            {
                get { return base.Active; }
                set
                {
                    if (Active == value) return;

                    base.Active = value;
                    updateState();
                }
            }

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
                ResizeTo(new Vector2(Width, 1.1f), transition_length, EasingTypes.OutQuint);

                box.FadeColour(backgroundActive, transition_length, EasingTypes.OutQuint);
                highlightBox.FadeIn(transition_length, EasingTypes.OutQuint);

                text.FadeOut(transition_length, EasingTypes.OutQuint);
                textBold.FadeIn(transition_length, EasingTypes.OutQuint);
            }

            private void fadeInactive()
            {
                ResizeTo(new Vector2(Width, 1), transition_length, EasingTypes.OutQuint);

                box.FadeColour(backgroundInactive, transition_length, EasingTypes.OutQuint);
                highlightBox.FadeOut(transition_length, EasingTypes.OutQuint);

                text.FadeIn(transition_length, EasingTypes.OutQuint);
                textBold.FadeOut(transition_length, EasingTypes.OutQuint);
            }

            protected override bool OnHover(InputState state)
            {
                if (!Active)
                    box.FadeColour(backgroundHover, transition_length, EasingTypes.OutQuint);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                updateState();
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                backgroundActive = colours.ChatBlue;
                backgroundInactive = colours.Gray4;
                backgroundHover = colours.Gray7;

                highlightBox.Colour = colours.Yellow;

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
                EdgeEffect = new EdgeEffect
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
                            new TextAwesome
                            {
                                Icon = FontAwesome.fa_hashtag,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Colour = Color4.Black,
                                X = -10,
                                Alpha = 0.2f,
                                TextSize = ChatOverlay.TAB_AREA_HEIGHT,
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
                        }
                    }
                };
            }
        }
    }
}
