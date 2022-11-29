// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Skinning.Default
{
    internal partial class FruitPiece : CatchHitObjectPiece
    {
        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected override Drawable BorderPiece { get; }
        protected override Drawable HyperBorderPiece { get; }

        public FruitPiece()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new[]
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

            IndexInBeatmap.BindValueChanged(index =>
            {
                VisualRepresentation.Value = Fruit.GetVisualRepresentation(index.NewValue);
            }, true);
        }
    }
}
