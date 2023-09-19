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
                    SetTexture(Skin.GetTextureWithMaxSize("fruit-pear", fruit_max_size), Skin.GetTextureWithMaxSize("fruit-pear-overlay", fruit_max_size));
                    break;

                case FruitVisualRepresentation.Grape:
                    SetTexture(Skin.GetTextureWithMaxSize("fruit-grapes", fruit_max_size), Skin.GetTextureWithMaxSize("fruit-grapes-overlay", fruit_max_size));
                    break;

                case FruitVisualRepresentation.Pineapple:
                    SetTexture(Skin.GetTextureWithMaxSize("fruit-apple", fruit_max_size), Skin.GetTextureWithMaxSize("fruit-apple-overlay", fruit_max_size));
                    break;

                case FruitVisualRepresentation.Raspberry:
                    SetTexture(Skin.GetTextureWithMaxSize("fruit-orange", fruit_max_size), Skin.GetTextureWithMaxSize("fruit-orange-overlay", fruit_max_size));
                    break;
            }
        }
    }
}
