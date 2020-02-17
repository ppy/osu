// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableFruit : PalpableCatchHitObject<Fruit>
    {
        private Container scaleContainer;

        /// <summary>
        /// Because we're adding a border around the fruit, we need to scale down some.
        /// </summary>
        public const float RADIUS_ADJUST = 1.1f;

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

            AccentColour.Value = colourForRepresentation(HitObject.VisualRepresentation);

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

        private Color4 colourForRepresentation(FruitVisualRepresentation representation)
        {
            switch (representation)
            {
                default:
                case FruitVisualRepresentation.Pear:
                    return new Color4(17, 136, 170, 255);

                case FruitVisualRepresentation.Grape:
                    return new Color4(204, 102, 0, 255);

                case FruitVisualRepresentation.Raspberry:
                    return new Color4(121, 9, 13, 255);

                case FruitVisualRepresentation.Pineapple:
                    return new Color4(102, 136, 0, 255);

                case FruitVisualRepresentation.Banana:
                    switch (RNG.Next(0, 3))
                    {
                        default:
                            return new Color4(255, 240, 0, 255);

                        case 1:
                            return new Color4(255, 192, 0, 255);

                        case 2:
                            return new Color4(214, 221, 28, 255);
                    }
            }
        }
    }
}
