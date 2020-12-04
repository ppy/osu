// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class CirclePiece : CompositeDrawable
    {
        [Resolved]
        private DrawableHitObject drawableObject { get; set; }

        private TrianglesPiece triangles;

        public CirclePiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);
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

            if (drawableObject != null)
                drawableObject.HitObjectApplied -= onHitObjectApplied;
        }
    }
}
