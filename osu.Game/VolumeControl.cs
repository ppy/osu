using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Graphics.Transformations;
using OpenTK;

namespace osu.Game
{
    internal class VolumeControl : Container
    {
        private FlowContainer volumeMetersContainer;
        private VolumeMeter VolumeMeterGlobal;
        private VolumeMeter VolumeMeterSample;
        private VolumeMeter VolumeMeterTrack;

        public BindableDouble VolumeGlobal { get; set; }
        public BindableDouble VolumeSample { get; set; }
        public BindableDouble VolumeTrack { get; set; }

        public VolumeControl()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Load()
        {
            base.Load();
            Children = new Drawable[]
            {
                volumeMetersContainer = new FlowContainer() {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(10, 30),
                    Alpha = 0,
                    Padding = new Vector2(15,0),
                    Children = new Drawable[]
                    {
                        VolumeMeterGlobal = new VolumeMeter("Master"),
                        VolumeMeterSample= new VolumeMeter("Effects"),
                        VolumeMeterTrack= new VolumeMeter("Music")
                    }
                }
            };

            updateFill();
        }

        protected override bool OnWheelDown(InputState state)
        {
            appear();

            if (VolumeMeterSample.Contains(state.Mouse.Position))
            {
                VolumeSample.Value -= 0.05f;
            }
            else if (VolumeMeterTrack.Contains(state.Mouse.Position))
            {
                VolumeTrack.Value -= 0.05f;
            }
            else
            {
                VolumeGlobal.Value -= 0.05f;
            }

            updateFill();

            return base.OnWheelDown(state);
        }

        protected override bool OnWheelUp(InputState state)
        {
            appear();

            if (VolumeMeterSample.Contains(state.Mouse.Position))
            {
                VolumeSample.Value += 0.05f;
            }
            else if (VolumeMeterTrack.Contains(state.Mouse.Position))
            {
                VolumeTrack.Value += 0.05f;
            }
            else
            {
                VolumeGlobal.Value += 0.05f;
            }

            updateFill();

            return base.OnWheelUp(state);
        }

        private void updateFill()
        {
            VolumeMeterGlobal.MeterFill.ScaleTo(new Vector2(1, (float)VolumeGlobal.Value), 300, EasingTypes.OutQuint);
            VolumeMeterSample.MeterFill.ScaleTo(new Vector2(1, (float)VolumeSample.Value), 300, EasingTypes.OutQuint);
            VolumeMeterTrack.MeterFill.ScaleTo(new Vector2(1, (float)VolumeTrack.Value), 300, EasingTypes.OutQuint);
        }

        private void appear()
        {
            volumeMetersContainer.ClearTransformations();
            volumeMetersContainer.FadeIn(100);
            volumeMetersContainer.Delay(1000);
            volumeMetersContainer.FadeOut(100);
        }
    }
}