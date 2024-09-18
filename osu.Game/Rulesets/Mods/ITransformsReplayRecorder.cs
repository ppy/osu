// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;

namespace osu.Game.Rulesets.Mods
{
    public interface ITransformsReplayRecorder : IApplicableMod
    {
        Func<Vector2, Vector2>? TransformMouseInput { get; set; }
    }
}
