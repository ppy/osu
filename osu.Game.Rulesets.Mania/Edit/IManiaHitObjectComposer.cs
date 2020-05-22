// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public interface IManiaHitObjectComposer
    {
        ManiaPlayfield Playfield { get; }

        Vector2 ScreenSpacePositionAtTime(double time, Column column = null);
    }
}
