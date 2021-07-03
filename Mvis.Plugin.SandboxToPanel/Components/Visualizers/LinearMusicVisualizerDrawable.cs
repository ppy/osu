using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace Mvis.Plugin.Sandbox.Components.Visualizers
{
    public abstract class LinearMusicVisualizerDrawable : MusicVisualizerDrawable
    {
        public readonly Bindable<BarAnchor> BarAnchorBindable = new Bindable<BarAnchor>(BarAnchor.Centre);

        protected LinearMusicVisualizerDrawable()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override VisualizerDrawNode CreateVisualizerDrawNode() => CreateLinearVisualizerDrawNode();

        protected abstract LinearVisualizerDrawNode CreateLinearVisualizerDrawNode();

        protected abstract class LinearVisualizerDrawNode : VisualizerDrawNode
        {
            public new LinearMusicVisualizerDrawable Source => (LinearMusicVisualizerDrawable)base.Source;

            protected BarAnchor Origin;

            public LinearVisualizerDrawNode(LinearMusicVisualizerDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();
                Origin = Source.BarAnchorBindable.Value;
            }

            protected override float Spacing => (float)((Size.X - BarWidth) / (AudioData.Count - 1));

            private float barY;

            protected override void PreCompute()
            {
                base.PreCompute();
                barY = getBarPositionY();
            }

            protected override void DrawBar(int index, float data, float spacing, Vector2 inflation)
            {
                var barPosition = new Vector2(index * spacing, barY);
                var barSize = new Vector2((float)BarWidth, data);

                DrawBar(barPosition, barSize);
            }

            protected abstract void DrawBar(Vector2 barPosition, Vector2 barSize);

            private float getBarPositionY()
            {
                switch (Origin)
                {
                    default:
                    case BarAnchor.Bottom:
                        return Size.Y;

                    case BarAnchor.Centre:
                        return Size.Y / 2;

                    case BarAnchor.Top:
                        return 0;
                }
            }
        }
    }

    public enum BarAnchor
    {
        Top,
        Centre,
        Bottom
    }
}
