// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableFruit : DrawablePalpableCatchHitObject
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected virtual FruitVisualRepresentation GetVisualRepresentation(int indexInBeatmap) => (FruitVisualRepresentation)(indexInBeatmap % 4);

        public DrawableFruit()
            : this(null)
        {
        }

        public DrawableFruit([CanBeNull] Fruit h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            IndexInBeatmap.BindValueChanged(change =>
            {
                VisualRepresentation.Value = GetVisualRepresentation(change.NewValue);
            }, true);

            VisualRepresentation.BindValueChanged(_ => updatePiece());
            HyperDash.BindValueChanged(_ => updatePiece(), true);
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();

            ScaleContainer.RotateTo((RandomSingle(1) - 0.5f) * 40);
        }

        private void updatePiece()
        {
            ScaleContainer.Child = new SkinnableDrawable(
                new CatchSkinComponent(getComponent(VisualRepresentation.Value)),
                _ => new FruitPiece
                {
                    VisualRepresentation = { BindTarget = VisualRepresentation },
                    HyperDash = { BindTarget = HyperDash },
                });
        }

        private CatchSkinComponents getComponent(FruitVisualRepresentation hitObjectVisualRepresentation)
        {
            switch (hitObjectVisualRepresentation)
            {
                case FruitVisualRepresentation.Pear:
                    return CatchSkinComponents.FruitPear;

                case FruitVisualRepresentation.Grape:
                    return CatchSkinComponents.FruitGrapes;

                case FruitVisualRepresentation.Pineapple:
                    return CatchSkinComponents.FruitApple;

                case FruitVisualRepresentation.Raspberry:
                    return CatchSkinComponents.FruitOrange;

                case FruitVisualRepresentation.Banana:
                    return CatchSkinComponents.FruitBananas;

                default:
                    throw new ArgumentOutOfRangeException(nameof(hitObjectVisualRepresentation), hitObjectVisualRepresentation, null);
            }
        }
    }

    public enum FruitVisualRepresentation
    {
        Pear,
        Grape,
        Pineapple,
        Raspberry,
        Banana // banananananannaanana
    }
}
