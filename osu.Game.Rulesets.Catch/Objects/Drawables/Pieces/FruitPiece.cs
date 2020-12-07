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

            InternalChildren = new Drawable[]
            {
                new FruitPulpFormation
                {
                    AccentColour = { BindTarget = AccentColour },
                    VisualRepresentation = { BindTarget = VisualRepresentation }
                },
                BorderPiece = new BorderPiece(),
                HyperBorderPiece = new HyperBorderPiece()
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (DrawableHitObject != null)
            {
                var fruit = (DrawableFruit)DrawableHitObject;
                VisualRepresentation.BindTo(fruit.VisualRepresentation);
            }
        }
    }
}
