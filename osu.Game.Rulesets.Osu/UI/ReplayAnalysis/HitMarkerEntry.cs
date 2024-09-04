// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;

namespace osu.Game.Rulesets.Osu.UI.ReplayAnalysis
{
    public partial class HitMarkerEntry : AimPointEntry
    {
        public bool IsLeftMarker { get; }

        public HitMarkerEntry(double lifetimeStart, Vector2 position, bool isLeftMarker)
            : base(lifetimeStart, position)
        {
            IsLeftMarker = isLeftMarker;
        }
    }
}
