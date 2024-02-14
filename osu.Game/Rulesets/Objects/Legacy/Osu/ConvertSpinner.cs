// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Osu
{
    /// <summary>
    /// Legacy osu! Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : ConvertHitObject, IHasDuration, IHasPosition
    {
        public double Duration { get; set; }

        public double EndTime => StartTime + Duration;

        public Vector2 Position { get; set; }

        public float X => Position.X;

        public float Y => Position.Y;
    }
}
