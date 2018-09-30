// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using OpenTK;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input.States;

namespace osu.Game.Overlays.Toolbar
{
    public class Toolbar : OverlayContainer
    {
        public const float HEIGHT = 40;
        public const float TOOLTIP_HEIGHT = 30;

        public Action OnHome;

        private readonly ToolbarUserArea userArea;

        protected override bool BlockPositionalInput => false;

        private const double transition_time = 500;

        private const float alpha_hovering = 0.8f;
        private const float alpha_normal = 0.6f;

        private readonly Bindable<OverlayActivation> overlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        public Toolbar()
        {
            Children = new Drawable[]
            {
                new ToolbarBackground(),
                new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarSettingsButton(),
                        new ToolbarHomeButton
                        {
                            Action = () => OnHome?.Invoke()
                        },
                        new ToolbarRulesetSelector()
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarDirectButton(),
                        new ToolbarChatButton(),
                        new ToolbarSocialButton(),
                        new ToolbarMusicButton(),
                        //new ToolbarButton
                        //{
                        //    Icon = FontAwesome.fa_search
                        //},
                        userArea = new ToolbarUserArea(),
                        new ToolbarNotificationButton(),
                    }
                }
            };

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, HEIGHT);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame osuGame)
        {
            StateChanged += visibility =>
            {
                if (overlayActivationMode == OverlayActivation.Disabled)
                    State = Visibility.Hidden;
            };

            if (osuGame != null)
                overlayActivationMode.BindTo(osuGame.OverlayActivationMode);
        }

        public class ToolbarBackground : Container
        {
            private readonly Box solidBackground;
            private readonly Box gradientBackground;

            public ToolbarBackground()
            {
                RelativeSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    solidBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.1f),
                        Alpha = alpha_normal,
                    },
                    gradientBackground = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.BottomLeft,
                        Alpha = 0,
                        Height = 90,
                        Colour = ColourInfo.GradientVertical(
                            OsuColour.Gray(0.1f).Opacity(0.5f), OsuColour.Gray(0.1f).Opacity(0)),
                    },
                };
            }

            protected override bool OnHover(InputState state)
            {
                solidBackground.FadeTo(alpha_hovering, transition_time, Easing.OutQuint);
                gradientBackground.FadeIn(transition_time, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                solidBackground.FadeTo(alpha_normal, transition_time, Easing.OutQuint);
                gradientBackground.FadeOut(transition_time, Easing.OutQuint);
            }
        }

        protected override void PopIn()
        {
            this.MoveToY(0, transition_time, Easing.OutQuint);
            this.FadeIn(transition_time / 2, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            userArea?.LoginOverlay.Hide();

            this.MoveToY(-DrawSize.Y, transition_time, Easing.OutQuint);
            this.FadeOut(transition_time);
        }
    }
}
