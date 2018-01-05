﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Objects.Legacy.Catch
{
    /// <summary>
    /// Legacy osu!catch Slider-type, used for parsing Beatmaps.
    /// </summary>
    internal sealed class ConvertSlider : Legacy.ConvertSlider, IHasXPosition, IHasCombo
    {
        public float X { get; set; }

        public bool NewCombo { get; set; }
    }
}
