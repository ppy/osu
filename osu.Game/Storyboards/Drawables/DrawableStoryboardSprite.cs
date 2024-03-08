// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Storyboards.Drawables
{
    public partial class DrawableStoryboardSprite : Sprite, IFlippable, IVectorScalable
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

        public DrawableStoryboardSprite(StoryboardSprite sprite)
        {
            Sprite = sprite;
            Origin = sprite.Origin;
            Position = sprite.InitialPosition;

            LifetimeStart = sprite.StartTime;
            LifetimeEnd = sprite.EndTimeForDisplay;
        }

        [BackgroundDependencyLoader]
        private void load(Storyboard storyboard)
        {
            if (storyboard.UseSkinSprites)
            {
                skin.SourceChanged += skinSourceChanged;
                skinSourceChanged();
            }
            else
                Texture = textureStore.Get(Sprite.Path);

            if (Sprite.Path == "SB/textbox.png")
                Debugger.Break();

            Sprite.ApplyTransforms(this);
        }

        private void skinSourceChanged()
        {
            Texture = skin.GetTexture(Sprite.Path) ?? textureStore.Get(Sprite.Path);

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
    }
}
