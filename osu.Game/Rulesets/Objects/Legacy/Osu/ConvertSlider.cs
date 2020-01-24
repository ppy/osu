// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Osu
{
    /// <summary>
    /// Legacy osu! Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSlider : Legacy.ConvertSlider, IHasPosition, IHasCombo
    {
        public Vector2 Position { get; set; }

        public float X => Position.X;

        public float Y => Position.Y;

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
