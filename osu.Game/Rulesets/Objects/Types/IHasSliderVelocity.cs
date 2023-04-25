// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types;

/// <summary>
/// A HitObject that has a slider velocity multiplier.
/// </summary>
public interface IHasSliderVelocity
{
    /// <summary>
    /// The slider velocity multiplier.
    /// </summary>
    double SliderVelocity { get; set; }
}
