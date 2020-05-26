// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public interface IManiaHitObjectComposer
    {
        ManiaPlayfield Playfield { get; }
    }
}
