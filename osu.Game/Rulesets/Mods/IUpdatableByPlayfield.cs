﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public interface IUpdatableByPlayfield : IApplicableMod
    {
        void Update(Playfield playfield);
    }
}
