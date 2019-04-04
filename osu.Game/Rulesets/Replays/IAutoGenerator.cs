// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Replays;

namespace osu.Game.Rulesets.Replays
{
    public interface IAutoGenerator
    {
        Replay Generate();
    }
}
