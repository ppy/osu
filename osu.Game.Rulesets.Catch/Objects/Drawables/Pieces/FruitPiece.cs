// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();
        public readonly Bindable<bool> HyperDash = new Bindable<bool>();

        [CanBeNull]
        private DrawableCatchHitObject drawableHitObject;

        [CanBeNull]
        private BorderPiece borderPiece;

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load([CanBeNull] DrawableHitObject drawable)
        {
            drawableHitObject = (DrawableCatchHitObject)drawable;

            AddInternal(getFruitFor(VisualRepresentation.Value));

            // if it is not part of a DHO, the border is always invisible.
            if (drawableHitObject != null)
                AddInternal(borderPiece = new BorderPiece());

            if (HyperDash.Value)
                AddInternal(new HyperBorderPiece());
        }

        protected override void Update()
        {
            if (borderPiece != null && drawableHitObject?.HitObject != null)
                borderPiece.Alpha = (float)Math.Clamp((drawableHitObject.HitObject.StartTime - Time.Current) / 500, 0, 1);
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
