// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Toolbar
{
    public class Toolbar : VisibilityContainer
    {
        public const float HEIGHT = 40;
        public const float TOOLTIP_HEIGHT = 30;

        public Action OnHome;

        private ToolbarUserButton userButton;
        private ToolbarRulesetSelector rulesetSelector;

        private const double transition_time = 500;

        private const float alpha_hovering = 0.8f;
        private const float alpha_normal = 0.6f;

        private readonly Bindable<OverlayActivation> overlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        public Toolbar()
        {
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, HEIGHT);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame osuGame, Bindable<RulesetInfo> parentRuleset)
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
                        rulesetSelector = new ToolbarRulesetSelector()
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
                        new ToolbarChangelogButton(),
                        new ToolbarDirectButton(),
                        new ToolbarChatButton(),
                        new ToolbarSocialButton(),
                        new ToolbarMusicButton(),
                        //new ToolbarButton
                        //{
                        //    Icon = FontAwesome.Solid.search
                        //},
                        userButton = new ToolbarUserButton(),
                        new ToolbarNotificationButton(),
                    }
                }
            };

            // Bound after the selector is added to the hierarchy to give it a chance to load the available rulesets
            rulesetSelector.Current.BindTo(parentRuleset);

            State.ValueChanged += visibility =>
            {
                if (overlayActivationMode.Value == OverlayActivation.Disabled)
                    Hide();
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

            protected override bool OnHover(HoverEvent e)
            {
                solidBackground.FadeTo(alpha_hovering, transition_time, Easing.OutQuint);
                gradientBackground.FadeIn(transition_time, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
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
            userButton?.StateContainer.Hide();

            this.MoveToY(-DrawSize.Y, transition_time, Easing.OutQuint);
            this.FadeOut(transition_time);
        }
    }
}
