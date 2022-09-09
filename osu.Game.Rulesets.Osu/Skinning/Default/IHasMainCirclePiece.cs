// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public interface IHasMainCirclePiece
    {
        SkinnableDrawable CirclePiece { get; }
    }
}
