// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class PlaybackSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        public readonly Bindable<double> UserPlaybackRate = new BindableDouble(1)
        {
            MinValue = 0.05,
            MaxValue = 2,
            Precision = 0.01,
        };

        private PlayerSliderBar<double> rateSlider = null!;

        private OsuSpriteText multiplierText = null!;

        private readonly IBindable<bool> isPaused = new BindableBool();

        [Resolved]
        private GameplayClockContainer gameplayClock { get; set; } = null!;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        private IconButton pausePlay = null!;

        public PlaybackSettings()
            : base("playback")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            const double seek_amount = 5000;
            const double seek_fast_amount = 10000;

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
                                    TooltipText = PlayerSettingsOverlayStrings.SeekBackwardSeconds(seek_fast_amount / 1000),
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Backward,
                                    Action = () => seek(-1, seek_amount),
                                    TooltipText = PlayerSettingsOverlayStrings.SeekBackwardSeconds(seek_amount / 1000),
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.StepBackward,
                                    Action = () => seekFrame(-1),
                                    TooltipText = PlayerSettingsOverlayStrings.StepBackward,
                                },
                                pausePlay = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Scale = new Vector2(1.4f),
                                    IconScale = new Vector2(1.4f),
                                    Action = () =>
                                    {
                                        if (gameplayClock.IsRunning)
                                            gameplayClock.Stop();
                                        else
                                            gameplayClock.Start();
                                    },
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.StepForward,
                                    Action = () => seekFrame(1),
                                    TooltipText = PlayerSettingsOverlayStrings.StepForward,
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Forward,
                                    Action = () => seek(1, seek_amount),
                                    TooltipText = PlayerSettingsOverlayStrings.SeekForwardSeconds(seek_amount / 1000),
                                },
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.FastForward,
                                    Action = () => seek(1, seek_fast_amount),
                                    TooltipText = PlayerSettingsOverlayStrings.SeekForwardSeconds(seek_fast_amount / 1000),
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            rateSlider.Current.BindValueChanged(multiplier => multiplierText.Text = $"{multiplier.NewValue:0.00}x", true);

            isPaused.BindTo(gameplayClock.IsPaused);
            isPaused.BindValueChanged(paused =>
            {
                if (!paused.NewValue)
                {
                    pausePlay.TooltipText = ToastStrings.PauseTrack;
                    pausePlay.Icon = FontAwesome.Regular.PauseCircle;
                }
                else
                {
                    pausePlay.TooltipText = ToastStrings.PlayTrack;
                    pausePlay.Icon = FontAwesome.Regular.PlayCircle;
                }
            }, true);
        }

        private void seekFrame(int direction)
        {
            gameplayClock.Stop();

            var frames = gameplayState.Score.Replay.Frames;

            if (frames.Count == 0)
                return;

            gameplayClock.Seek(direction < 0
                ? (frames.LastOrDefault(f => f.Time < gameplayClock.CurrentTime) ?? frames.First()).Time
                : (frames.FirstOrDefault(f => f.Time > gameplayClock.CurrentTime) ?? frames.Last()).Time
            );
        }

        private void seek(int direction, double amount)
        {
            double target = Math.Clamp(gameplayClock.CurrentTime + (direction * amount), 0, gameplayState.Beatmap.GetLastObjectTime());
            gameplayClock.Seek(target);
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
