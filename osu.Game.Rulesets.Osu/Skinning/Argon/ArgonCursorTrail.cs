// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonCursorTrail : CursorTrail
    {
        protected override float IntervalMultiplier => 0.4f;

        protected override float FadeExponent => 4;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get(@"Cursor/cursortrail");
            Scale = new Vector2(0.8f / Texture.ScaleAdjust);

            Blending = BlendingParameters.Additive;

            Alpha = 0.8f;
        }
    }
}
