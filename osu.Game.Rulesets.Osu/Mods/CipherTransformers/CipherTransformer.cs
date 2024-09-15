// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public abstract class CipherTransformer
    {
        public abstract Vector2 Transform(Vector2 mousePosition);
    }
}
