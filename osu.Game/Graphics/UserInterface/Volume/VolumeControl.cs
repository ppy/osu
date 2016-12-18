//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Configuration;
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
        private VolumeMeter volumeMeterMaster;

        private void volumeChanged(object sender, EventArgs e)
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
                new FlowContainer
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
            if (!IsVisible)
            {
                Show();
                return;
            }

            volumeMeterMaster.TriggerWheel(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            volumeMeterMaster.Bindable.Weld(audio.Volume);
            volumeMeterEffect.Bindable.Weld(audio.VolumeSample);
            volumeMeterMusic.Bindable.Weld(audio.VolumeTrack);
        }

        ScheduledDelegate popOutDelegate;

        private VolumeMeter volumeMeterEffect;
        private VolumeMeter volumeMeterMusic;

        protected override void PopIn()
        {
            ClearTransformations();
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