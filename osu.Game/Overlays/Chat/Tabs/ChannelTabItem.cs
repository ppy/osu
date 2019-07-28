// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class ChannelTabItem : TabItem<Channel>
    {
        protected Color4 BackgroundInactive;
        private Color4 backgroundHover;
        protected Color4 BackgroundActive;

        public override bool IsRemovable => !Pinned;

        protected readonly SpriteText Text;
        protected readonly ClickableContainer CloseButton;
        private readonly Box box;
        private readonly Box highlightBox;
        protected readonly SpriteIcon Icon;

        public Action<ChannelTabItem> OnRequestClose;
        private readonly Container content;

        protected override Container<Drawable> Content => content;

        public ChannelTabItem(Channel value)
            : base(value)
        {
            Width = 150;

            RelativeSizeAxes = Axes.Y;

            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            Shear = new Vector2(ChannelTabControl.SHEAR_WIDTH / ChatOverlay.TAB_AREA_HEIGHT, 0);

            Masking = true;

            InternalChildren = new Drawable[]
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
                content = new Container
                {
                    Shear = new Vector2(-ChannelTabControl.SHEAR_WIDTH / ChatOverlay.TAB_AREA_HEIGHT, 0),
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Icon = new SpriteIcon
                        {
                            Icon = DisplayIcon,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = Color4.Black,
                            X = -10,
                            Alpha = 0.2f,
                            Size = new Vector2(ChatOverlay.TAB_AREA_HEIGHT),
                        },
                        Text = new OsuSpriteText
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Text = value.ToString(),
                            Font = OsuFont.GetFont(size: 18),
                            Padding = new MarginPadding(5)
                            {
                                Left = LeftTextPadding,
                                Right = RightTextPadding,
                            },
                            RelativeSizeAxes = Axes.X,
                            Truncate = true,
                        },
                        CloseButton = new TabCloseButton
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

        protected virtual float LeftTextPadding => 5;

        protected virtual float RightTextPadding => IsRemovable ? 40 : 5;

        protected virtual IconUsage DisplayIcon => FontAwesome.Solid.Hashtag;

        protected virtual bool ShowCloseOnHover => true;

        protected virtual bool IsBoldWhenActive => true;

        protected override bool OnHover(HoverEvent e)
        {
            if (IsRemovable && ShowCloseOnHover)
                CloseButton.FadeIn(200, Easing.OutQuint);

            if (!Active.Value)
                box.FadeColour(backgroundHover, TRANSITION_LENGTH, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            CloseButton.FadeOut(200, Easing.OutQuint);
            updateState();
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Middle:
                    CloseButton.Click();
                    return true;

                default:
                    return false;
            }
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
            FinishTransforms(true);
        }

        private void updateState()
        {
            if (Active.Value)
                FadeActive();
            else
                FadeInactive();
        }

        protected const float TRANSITION_LENGTH = 400;

        private readonly EdgeEffectParameters activateEdgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 15,
            Colour = Color4.Black.Opacity(0.4f),
        };

        private readonly EdgeEffectParameters deactivateEdgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 10,
            Colour = Color4.Black.Opacity(0.2f),
        };

        protected virtual void FadeActive()
        {
            this.ResizeHeightTo(1.1f, TRANSITION_LENGTH, Easing.OutQuint);

            TweenEdgeEffectTo(activateEdgeEffect, TRANSITION_LENGTH);

            box.FadeColour(BackgroundActive, TRANSITION_LENGTH, Easing.OutQuint);
            highlightBox.FadeIn(TRANSITION_LENGTH, Easing.OutQuint);

            if (IsBoldWhenActive) Text.Font = Text.Font.With(weight: FontWeight.Bold);
        }

        protected virtual void FadeInactive()
        {
            this.ResizeHeightTo(1, TRANSITION_LENGTH, Easing.OutQuint);

            TweenEdgeEffectTo(deactivateEdgeEffect, TRANSITION_LENGTH);

            box.FadeColour(BackgroundInactive, TRANSITION_LENGTH, Easing.OutQuint);
            highlightBox.FadeOut(TRANSITION_LENGTH, Easing.OutQuint);

            Text.Font = Text.Font.With(weight: FontWeight.Medium);
        }

        protected override void OnActivated() => updateState();
        protected override void OnDeactivated() => updateState();
    }
}
