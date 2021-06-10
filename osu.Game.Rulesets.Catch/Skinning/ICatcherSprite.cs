// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Textures;

namespace osu.Game.Rulesets.Catch.Skinning
{
    public interface ICatcherSprite
    {
        Texture CurrentTexture { get; }
    }
}
