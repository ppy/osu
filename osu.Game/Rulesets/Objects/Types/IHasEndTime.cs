// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Objects.Types
{
    [Obsolete("Use IHasDuration instead.")] // can be removed 20201126
    public interface IHasEndTime : IHasDuration
    {
    }
}
