// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSlider : Legacy.ConvertSlider, IHasPosition, IHasCombo
    {
        public float X => Position.X;

        public float Y => Position.Y;

        public Vector2 Position { get; set; }

        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        protected override void CopyFrom(HitObject other, IDictionary<object, object>? referenceLookup = null)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertSlider slider)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertSlider)}");

            Position = slider.Position;
            NewCombo = slider.NewCombo;
            ComboOffset = slider.ComboOffset;
        }

        protected override HitObject CreateInstance() => new ConvertSlider();
    }
}
