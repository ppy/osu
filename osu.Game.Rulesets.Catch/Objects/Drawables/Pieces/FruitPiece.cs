// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    internal class FruitPiece : CompositeDrawable
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

        private BorderPiece border;
        private PalpableCatchHitObject hitObject;

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            var drawableCatchObject = (DrawablePalpableCatchHitObject)drawableObject;
            hitObject = drawableCatchObject.HitObject;

            AddRangeInternal(new[]
            {
                getFruitFor(hitObject.VisualRepresentation),
                border = new BorderPiece(),
            });

            if (hitObject.HyperDash)
            {
                AddInternal(new HyperBorderPiece());
            }
        }

        protected override void Update()
        {
            base.Update();
            border.Alpha = (float)Math.Clamp((hitObject.StartTime - Time.Current) / 500, 0, 1);
        }

        private Drawable getFruitFor(FruitVisualRepresentation representation)
        {
            switch (representation)
            {
                case FruitVisualRepresentation.Pear:
                    return new PearPiece();

                case FruitVisualRepresentation.Grape:
                    return new GrapePiece();

                case FruitVisualRepresentation.Pineapple:
                    return new PineapplePiece();

                case FruitVisualRepresentation.Banana:
                    return new BananaPiece();

                case FruitVisualRepresentation.Raspberry:
                    return new RaspberryPiece();
            }

            return Empty();
        }
    }
}
