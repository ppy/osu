// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class AnalysisFrameEntry : LifetimeEntry
    {
        public OsuAction[] Action { get; }

        public Vector2 Position { get; }

        public AnalysisFrameEntry(double time, double displayLength, Vector2 position, params OsuAction[] action)
        {
            LifetimeStart = time;
            LifetimeEnd = time + displayLength;
            Position = position;
            Action = action;
        }
    }
}
