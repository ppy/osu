// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public interface IHasComboColours
    {
        List<Color4> ComboColours { get; set; }
    }
}
