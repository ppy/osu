// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    internal class LegacyFruitPiece : LegacyCatchHitObjectPiece
    {
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
                    SetTexture(Skin.GetTexture("fruit-pear"), Skin.GetTexture("fruit-pear-overlay"));
                    break;

                case FruitVisualRepresentation.Grape:
                    SetTexture(Skin.GetTexture("fruit-grapes"), Skin.GetTexture("fruit-grapes-overlay"));
                    break;

                case FruitVisualRepresentation.Pineapple:
                    SetTexture(Skin.GetTexture("fruit-apple"), Skin.GetTexture("fruit-apple-overlay"));
                    break;

                case FruitVisualRepresentation.Raspberry:
                    SetTexture(Skin.GetTexture("fruit-orange"), Skin.GetTexture("fruit-orange-overlay"));
                    break;
            }
        }
    }
}
