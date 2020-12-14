// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    internal class FruitPiece : CatchHitObjectPiece
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected override BorderPiece BorderPiece { get; }
        protected override HyperBorderPiece HyperBorderPiece { get; }

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

            var fruitState = (IHasFruitState)ObjectState;
            VisualRepresentation.BindTo(fruitState.VisualRepresentation);
        }
    }
}
