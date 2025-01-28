// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    internal partial class LegacyFruitPiece : LegacyCatchHitObjectPiece
    {
        private static readonly Vector2 fruit_max_size = new Vector2(160);

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
                    setTextures("pear");
                    break;

                case FruitVisualRepresentation.Grape:
                    setTextures("grapes");
                    break;

                case FruitVisualRepresentation.Pineapple:
                    setTextures("apple");
                    break;

                case FruitVisualRepresentation.Raspberry:
                    setTextures("orange");
                    break;
            }

            void setTextures(string fruitName) => SetTexture(
                Skin.GetTexture($"fruit-{fruitName}")?.WithMaximumSize(fruit_max_size),
                Skin.GetTexture($"fruit-{fruitName}-overlay")?.WithMaximumSize(fruit_max_size)
            );
        }
    }
}
