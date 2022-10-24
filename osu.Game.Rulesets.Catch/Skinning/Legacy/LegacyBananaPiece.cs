// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public class LegacyBananaPiece : LegacyCatchHitObjectPiece
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Texture texture = Skin.GetTexture("fruit-bananas");
            Texture overlayTexture = Skin.GetTexture("fruit-bananas-overlay");

            SetTexture(texture, overlayTexture);
        }
    }
}
