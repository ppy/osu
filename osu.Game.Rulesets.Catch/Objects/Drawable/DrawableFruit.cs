// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableFruit : PalpableCatchHitObject<Fruit>
    {
        private Container scaleContainer;

        public DrawableFruit(Fruit h)
            : base(h)
        {
            Origin = Anchor.Centre;

            Size = new Vector2(CatchHitObject.OBJECT_RADIUS * 2);

            Masking = false;

            Rotation = (float)(RNG.NextDouble() - 0.5f) * 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Framework.Graphics.Drawable[]
            {
                scaleContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        new SkinnableDrawable(
                            new CatchSkinComponent(getComponent(HitObject.VisualRepresentation)), _ => new FruitPiece())
                    }
                }
            });

            scaleContainer.Scale = new Vector2(HitObject.Scale);
        }

        private CatchSkinComponents getComponent(FruitVisualRepresentation hitObjectVisualRepresentation)
        {
            switch (hitObjectVisualRepresentation)
            {
                case FruitVisualRepresentation.Pear:
                    return CatchSkinComponents.FruitPear;

                case FruitVisualRepresentation.Grape:
                    return CatchSkinComponents.FruitGrapes;

                case FruitVisualRepresentation.Raspberry:
                    return CatchSkinComponents.FruitOrange;

                case FruitVisualRepresentation.Pineapple:
                    return CatchSkinComponents.FruitApple;

                case FruitVisualRepresentation.Banana:
                    return CatchSkinComponents.FruitBananas;

                default:
                    throw new ArgumentOutOfRangeException(nameof(hitObjectVisualRepresentation), hitObjectVisualRepresentation, null);
            }
        }
    }
}
