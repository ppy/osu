﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using OpenTK;

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
    }
}
