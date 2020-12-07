// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Catch.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Skinning
{
    internal class LegacyFruitPiece : LegacyCatchHitObjectPiece
    {
        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        private readonly string[] lookupNames =
        {
            "fruit-pear", "fruit-grapes", "fruit-apple", "fruit-orange"
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var fruit = (DrawableFruit)DrawableHitObject;

            if (fruit != null)
                VisualRepresentation.BindTo(fruit.VisualRepresentation);

            VisualRepresentation.BindValueChanged(visual => setTexture(visual.NewValue), true);
        }

        private void setTexture(FruitVisualRepresentation visualRepresentation)
        {
            Texture texture = Skin.GetTexture(lookupNames[(int)visualRepresentation]);
            Texture overlayTexture = Skin.GetTexture(lookupNames[(int)visualRepresentation] + "-overlay");

            SetTexture(texture, overlayTexture);
        }
    }
}
