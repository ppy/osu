// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        public DrawableFruit(Fruit h)
            : base(h)
        {
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

        protected override void UpdateComboColour(Color4 proposedColour, IReadOnlyList<Color4> comboColours)
        {
            // ignore the incoming combo colour as we use a custom lookup
            AccentColour.Value = comboColours[(HitObject.IndexInBeatmap + 1) % comboColours.Count];
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
