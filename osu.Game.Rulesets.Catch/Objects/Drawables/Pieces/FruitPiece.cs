// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

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

        public BorderPiece Border { get; private set; }

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new[]
            {
                getFruitFor(VisualRepresentation.Value),
                Border = new BorderPiece(),
            });

            if (HyperDash.Value)
            {
                AddInternal(new HyperBorderPiece());
            }
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
