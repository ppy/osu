// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Modes;
using OpenTK;

namespace osu.Game.Overlays.Toolbar
{
    public class Toolbar : OverlayContainer
    {
        public const float HEIGHT = 40;
        public const float TOOLTIP_HEIGHT = 30;

        public Action OnHome;
        public Action<PlayMode> OnPlayModeChange;

        private ToolbarModeSelector modeSelector;
        private ToolbarUserArea userArea;

        protected override bool HideOnEscape => false;

        protected override bool BlockPassThroughInput => false;

        private const int transition_time = 500;

        private const float alpha_hovering = 0.8f;
        private const float alpha_normal = 0.6f;

        public override bool Contains(Vector2 screenSpacePos) => true;

        public Toolbar()
        {
            Children = new Drawable[]
            {
                new ToolbarBackground(),
                new FillFlowContainer
                {
                    Direction = FillDirection.Right,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarSettingsButton(),
                        new ToolbarHomeButton()
                        {
                            Action = () => OnHome?.Invoke()
                        },
                        modeSelector = new ToolbarModeSelector
                        {
                            OnPlayModeChange = (PlayMode mode) =>
                            {
                                OnPlayModeChange?.Invoke(mode);
                            }
                        }
                    }
                },
                new PassThroughFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Right,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarMusicButton(),
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_search
                        },
                        userArea = new ToolbarUserArea(),
                        new ToolbarNotificationButton(),
                    }
                }
            };

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, HEIGHT);
        }

        public class ToolbarBackground : Container
        {
            private Box solidBackground;
            private Box gradientBackground;

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
                        ColourInfo = ColourInfo.GradientVertical(
                            OsuColour.Gray(0.1f).Opacity(0.5f), OsuColour.Gray(0.1f).Opacity(0)),
                    },
                };
            }

            protected override bool OnHover(InputState state)
            {
                solidBackground.FadeTo(alpha_hovering, transition_time, EasingTypes.OutQuint);
                gradientBackground.FadeIn(transition_time, EasingTypes.OutQuint);
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                solidBackground.FadeTo(alpha_normal, transition_time, EasingTypes.OutQuint);
                gradientBackground.FadeOut(transition_time, EasingTypes.OutQuint);
            }
        }

        public void SetGameMode(PlayMode mode) => modeSelector.SetGameMode(mode);

        protected override void PopIn()
        {
            MoveToY(0, transition_time, EasingTypes.OutQuint);
            FadeIn(transition_time / 2, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            userArea?.LoginOverlay.Hide();

            MoveToY(-DrawSize.Y, transition_time, EasingTypes.OutQuint);
            FadeOut(transition_time);
        }

        class PassThroughFlowContainer : FillFlowContainer
        {
            //needed to get input to the login overlay.
            public override bool Contains(Vector2 screenSpacePos) => true;
        }
    }
}
