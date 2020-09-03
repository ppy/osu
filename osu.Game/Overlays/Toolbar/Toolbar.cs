﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        protected readonly IBindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        // Toolbar components like RulesetSelector should receive keyboard input events even when the toolbar is hidden.
        public override bool PropagateNonPositionalInputSubTree => true;

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
                        new ToolbarNewsButton(),
                        new ToolbarChangelogButton(),
                        new ToolbarRankingsButton(),
                        new ToolbarBeatmapListingButton(),
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

            if (osuGame != null)
                OverlayActivationMode.BindTo(osuGame.OverlayActivationMode);
        }

        public class ToolbarBackground : Container
        {
            private readonly Box gradientBackground;

            public ToolbarBackground()
            {
                RelativeSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.1f),
                    },
                    gradientBackground = new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.BottomLeft,
                        Alpha = 0,
                        Height = 100,
                        Colour = ColourInfo.GradientVertical(
                            OsuColour.Gray(0).Opacity(0.9f), OsuColour.Gray(0).Opacity(0)),
                    },
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                gradientBackground.FadeIn(transition_time, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                gradientBackground.FadeOut(transition_time, Easing.OutQuint);
            }
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            if (state.NewValue == Visibility.Visible && OverlayActivationMode.Value == OverlayActivation.Disabled)
            {
                State.Value = Visibility.Hidden;
                return;
            }

            base.UpdateState(state);
        }

        protected override void PopIn()
        {
            this.MoveToY(0, transition_time, Easing.OutQuint);
            this.FadeIn(transition_time / 4, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            userButton.StateContainer?.Hide();

            this.MoveToY(-DrawSize.Y, transition_time, Easing.OutQuint);
            this.FadeOut(transition_time, Easing.InQuint);
        }
    }
}
