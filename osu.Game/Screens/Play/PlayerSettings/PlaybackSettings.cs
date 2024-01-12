// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class PlaybackSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        public readonly Bindable<double> UserPlaybackRate = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.1,
        };


        private readonly PlayerSliderBar<double> rateSlider;

        private readonly OsuSpriteText multiplierText;

        private readonly BindableBool isPaused = new BindableBool();

        [Resolved]
        private GameplayClockContainer? gameplayClock { get; set; }

        [Resolved]
        private GameplayState? gameplayState { get; set; }

        public PlaybackSettings()
            : base("playback")
        {
            const double seek_amount = 5000;
            const double seek_fast_amount = 10000;

            IconButton play;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, padding),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5, 0),
                            Children = new Drawable[]
                            {
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.FastBackward,
                                    Action = () => seek(-1, seek_fast_amount),
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Backward,
                                    Action = () => seek(-1, seek_amount),
                                },
                                play = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Scale = new Vector2(1.4f),
                                    IconScale = new Vector2(1.4f),
                                    Icon = FontAwesome.Regular.PlayCircle,
                                    Action = () =>
                                    {
                                        if (gameplayClock != null)
                                        {
                                            if (gameplayClock.IsRunning)
                                                gameplayClock.Stop();
                                            else
                                                gameplayClock.Start();
                                        }
                                    },
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Forward,
                                    Action = () => seek(1, seek_amount),
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.FastForward,
                                    Action = () => seek(1, seek_fast_amount),
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                rateSlider = new PlayerSliderBar<double>
                                {
                                    LabelText = "Playback speed",
                                    Current = UserPlaybackRate,
                                },
                                multiplierText = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                                    Margin = new MarginPadding { Right = 20 },
                                }
                            },
                        },
                    },
                },
            };

            isPaused.BindValueChanged(e => play.Icon = !e.NewValue ? FontAwesome.Regular.PauseCircle : FontAwesome.Regular.PlayCircle, true);

            void seek(int direction, double amount)
            {
                double target = Math.Clamp((gameplayClock?.CurrentTime ?? 0) + (direction * amount), 0, gameplayState?.Beatmap.GetLastObjectTime() ?? 0);
                gameplayClock?.Seek(target);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            rateSlider.Current.BindValueChanged(multiplier => multiplierText.Text = $"{multiplier.NewValue:0.0}x", true);

            if (gameplayClock != null)
                isPaused.BindTarget = gameplayClock.IsPaused;
        }

        private partial class SeekButton : IconButton
        {
            public SeekButton()
            {
                AddInternal(new RepeatingButtonBehaviour(this));
            }
        }
    }
}
