// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

//#nullable disable

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osu.Game.Graphics.Backgrounds;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardVideo : CompositeDrawable, IColouredDimmable
    {
        public readonly StoryboardVideo Video;

        private DrawableVideo? drawableVideo;

        public override bool RemoveWhenNotAlive => false;

        public DrawableStoryboardVideo(StoryboardVideo video)
        {
            Video = video;

            // In osu-stable, a mapper can add a scale command for a storyboard video.
            // This allows scaling based on the video's absolute size.
            //
            // If not specified we take up the full available space.
            bool useRelative = !video.Commands.Scale.Any();

            RelativeSizeAxes = useRelative ? Axes.Both : Axes.None;
            AutoSizeAxes = useRelative ? Axes.None : Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore textureStore)
        {
            var stream = textureStore.GetStream(Video.Path);

            if (stream == null)
                return;

            InternalChild = drawableVideo = new DrawableVideo(stream, false)
            {
                RelativeSizeAxes = RelativeSizeAxes,
                FillMode = FillMode.Fill,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0,
            };

            Video.ApplyTransforms(drawableVideo);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (drawableVideo == null) return;

            using (drawableVideo.BeginAbsoluteSequence(Video.StartTime))
            {
                Schedule(() => drawableVideo.PlaybackPosition = Time.Current - Video.StartTime);

                drawableVideo.FadeIn(500);

                using (drawableVideo.BeginDelayedSequence(drawableVideo.Duration - 500))
                    drawableVideo.FadeOut(500);
            }
        }

        private partial class DrawableVideo : Video, IFlippable, IVectorScalable, IColouredDimmable
        {
            private bool flipH;

            public bool FlipH
            {
                get => flipH;
                set
                {
                    if (flipH == value)
                        return;

                    flipH = value;
                    Invalidate(Invalidation.MiscGeometry);
                }
            }

            private bool flipV;

            public bool FlipV
            {
                get => flipV;
                set
                {
                    if (flipV == value)
                        return;

                    flipV = value;
                    Invalidate(Invalidation.MiscGeometry);
                }
            }

            private Vector2 vectorScale = Vector2.One;

            public Vector2 VectorScale
            {
                get => vectorScale;
                set
                {
                    if (vectorScale == value)
                        return;

                    if (!Validation.IsFinite(value)) throw new ArgumentException($@"{nameof(VectorScale)} must be finite, but is {value}.");

                    vectorScale = value;
                    Invalidate(Invalidation.MiscGeometry);
                }
            }

            protected override Vector2 DrawScale
                => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y) * VectorScale;

            public override Anchor Origin => StoryboardExtensions.AdjustOrigin(base.Origin, VectorScale, FlipH, FlipV);

            protected override VideoSprite CreateSprite() => new DrawableVideoSprite(this);

            public DrawableVideo(Stream stream, bool startAtCurrentTime = true)
                : base(stream, startAtCurrentTime)
            {
            }

            private partial class DrawableVideoSprite : VideoSprite, IColouredDimmable
            {
                private readonly Video video;

                protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
                {
                    bool result = base.OnInvalidate(invalidation, source);

                    if ((invalidation & Invalidation.Colour) > 0)
                    {
                        result |= Invalidate(Invalidation.DrawNode);
                    }

                    return result;
                }

                public DrawableVideoSprite(Video video)
                    : base(video)
                {
                    this.video = video;
                }

                [BackgroundDependencyLoader]
                private void load(ShaderManager shaders)
                {
                    TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableVideo");
                }

                protected override DrawNode CreateDrawNode() => new DrawableVideoSpriteDrawNode(this, video);

                public class DrawableVideoSpriteDrawNode : VideoSpriteDrawNode
                {
                    public new readonly DrawableVideoSprite Source;

                    public DrawableVideoSpriteDrawNode(DrawableVideoSprite source, Video video)
                        : base(video)
                    {
                        Source = source;
                    }

                    private Colour4 drawColourOffset;

                    public override void ApplyState()
                    {
                        base.ApplyState();

                        drawColourOffset = (Source as IColouredDimmable).DrawColourOffset;
                    }

                    private IUniformBuffer<DimParameters>? dimParametersBuffer;

                    protected override void BindUniformResources(IShader shader, IRenderer renderer)
                    {
                        base.BindUniformResources(shader, renderer);

                        dimParametersBuffer ??= renderer.CreateUniformBuffer<DimParameters>();

                        dimParametersBuffer.Data = dimParametersBuffer.Data with
                        {
                            DimColour = new UniformVector4
                            {
                                X = drawColourOffset.R,
                                Y = drawColourOffset.G,
                                Z = drawColourOffset.B,
                                W = drawColourOffset.A
                            },
                        };

                        shader.BindUniformBlock("m_DimParameters", dimParametersBuffer);
                    }

                    protected override void Dispose(bool isDisposing)
                    {
                        base.Dispose(isDisposing);
                        dimParametersBuffer?.Dispose();
                    }

                    [StructLayout(LayoutKind.Sequential, Pack = 1)]
                    private record struct DimParameters
                    {
                        public UniformVector4 DimColour;
                    }
                }
            }
        }
    }
}
