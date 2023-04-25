// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Context;

namespace osu.Game.Beatmaps.Legacy;

public class LegacyContext : IContext
{
    public LegacyContext(double bpmMultiplier, bool generateTicks)
    {
        BpmMultiplier = bpmMultiplier;
        GenerateTicks = generateTicks;
    }

    /// <summary>
    /// Legacy BPM multiplier that introduces floating-point errors for rulesets that depend on it.
    /// DO NOT USE THIS UNLESS 100% SURE.
    /// </summary>
    public double BpmMultiplier { get; }

    /// <summary>
    /// Whether or not slider ticks should be generated at this control point.
    /// This exists for backwards compatibility with maps that abuse NaN slider velocity behavior on osu!stable (e.g. /b/2628991).
    /// </summary>
    public bool GenerateTicks { get; }

    public IContext Copy()
    {
        return new LegacyContext(BpmMultiplier, GenerateTicks);
    }
}
