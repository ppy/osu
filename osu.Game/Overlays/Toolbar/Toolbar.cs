//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Modes;
using osu.Game.Online.API;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class Toolbar : OverlayContainer
    {
        private const float height = 50;

        public Action OnHome;
        public Action<PlayMode> OnPlayModeChange;

        private ToolbarModeSelector modeSelector;
        private Box solidBackground;
        private Box gradientBackground;

        private const int transition_time = 250;

        private const float alpha_hovering = 0.8f;
        private const float alpha_normal = 0.6f;


        protected override void PopIn()
        {
            MoveToY(0, transition_time, EasingTypes.OutQuint);
            FadeIn(transition_time, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            MoveToY(-DrawSize.Y, transition_time, EasingTypes.InQuint);
            FadeOut(transition_time, EasingTypes.InQuint);
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

        public Toolbar()
        {
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
                new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
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
                            OnPlayModeChange = OnPlayModeChange
                        }
                    }
                },
                new FlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new []
                    {
                        new ToolbarMusicButton(),
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_search
                        },
                        new ToolbarUserButton(),
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_bars
                        },
                    }
                }
            };

            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, height);
        }

        public void SetGameMode(PlayMode mode) => modeSelector.SetGameMode(mode);
    }
}
