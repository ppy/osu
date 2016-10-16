using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK;
using osu.Framework.Graphics.Primitives;

namespace osu.Game
{
    internal class VolumeControl : AutoSizeContainer
    {
        private FlowContainer volumeMetersContainer;
        private VolumeMeter volumeMeterMaster;
        public BindableDouble VolumeGlobal { get; set; }
        public BindableDouble VolumeSample { get; set; }
        public BindableDouble VolumeTrack { get; set; }

        public override bool Contains(Vector2 screenSpacePos) => true;

        private void volumeChanged(object sender, System.EventArgs e)
        {
            appear();

            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            VolumeGlobal.ValueChanged += volumeChanged;
            VolumeSample.ValueChanged += volumeChanged;
            VolumeTrack.ValueChanged += volumeChanged;

            Children = new Drawable[]
            {
                volumeMetersContainer = new FlowContainer
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(10, 30),
                    Spacing = new Vector2(15,0),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        volumeMeterMaster = new VolumeMeter("Master", VolumeGlobal),
                        new VolumeMeter("Effects", VolumeSample),
                        new VolumeMeter("Music", VolumeTrack)
                    }
                }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            VolumeGlobal.ValueChanged -= volumeChanged;
            VolumeSample.ValueChanged -= volumeChanged;
            VolumeTrack.ValueChanged -= volumeChanged;
            base.Dispose(isDisposing);
        }

        protected override bool OnWheelDown(InputState state)
        {
            volumeMeterMaster.TriggerWheelDown(state);
            return true;
        }

        protected override bool OnWheelUp(InputState state)
        {
            volumeMeterMaster.TriggerWheelUp(state);
            return true;
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