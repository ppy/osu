using System;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Threading;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Graphics.UserInterface.Volume
{
    internal class VolumeControl : OverlayContainer
    {
        public BindableDouble VolumeGlobal { get; set; }
        public BindableDouble VolumeSample { get; set; }
        public BindableDouble VolumeTrack { get; set; }

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

            VolumeGlobal.ValueChanged += volumeChanged;
            VolumeSample.ValueChanged += volumeChanged;
            VolumeTrack.ValueChanged += volumeChanged;

            volumeMeterMaster.Bindable = VolumeGlobal;
            volumeMeterEffect.Bindable = VolumeSample;
            volumeMeterMusic.Bindable = VolumeTrack;
        }

        protected override void Dispose(bool isDisposing)
        {
            VolumeGlobal.ValueChanged -= volumeChanged;
            VolumeSample.ValueChanged -= volumeChanged;
            VolumeTrack.ValueChanged -= volumeChanged;
            base.Dispose(isDisposing);
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