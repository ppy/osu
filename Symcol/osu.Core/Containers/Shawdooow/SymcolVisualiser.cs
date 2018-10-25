namespace osu.Core.Containers.Shawdooow
{
    /*
    internal class SymcolVisualiser : Drawable
    {
        private readonly Bindable<WorkingBeatmap> beatmap = new Bindable<Beatmaps.WorkingBeatmap>();
        private float[] audioData = new float[256];

        private const float minimumAmplitude = 1f / bar_length;

        //Needs to be a multiple of 3
        private const int bars = 255;

        private const float bar_length = 300;

        private Color4 barColor;

        public override bool IsPresent => false;

        private Shader shader;
        private readonly Texture texture;

        public SymcolVisualiser()
        {
            Alpha = 1;
            Anchor = Anchor.TopLeft;
            Origin = Anchor.TopLeft;
            RelativeSizeAxes = Axes.Y;
            texture = Texture.WhitePixel;
            barColor = new Color4(1, 1, 1, 0.2f);
            Blending = BlendingMode.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame game, ShaderManager shaders)
        {
            if (game?.Beatmap != null)
                beatmap.BindTo(game.Beatmap);
            shader = shaders?.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED);
        }

        protected override void Update()
        {
            base.Update();
            float[] tempData = beatmap?.Value?.Track?.CurrentAmplitudes.FrequencyAmplitudes ?? new float[256];
            for (int i = 0; i < 256; i++)
                if (beatmap?.Value?.Track?.IsRunning ?? false)
                {
                    float targetAmplitude = tempData[i] + Math.Min(tempData[i] * (tempData[i] / (1f / 3)), 0.3f);
                    if (audioData[i] < targetAmplitude)
                        audioData[i] = targetAmplitude;
                }
            float decayFactor = (float)Time.Elapsed * 0.0024f;
            for (int i = 0; i < bars; i++)
            {
                //4% of extra bar length to make it a little faster when bar is almost at it's minimum
                audioData[i] -= decayFactor * (audioData[i] + 0.04f);
                if (audioData[i] < 0)
                    audioData[i] = 0;
            }

            Invalidate(Invalidation.DrawNode, shallPropagate: true);
        }

        protected override DrawNode CreateDrawNode() => new SymcolVisDrawNode();

        private readonly SymcolVisSharedData sharedData = new SymcolVisSharedData();

        private class SymcolVisSharedData
        {
            public readonly LinearBatch<TexturedVertex2D> VertexBatch = new LinearBatch<TexturedVertex2D>(100 * 4, 10, PrimitiveType.Quads);
        }

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            var visNode = (SymcolVisDrawNode)node;

            visNode.Shader = shader;
            visNode.Texture = texture;
            visNode.Size = DrawSize.Y;
            visNode.Shared = sharedData;
            visNode.Colour = barColor;
            visNode.AudioData = audioData;
        }

        private class SymcolVisDrawNode : DrawNode
        {
            public Shader Shader;
            public Texture Texture;
            public SymcolVisSharedData Shared;
            //Vertical size m8.
            public float Size;

            public Color4 Colour;
            public float[] AudioData;


            public override void Draw(Action<TexturedVertex2D> vertexAction)
            {
                base.Draw(vertexAction);

                Shader.Bind();
                Texture.TextureGL.Bind();

                Vector2 inflation = DrawInfo.MatrixInverse.ExtractScale().Xy;

                ColourInfo colourInfo = DrawInfo.Colour;
                colourInfo.ApplyChild(Colour);

                if (AudioData != null)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int i = 0; i < bars / 3; i++)
                        {
                            float targetAmplitude = AudioData[i + j * (bars / 3)];
                            if (targetAmplitude < minimumAmplitude)
                                continue;

                            var barSize = new Vector2(bar_length * targetAmplitude, (Size - (1f * bars / 3)) / (bars / 3));
                            var barPosition = new Vector2(0, barSize.Y * i + 1f * i);
                            //The distance between the position and the sides of the bar.
                            var bottomOffset = new Vector2(0, barSize.Y);
                            //The distance between the bottom side of the bar and the top side.
                            var amplitudeOffset = new Vector2(barSize.X, 0);
                            
                            var rectangle = new Quad(
                                (barPosition) * DrawInfo.Matrix,
                                (barPosition + amplitudeOffset) * DrawInfo.Matrix,
                                (barPosition + bottomOffset) * DrawInfo.Matrix,
                                (barPosition + bottomOffset + amplitudeOffset) * DrawInfo.Matrix
                            );

                            Texture.DrawQuad(
                                rectangle,
                                colourInfo,
                                null,
                                Shared.VertexBatch.Add,
                                Vector2.Divide(inflation, barSize));
                        }
                    }
                }
                Shader.Unbind();
            }
        }
    }*/
}
