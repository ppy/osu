// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Performance;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class AimPointEntry : LifetimeEntry
    {
        public Vector2 Position { get; }

        public AimPointEntry(double time, Vector2 position)
        {
            LifetimeStart = time;
            LifetimeEnd = time + 1_000;
            Position = position;
        }
    }
}
