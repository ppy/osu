using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game
{
    internal class VolumeControl : Container
    {
        private FlowContainer volumeMetersContainer;
        private VolumeMeter volumeMeterMaster;
        public BindableDouble VolumeGlobal { get; set; }
        public BindableDouble VolumeSample { get; set; }
        public BindableDouble VolumeTrack { get; set; }

        public VolumeControl()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
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

        protected override bool OnWheelDown(InputState state)
        {
            appear();
            volumeMeterMaster.TriggerWheelDown(state);
            return true;
        }

        protected override bool OnWheelUp(InputState state)
        {
            appear();
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