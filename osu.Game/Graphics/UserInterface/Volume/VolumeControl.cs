// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using OpenTK;
using osu.Framework.Audio;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface.Volume
{
    public class VolumeControl : OverlayContainer
    {
        private AudioManager audio;

        private readonly VolumeMeter volumeMeterMaster;
        private readonly IconButton muteIcon;

        protected override bool BlockPassThroughMouse => false;

        public VolumeControl()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 10, Right = 10, Top = 30, Bottom = 30 },
                    Spacing = new Vector2(15, 0),
                    Children = new Drawable[]
                    {
                        muteIcon = new IconButton
                        {
                            Icon = FontAwesome.fa_volume_up,
                            Scale = new Vector2(2.0f),
                            Action = () =>
                            {
                                if (IsMuted)
                                    Unmute();
                                else
                                    Mute();
                            },
                        },
                        volumeMeterMaster = new VolumeMeter("Master"),
                        volumeMeterEffect = new VolumeMeter("Effects"),
                        volumeMeterMusic = new VolumeMeter("Music")
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            volumeMeterMaster.Bindable.ValueChanged += volumeChanged;
            volumeMeterEffect.Bindable.ValueChanged += volumeChanged;
            volumeMeterMusic.Bindable.ValueChanged += volumeChanged;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            volumeMeterMaster.Bindable.ValueChanged -= volumeChanged;
            volumeMeterEffect.Bindable.ValueChanged -= volumeChanged;
            volumeMeterMusic.Bindable.ValueChanged -= volumeChanged;
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
                    if (State == Visibility.Hidden)
                        Show();
                    if (IsMuted)
                        Unmute();
                    else
                        Mute();
                    return true;
            }

            return false;
        }

        private void volumeChanged(double newVolume)
        {
            Show();
            schedulePopOut();
        }

        private readonly BindableDouble muteBindable = new BindableDouble();

        public bool IsMuted { get; private set; }

        public void Mute()
        {
            if (IsMuted)
                return;

            audio.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            IsMuted = true;
            muteIcon.Icon = FontAwesome.fa_volume_off;
        }

        public void Unmute()
        {
            if (!IsMuted)
                return;

            audio.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
            IsMuted = false;
            muteIcon.Icon = FontAwesome.fa_volume_up;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            this.audio = audio;
            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);
        }

        private ScheduledDelegate popOutDelegate;

        private readonly VolumeMeter volumeMeterEffect;
        private readonly VolumeMeter volumeMeterMusic;

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
