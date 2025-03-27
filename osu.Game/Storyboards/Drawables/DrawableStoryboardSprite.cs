// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardSprite : Sprite, IFlippable, IVectorScalable, IColouredDimmable
    {
        public StoryboardSprite Sprite { get; }

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

        public override bool RemoveWhenNotAlive => false;

        protected override Vector2 DrawScale
            => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y) * VectorScale;

        public override Anchor Origin => StoryboardExtensions.AdjustOrigin(base.Origin, VectorScale, FlipH, FlipV);

        public override bool IsPresent
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && base.IsPresent;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private TextureStore textureStore { get; set; } = null!;

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            bool result = base.OnInvalidate(invalidation, source);

            if ((invalidation & Invalidation.Colour) > 0)
            {
                result |= Invalidate(Invalidation.DrawNode);
            }

            return result;
        }

        public DrawableStoryboardSprite(StoryboardSprite sprite)
        {
            Sprite = sprite;
            Origin = sprite.Origin;
            Position = sprite.InitialPosition;
            Name = sprite.Path;

            LifetimeStart = sprite.StartTime;
            LifetimeEnd = sprite.EndTimeForDisplay;
        }

        [BackgroundDependencyLoader]
        private void load(Storyboard storyboard, ShaderManager shaders)
        {
            if (storyboard.UseSkinSprites)
            {
                skin.SourceChanged += skinSourceChanged;
                skinSourceChanged();
            }
            else
                Texture = textureStore.Get(Sprite.Path, WrapMode.ClampToEdge, WrapMode.ClampToEdge);

            Sprite.ApplyTransforms(this);

            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "ColouredDimmableTexture");
        }

        private void skinSourceChanged()
        {
            Texture = skin.GetTexture(Sprite.Path, WrapMode.ClampToEdge, WrapMode.ClampToEdge) ??
                      textureStore.Get(Sprite.Path, WrapMode.ClampToEdge, WrapMode.ClampToEdge);

            // Setting texture will only update the size if it's zero.
            // So let's force an explicit update.
            Size = new Vector2(Texture?.DisplayWidth ?? 0, Texture?.DisplayHeight ?? 0);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (skin.IsNotNull())
                skin.SourceChanged -= skinSourceChanged;
        }

        protected override DrawNode CreateDrawNode() => new DrawableStoryboardSpriteDrawNode(this);

        public class DrawableStoryboardSpriteDrawNode : SpriteDrawNode
        {
            public new DrawableStoryboardSprite Source => (DrawableStoryboardSprite)base.Source;

            public DrawableStoryboardSpriteDrawNode(DrawableStoryboardSprite source)
                : base(source)
            {
            }

            private Colour4 drawColourOffset;

            public override void ApplyState()
            {
                base.ApplyState();

                drawColourOffset = (Source as IColouredDimmable).DrawColourOffset;
            }

            private IUniformBuffer<DimParameters> dimParametersBuffer = null!;

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
