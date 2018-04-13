﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Volume;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class VolumeOverlay : OverlayContainer
    {
        private const float offset = 10;

        private VolumeMeter volumeMeterMaster;
        private VolumeMeter volumeMeterEffect;
        private VolumeMeter volumeMeterMusic;
        private MuteButton muteButton;

        protected override bool BlockPassThroughMouse => false;

        private readonly BindableDouble muteAdjustment = new BindableDouble();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.75f), Color4.Black.Opacity(0))
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Spacing = new Vector2(0, offset),
                    Margin = new MarginPadding { Left = offset },
                    Children = new Drawable[]
                    {
                        volumeMeterEffect = new VolumeMeter("EFFECTS", 125, colours.BlueDarker)
                        {
                            Margin = new MarginPadding { Top = 100 + MuteButton.HEIGHT } //to counter the mute button and re-center the volume meters
                        },
                        volumeMeterMaster = new VolumeMeter("MASTER", 150, colours.PinkDarker),
                        volumeMeterMusic = new VolumeMeter("MUSIC", 125, colours.BlueDarker),
                        muteButton = new MuteButton
                        {
                            Margin = new MarginPadding { Top = 100 }
                        }
                    }
                },
            });

            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);

            muteButton.Current.ValueChanged += mute =>
            {
                if (mute)
                    audio.AddAdjustment(AdjustableProperty.Volume, muteAdjustment);
                else
                    audio.RemoveAdjustment(AdjustableProperty.Volume, muteAdjustment);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            volumeMeterMaster.Bindable.ValueChanged += _ => settingChanged();
            volumeMeterEffect.Bindable.ValueChanged += _ => settingChanged();
            volumeMeterMusic.Bindable.ValueChanged += _ => settingChanged();
            muteButton.Current.ValueChanged += _ => settingChanged();
        }

        private void settingChanged()
        {
            Show();
            schedulePopOut();
        }

        public bool Adjust(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    if (State == Visibility.Hidden)
                        Show();
                    else
                        volumeMeterMaster.Decrease();
                    return true;
                case GlobalAction.IncreaseVolume:
                    if (State == Visibility.Hidden)
                        Show();
                    else
                        volumeMeterMaster.Increase();
                    return true;
                case GlobalAction.ToggleMute:
                    Show();
                    muteButton.Current.Value = !muteButton.Current;
                    return true;
            }

            return false;
        }

        private ScheduledDelegate popOutDelegate;

        protected override void PopIn()
        {
            ClearTransforms();
            this.FadeIn(100);

            schedulePopOut();
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();
            this.Delay(1000).Schedule(Hide, out popOutDelegate);
        }
    }
}
