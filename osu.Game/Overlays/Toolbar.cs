//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transformations;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Colour;
using osu.Game.Modes;
using osu.Game.Screens.Play;
using osu.Framework.Input;

namespace osu.Game.Overlays
{
    public class Toolbar : OverlayContainer
    {
        private const float height = 50;

        public Action OnSettings;
        public Action OnHome;
        public Action<PlayMode> OnPlayModeChange;
        public Action OnMusicController;

        private ToolbarModeSelector modeSelector;
        private ToolbarButton userButton;
        private Box gradientBackground;

        private const int transition_time = 200;

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
            gradientBackground.FadeIn(200);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            gradientBackground.FadeOut(200);
        }

        public Toolbar()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0.1f, 0.1f, 0.1f, 0.6f)
                },
                gradientBackground = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomLeft,
                    Alpha = 0,
                    Height = 90,
                    ColourInfo = ColourInfo.GradientVertical(new Color4(0.1f, 0.1f, 0.1f, 0.5f), new Color4(0.1f, 0.1f, 0.1f, 0f)),
                },
                new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_gear,
                            TooltipMain = "Settings",
                            TooltipSub = "Change your settings",
                            Action = () => OnSettings?.Invoke()
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_home,
                            TooltipMain = "Home",
                            TooltipSub = "Return to the main menu",
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
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_music,
                            Action = () => OnMusicController?.Invoke()
                        },
                        new ToolbarButton
                        {
                            Icon = FontAwesome.fa_search
                        },
                        userButton = new ToolbarButton
                        {
                            Icon = FontAwesome.fa_user,
                        },
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

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userButton.Text = config.Get<string>(OsuConfig.Username);
            config.GetBindable<string>(OsuConfig.Username).ValueChanged += delegate
            {
                userButton.Text = config.Get<string>(OsuConfig.Username);
            };
        }

        public void SetGameMode(PlayMode mode) => modeSelector.SetGameMode(mode);
    }
}
