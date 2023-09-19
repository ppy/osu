// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Legacy
{
    public partial class LegacyBananaPiece : LegacyCatchHitObjectPiece
    {
        private static readonly Vector2 banana_max_size = new Vector2(128);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Texture? texture = Skin.GetTextureWithMaxSize("fruit-bananas", banana_max_size);
            Texture? overlayTexture = Skin.GetTextureWithMaxSize("fruit-bananas-overlay", banana_max_size);

            SetTexture(texture, overlayTexture);
        }
    }
}
