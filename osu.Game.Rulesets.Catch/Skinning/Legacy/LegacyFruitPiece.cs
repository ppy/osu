// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    internal partial class LegacyFruitPiece : LegacyCatchHitObjectPiece
    {
        private static readonly Vector2 fruit_max_size = new Vector2(128);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IndexInBeatmap.BindValueChanged(index =>
            {
                setTexture(Fruit.GetVisualRepresentation(index.NewValue));
            }, true);
        }

        private void setTexture(FruitVisualRepresentation visualRepresentation)
        {
            switch (visualRepresentation)
            {
                case FruitVisualRepresentation.Pear:
                    SetTexture(Skin.GetTexture("fruit-pear")?.WithMaximumSize(fruit_max_size), Skin.GetTexture("fruit-pear-overlay")?.WithMaximumSize(fruit_max_size));
                    break;

                case FruitVisualRepresentation.Grape:
                    SetTexture(Skin.GetTexture("fruit-grapes")?.WithMaximumSize(fruit_max_size), Skin.GetTexture("fruit-grapes-overlay")?.WithMaximumSize(fruit_max_size));
                    break;

                case FruitVisualRepresentation.Pineapple:
                    SetTexture(Skin.GetTexture("fruit-apple")?.WithMaximumSize(fruit_max_size), Skin.GetTexture("fruit-apple-overlay")?.WithMaximumSize(fruit_max_size));
                    break;

                case FruitVisualRepresentation.Raspberry:
                    SetTexture(Skin.GetTexture("fruit-orange")?.WithMaximumSize(fruit_max_size), Skin.GetTexture("fruit-orange-overlay")?.WithMaximumSize(fruit_max_size));
                    break;
            }
        }
    }
}
