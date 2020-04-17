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
using osu.Game.Configuration;

namespace osu.Game.Overlays.Toolbar
{
    public class Toolbar : VisibilityContainer
    {
        public const float HEIGHT = 40;
        public const float TOOLTIP_HEIGHT = 30;

        public Action OnHome;

        public readonly Bindable<bool> toolbarIsHovered = new Bindable<bool>();
        private Bindable<bool> optUI { get; set; }
        protected ToolbarTimeButton ToolbarTimeButton { get; private set; }
        protected ToolbarMfButton ToolbarMfButton { get; private set; }
        private ToolbarUserButton userButton;
        private ToolbarRulesetSelector rulesetSelector;
        private FillFlowContainer LeftSideToolbar;
        private ToolbarBackground toolbarBg;

        private const double transition_time = 500;
        private float anim_time = 0;
        private bool OnLaunch = true;

        private const float alpha_hovering = 0.8f;
        private const float alpha_normal = 0.6f;

        private readonly Bindable<OverlayActivation> overlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        public Toolbar()
        {
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, HEIGHT);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame osuGame, Bindable<RulesetInfo> parentRuleset, OsuConfigManager config)
        {
            optUI = config.GetBindable<bool>(OsuSetting.OptUI);

            Children = new Drawable[]
            {
                toolbarBg = new ToolbarBackground(),
                LeftSideToolbar = new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    LayoutDuration = anim_time,
                    LayoutEasing = Easing.OutBounce,
                    Children = new Drawable[]
                    {
                        new ToolbarSettingsButton(),
                        new ToolbarHomeButton
                        {
                            Action = () => OnHome?.Invoke()
                        },
                        ToolbarMfButton =  new ToolbarMfButton(),
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
                        ToolbarTimeButton = new ToolbarTimeButton(),
                        new ToolbarChangelogButton(),
                        new ToolbarRankingsButton(),
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
            toolbarIsHovered.BindTo( toolbarBg.toolbar_IsHovered );

            State.ValueChanged += visibility =>
            {
                if (overlayActivationMode.Value == OverlayActivation.Disabled)
                    Hide();
            };

            if (osuGame != null)
                overlayActivationMode.BindTo(osuGame.OverlayActivationMode);

            UpdateIcons();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            optUI.ValueChanged += _ => UpdateIcons();
        }

        private void UpdateIcons()
        {
            if (!OnLaunch) LeftSideToolbar.LayoutDuration = 600f;
            switch (optUI.Value)
            {
                case true:
                    ToolbarMfButton.FadeTo(1f, 250);
                    ToolbarTimeButton.FadeTo(1f, 250);
                    break;

                case false:
                    ToolbarMfButton.FadeTo(0f, 250);
                    ToolbarTimeButton.FadeTo(0f, 250);
                    break;
            }
        }

        public class ToolbarBackground : Container
        {
            public readonly Bindable<bool> toolbar_IsHovered = new Bindable<bool>();
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
                this.toolbar_IsHovered.Value = true;
                solidBackground.FadeTo(alpha_hovering, transition_time, Easing.OutQuint);
                gradientBackground.FadeIn(transition_time, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.toolbar_IsHovered.Value = false;
                solidBackground.FadeTo(alpha_normal, transition_time, Easing.OutQuint);
                gradientBackground.FadeOut(transition_time, Easing.OutQuint);
            }
        }

        protected override void PopIn()
        {
            this.MoveToY(0, transition_time, Easing.OutQuint);
            this.FadeIn(transition_time / 2, Easing.OutQuint);
            OnLaunch = false;
        }

        protected override void PopOut()
        {
            userButton?.StateContainer.Hide();

            this.MoveToY(-DrawSize.Y, transition_time, Easing.OutQuint);
            this.FadeOut(transition_time);
        }
    }
}
