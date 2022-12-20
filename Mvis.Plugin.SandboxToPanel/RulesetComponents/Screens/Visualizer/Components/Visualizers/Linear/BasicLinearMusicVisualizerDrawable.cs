using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Visualizers.Linear
{
    public partial class BasicLinearMusicVisualizerDrawable : LinearMusicVisualizerDrawable
    {
        protected override LinearVisualizerDrawNode CreateLinearVisualizerDrawNode() => new BasicDrawNode(this);

        private partial class BasicDrawNode : LinearVisualizerDrawNode
        {
            public BasicDrawNode(BasicLinearMusicVisualizerDrawable dw)
                : base(dw)
            {
            }

            protected override void DrawBar(Vector2 barPosition, Vector2 barSize, IRenderer renderer)
            {
                var adjustedSize = barSize + new Vector2(0, 2);

                renderer.DrawQuad(
                    Texture,
                    getQuad(barPosition, adjustedSize),
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.Y, 1f / barSize.X));
            }

            private Quad getQuad(Vector2 barPosition, Vector2 barSize)
            {
                switch (Origin)
                {
                    default:
                    case BarAnchor.Bottom:
                        return new Quad(
                            Vector2Extensions.Transform(barPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.Y), DrawInfo.Matrix)
                            );

                    case BarAnchor.Centre:
                        return new Quad(
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.Y / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.Y / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.Y / 2), DrawInfo.Matrix)
                            );

                    case BarAnchor.Top:
                        return new Quad(
                            Vector2Extensions.Transform(barPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.Y), DrawInfo.Matrix)
                            );
                }
            }
        }
    }
}
