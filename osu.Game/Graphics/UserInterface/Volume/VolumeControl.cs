using System;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Threading;
using OpenTK;

namespace osu.Game.Graphics.UserInterface.Volume
{
    internal class VolumeControl : OverlayContainer
    {
        public BindableDouble VolumeGlobal { get; set; }
        public BindableDouble VolumeSample { get; set; }
        public BindableDouble VolumeTrack { get; set; }

        private VolumeMeter volumeMeterMaster;

        public override bool Contains(Vector2 screenSpacePos) => true;

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
        }

        protected override void Load(BaseGame game)
        {
            VolumeGlobal.ValueChanged += volumeChanged;
            VolumeSample.ValueChanged += volumeChanged;
            VolumeTrack.ValueChanged += volumeChanged;

            Children = new Drawable[]
            {
                new FlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(10, 30),
                    Spacing = new Vector2(15,0),
                    Children = new Drawable[]
                    {
                        volumeMeterMaster = new VolumeMeter("Master", VolumeGlobal),
                        new VolumeMeter("Effects", VolumeSample),
                        new VolumeMeter("Music", VolumeTrack)
                    }
                }
            };

            base.Load(game);
        }

        protected override void Dispose(bool isDisposing)
        {
            VolumeGlobal.ValueChanged -= volumeChanged;
            VolumeSample.ValueChanged -= volumeChanged;
            VolumeTrack.ValueChanged -= volumeChanged;
            base.Dispose(isDisposing);
        }

        protected override bool OnWheel(InputState state)
        {
            if (!IsVisible)
                return false;

            volumeMeterMaster.TriggerWheel(state);
            return true;
        }

        ScheduledDelegate popOutDelegate;

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