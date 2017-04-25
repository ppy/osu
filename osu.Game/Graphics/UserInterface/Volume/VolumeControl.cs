// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Threading;
using OpenTK;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Audio;
using osu.Framework.Allocation;

namespace osu.Game.Graphics.UserInterface.Volume
{
    internal class VolumeControl : OverlayContainer
    {
        private readonly VolumeMeter volumeMeterMaster;

        protected override bool HideOnEscape => false;

        private void volumeChanged(double newVolume)
        {
            Show();
            schedulePopOut();
        }

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

        public void Adjust(InputState state)
        {
            if (State == Visibility.Hidden)
            {
                Show();
                return;
            }

            volumeMeterMaster.TriggerWheel(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
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
            FadeIn(100);

            schedulePopOut();
        }

        protected override void PopOut()
        {
            FadeOut(100);
        }

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();
            Delay(1000);
            popOutDelegate = Schedule(Hide);
        }
    }
}