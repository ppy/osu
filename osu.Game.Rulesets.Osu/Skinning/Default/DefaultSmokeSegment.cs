// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class DefaultSmokeSegment : SmokeSegment
    {
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            // ISkinSource doesn't currently fallback to global textures.
            // We might want to change this in the future if the intention is to allow the user to skin this as per legacy skins.
            Texture = textures.Get("Gameplay/osu/cursor-smoke");
        }
    }
}
