// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Utils;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableFruit : PalpableCatchHitObject<Fruit>
    {
        public DrawableFruit(Fruit h)
            : base(h)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScaleContainer.Child = new SkinnableDrawable(
                new CatchSkinComponent(getComponent(HitObject.VisualRepresentation)), _ => new FruitPiece());

            ScaleContainer.Rotation = (float)(RNG.NextDouble() - 0.5f) * 40;
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
}
