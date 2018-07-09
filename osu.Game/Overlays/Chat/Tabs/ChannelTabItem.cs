// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class ChannelTabItem : TabItem<Channel>
    {

        protected Color4 BackgroundInactive;
        private Color4 backgroundHover;
        protected Color4 BackgroundActive;

        public override bool IsRemovable => !Pinned;

        protected readonly SpriteText Text;
        protected readonly SpriteText TextBold;
        private readonly ClickableContainer closeButton;
        private readonly Box box;
        private readonly Box highlightBox;
        protected readonly SpriteIcon Icon;

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

            box.FadeColour(BackgroundActive, transition_length, Easing.OutQuint);
            highlightBox.FadeIn(transition_length, Easing.OutQuint);

            Text.FadeOut(transition_length, Easing.OutQuint);
            TextBold.FadeIn(transition_length, Easing.OutQuint);
        }

        private void fadeInactive()
        {
            this.ResizeTo(new Vector2(Width, 1), transition_length, Easing.OutQuint);

            box.FadeColour(BackgroundInactive, transition_length, Easing.OutQuint);
            highlightBox.FadeOut(transition_length, Easing.OutQuint);

            Text.FadeIn(transition_length, Easing.OutQuint);
            TextBold.FadeOut(transition_length, Easing.OutQuint);
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
            BackgroundActive = colours.ChatBlue;
            BackgroundInactive = colours.Gray4;
            backgroundHover = colours.Gray7;

            highlightBox.Colour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        protected override void OnActivated() => updateState();

        protected override void OnDeactivated() => updateState();

        public ChannelTabItem(Channel value)
            : base(value)
        {
            Width = 150;

            RelativeSizeAxes = Axes.Y;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Shear = new Vector2(ChannelTabControl.shear_width / ChatOverlay.TAB_AREA_HEIGHT, 0);

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
                    Shear = new Vector2(-ChannelTabControl.shear_width / ChatOverlay.TAB_AREA_HEIGHT, 0),
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Icon = new SpriteIcon
                        {
                            Icon = FontAwesome.fa_hashtag,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.Black,
                            X = -10,
                            Alpha = 0.2f,
                            Size = new Vector2(ChatOverlay.TAB_AREA_HEIGHT),
                        },
                        Text = new OsuSpriteText
                        {
                            Margin = new MarginPadding(5),
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Text = value.ToString(),
                            TextSize = 18,
                        },
                        TextBold = new OsuSpriteText
                        {
                            Alpha = 0,
                            Margin = new MarginPadding(5),
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Text = value.ToString(),
                            Font = @"Exo2.0-Bold",
                            TextSize = 18,
                        },
                        closeButton = new TabCloseButton
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
    }
}
