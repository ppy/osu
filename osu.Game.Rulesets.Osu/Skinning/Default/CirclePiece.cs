// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class CirclePiece : CompositeDrawable
    {
        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        private TrianglesPiece triangles = null!;

        public CirclePiece()
        {
            Size = OsuHitObject.OBJECT_DIMENSIONS;
            Masking = true;

            CornerRadius = Size.X / 2;
            CornerExponent = 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = textures.Get(@"Gameplay/osu/disc"),
                },
                new KiaiFlash
                {
                    RelativeSizeAxes = Axes.Both,
                },
                triangles = new TrianglesPiece
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.5f,
                }
            };

            drawableObject.HitObjectApplied += onHitObjectApplied;
            onHitObjectApplied(drawableObject);
        }

        private void onHitObjectApplied(DrawableHitObject obj)
        {
            if (obj.HitObject == null)
                return;

            triangles.Reset((int)obj.HitObject.StartTime);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject.IsNotNull())
                drawableObject.HitObjectApplied -= onHitObjectApplied;
        }
    }
}
