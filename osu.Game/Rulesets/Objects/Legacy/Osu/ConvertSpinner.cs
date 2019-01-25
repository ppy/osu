﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Osu
{
    /// <summary>
    /// Legacy osu! Spinner-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSpinner : HitObject, IHasEndTime, IHasPosition, IHasCombo
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;

        public Vector2 Position { get; set; }

        public float X => Position.X;

        public float Y => Position.Y;

        protected override HitWindows CreateHitWindows() => null;

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }
    }
}
