// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class ExplodePiece : Container
    {
        [Resolved]
        private DrawableHitObject drawableObject { get; set; }

        private TrianglesPiece triangles;

        public ExplodePiece()
        {
            Size = new Vector2(OsuHitObject.OBJECT_RADIUS * 2);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = triangles = new TrianglesPiece
            {
                Blending = BlendingParameters.Additive,
                RelativeSizeAxes = Axes.Both,
                Alpha = 0.2f,
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
