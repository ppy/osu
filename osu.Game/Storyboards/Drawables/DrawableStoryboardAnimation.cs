// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardAnimation : TextureAnimation, IFlippable, IVectorScalable, IColouredDimmable
    {
        public StoryboardAnimation Animation { get; }

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

        protected override Sprite CreateSprite() => new DrawableStoryboardAnimationSprite
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        };

        public override bool RemoveWhenNotAlive => false;

        protected override Vector2 DrawScale
            => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y) * VectorScale;

        public override Anchor Origin => StoryboardExtensions.AdjustOrigin(base.Origin, VectorScale, FlipH, FlipV);

        public override bool IsPresent
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && base.IsPresent;

        public DrawableStoryboardAnimation(StoryboardAnimation animation)
        {
            Animation = animation;
            Origin = animation.Origin;
            Position = animation.InitialPosition;
            Loop = animation.LoopType == AnimationLoopType.LoopForever;
            Name = animation.Path;

            LifetimeStart = animation.StartTime;
            LifetimeEnd = animation.EndTimeForDisplay;
        }

        [Resolved]
        private ISkinSource skin { get; set; }

        [Resolved]
        private IBeatSyncProvider beatSyncProvider { get; set; }

        [Resolved]
        private TextureStore textureStore { get; set; }

        [BackgroundDependencyLoader]
        private void load(Storyboard storyboard)
        {
            if (storyboard.UseSkinSprites)
            {
                skin.SourceChanged += skinSourceChanged;
                skinSourceChanged();
            }
            else
                addFramesFromStoryboardSource();

            Animation.ApplyTransforms(this);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Framework animation class tries its best to synchronise the animation at LoadComplete,
            // but in some cases (such as fast forward) this results in an incorrect start offset.
            //
            // In the case of storyboard animations, we want to synchronise with game time perfectly
            // so let's get a correct time based on gameplay clock and earliest transform.
            PlaybackPosition = beatSyncProvider.Clock.CurrentTime - Animation.EarliestTransformTime;
        }

        private void skinSourceChanged()
        {
            ClearFrames();

            // When reading from a skin, we match stables weird behaviour where `FrameCount` is ignored
            // and resources are retrieved until the end of the animation.
            var skinTextures = skin.GetTextures(Path.ChangeExtension(Animation.Path, null), default, default, true, string.Empty, null, out _);

            if (skinTextures.Length > 0)
            {
                foreach (var texture in skinTextures)
                    AddFrame(texture, Animation.FrameDelay);
            }
            else
            {
                addFramesFromStoryboardSource();
            }
        }

        private void addFramesFromStoryboardSource()
        {
            int frameIndex;
            // sourcing from storyboard.
            for (frameIndex = 0; frameIndex < Animation.FrameCount; frameIndex++)
                AddFrame(textureStore.Get(getFramePath(frameIndex)), Animation.FrameDelay);

            string getFramePath(int i) => Animation.Path.Replace(".", $"{i}.");
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skin != null)
                skin.SourceChanged -= skinSourceChanged;
        }

        public partial class DrawableStoryboardAnimationSprite : Sprite, IColouredDimmable
        {
            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                bool result = base.OnInvalidate(invalidation, source);

                if ((invalidation & Invalidation.Colour) > 0)
                {
                    result |= Invalidate(Invalidation.DrawNode);
                }

                return result;
            }

            public DrawableStoryboardAnimationSprite()
            {
            }

            [BackgroundDependencyLoader]
            private void load(ShaderManager shaders)
            {
                TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
            }

            protected override DrawNode CreateDrawNode() => new BeatmapBackgroundSpriteDrawNode(this);

            public class BeatmapBackgroundSpriteDrawNode : SpriteDrawNode
            {
                public new DrawableStoryboardAnimationSprite Source => (DrawableStoryboardAnimationSprite)base.Source;

                public BeatmapBackgroundSpriteDrawNode(DrawableStoryboardAnimationSprite source)
                    : base(source)
                {
                }

                private Colour4 drawColourOffset;

                public override void ApplyState()
                {
                    base.ApplyState();

                    drawColourOffset = (Source as IColouredDimmable).DrawColourOffset;
                }

                private IUniformBuffer<DimParameters> dimParametersBuffer;

                protected override void BindUniformResources(IShader shader, IRenderer renderer)
                {
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
