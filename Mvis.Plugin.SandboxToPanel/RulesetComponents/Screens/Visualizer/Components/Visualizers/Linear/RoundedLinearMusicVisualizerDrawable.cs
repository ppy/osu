using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Visualizers.Linear
{
    public partial class RoundedLinearMusicVisualizerDrawable : LinearMusicVisualizerDrawable
    {
        private Texture circleTexture;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            circleTexture = textures.Get("Visualizer/particle");
        }

        protected override LinearVisualizerDrawNode CreateLinearVisualizerDrawNode() => new RoundedDrawNode(this);

        private partial class RoundedDrawNode : LinearVisualizerDrawNode
        {
            protected new RoundedLinearMusicVisualizerDrawable Source => (RoundedLinearMusicVisualizerDrawable)base.Source;

            private Texture circleTexture;

            public RoundedDrawNode(RoundedLinearMusicVisualizerDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();
                circleTexture = Source.circleTexture;
            }

            protected override void DrawBar(Vector2 barPosition, Vector2 barSize, IRenderer renderer)
            {
                switch(Origin)
                {
                    case BarAnchor.Top:
                        drawTop(barPosition, barSize, renderer);
                        return;

                    case BarAnchor.Centre:
                        drawCentre(barPosition, barSize, renderer);
                        return;

                    case BarAnchor.Bottom:
                        drawBottom(barPosition, barSize, renderer);
                        return;
                }
            }

            private void drawTop(Vector2 barPosition, Vector2 barSize, IRenderer renderer)
            {
                var topDot = new Quad(
                            Vector2Extensions.Transform(barPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.X), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    circleTexture,
                    topDot,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.X));

                var adjustedPosition = barPosition + new Vector2(0, barSize.X / 2);

                var bottomDot = new Quad(
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.X + barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.X + barSize.Y), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    circleTexture,
                    bottomDot,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.X));

                var quad = new Quad(
                            Vector2Extensions.Transform(adjustedPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(adjustedPosition + new Vector2(0, barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(adjustedPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(adjustedPosition + new Vector2(barSize.X, barSize.Y), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    Texture,
                    quad,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(0, 1f / barSize.X));
            }

            private void drawCentre(Vector2 barPosition, Vector2 barSize, IRenderer renderer)
            {
                var topDot = new Quad(
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.Y / 2 - barSize.X / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.Y / 2 - barSize.X / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.Y / 2 + barSize.X / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.Y / 2 + barSize.X / 2), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    circleTexture,
                    topDot,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.X));

                var bottomDot = new Quad(
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y / 2 + barSize.X / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.Y / 2 + barSize.X / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y / 2 - barSize.X / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.Y / 2 - barSize.X / 2), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    circleTexture,
                    bottomDot,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.X));

                var quad = new Quad(
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.Y / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, barSize.Y / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.Y / 2), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, barSize.Y / 2), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    Texture,
                    quad,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(0, 1f / barSize.X));
            }

            private void drawBottom(Vector2 barPosition, Vector2 barSize, IRenderer renderer)
            {
                var topDot = new Quad(
                            Vector2Extensions.Transform(barPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.X), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.X), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    circleTexture,
                    topDot,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.X));

                var adjustedPosition = barPosition + new Vector2(0, -barSize.X / 2);

                var bottomDot = new Quad(
                            Vector2Extensions.Transform(barPosition + new Vector2(0, -barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, -barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(0, - barSize.X - barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(barPosition + new Vector2(barSize.X, - barSize.X - barSize.Y), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    circleTexture,
                    bottomDot,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(1f / barSize.X));

                var quad = new Quad(
                            Vector2Extensions.Transform(adjustedPosition, DrawInfo.Matrix),
                            Vector2Extensions.Transform(adjustedPosition + new Vector2(0, -barSize.Y), DrawInfo.Matrix),
                            Vector2Extensions.Transform(adjustedPosition + new Vector2(barSize.X, 0), DrawInfo.Matrix),
                            Vector2Extensions.Transform(adjustedPosition + new Vector2(barSize.X, -barSize.Y), DrawInfo.Matrix)
                            );

                renderer.DrawQuad(
                    Texture,
                    quad,
                    DrawColourInfo.Colour,
                    null,
                    VertexBatch.AddAction,
                    new Vector2(0, 1f / barSize.X));
            }
        }
    }
}
