// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        protected override void Update()
        {
            base.Update();

            // In stable, alpha transforms exceeding values of 1 would result in sprites disappearing from view.
            // See https://github.com/peppy/osu-stable-reference/blob/08e3dafd525934cf48880b08e91c24ce4ad8b761/osu!/Graphics/Sprites/pSprite.cs#L413-L414
            //
            // Over the years, storyboard(ers) have taken advantage of this to create "flicker" patterns.
            // This is quite a common technique, so we are reproducing it here for now.
            //
            // NOTE TO FUTURE VISITORS: If we do ever update the storyboard spec, we may want to move such flicker effects to their
            // own transform type, and make this a legacy behaviour. It feels very flimsy.
            if (Alpha > 1) Alpha %= 1;
        }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private TextureStore textureStore { get; set; } = null!;

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
        private void load(Storyboard storyboard)
        {
            if (storyboard.UseSkinSprites)
            {
                skin.SourceChanged += skinSourceChanged;
                skinSourceChanged();
            }
            else
                Texture = textureStore.Get(Sprite.Path, WrapMode.ClampToEdge, WrapMode.ClampToEdge);

            Sprite.ApplyTransforms(this);
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
    }
}
