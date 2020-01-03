// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public interface IManiaHitObjectComposer
    {
        Column ColumnAt(Vector2 screenSpacePosition);

        int TotalColumns { get; }
    }
}
