// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    internal class FruitPiece : CatchHitObjectPiece
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (DrawableHitObject != null)
            {
                var fruit = (DrawableFruit)DrawableHitObject;
                VisualRepresentation.BindTo(fruit.VisualRepresentation);
            }

            VisualRepresentation.BindValueChanged(_ => recreateChildren(), true);
        }

        private void recreateChildren()
        {
            ClearInternal();

            AddInternal(getFruitFor(VisualRepresentation.Value));

            if (DrawableHitObject != null)
                AddInternal(BorderPiece = new BorderPiece());

            if (HyperDash.Value)
                AddInternal(HyperBorderPiece = new HyperBorderPiece());
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

                case FruitVisualRepresentation.Raspberry:
                    return new RaspberryPiece();
            }

            return Empty();
        }
    }
}
