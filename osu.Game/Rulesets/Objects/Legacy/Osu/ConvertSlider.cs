// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Osu
{
    /// <summary>
    /// Legacy osu! Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSlider : Legacy.ConvertSlider, IHasPosition, IHasCombo, IHasGenerateTicks
    {
        public Vector2 Position { get; set; }

        public float X => Position.X;

        public float Y => Position.Y;

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        public bool GenerateTicks { get; set; } = true;

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertSlider convertSlider)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertSlider)}");

            Position = convertSlider.Position;
            NewCombo = convertSlider.NewCombo;
            ComboOffset = convertSlider.ComboOffset;
            GenerateTicks = convertSlider.GenerateTicks;
        }

        protected override HitObject CreateInstance() => new ConvertSlider();
    }
}
