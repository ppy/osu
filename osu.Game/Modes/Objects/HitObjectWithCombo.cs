// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;
using OpenTK.Graphics;

namespace osu.Game.Modes.Objects
{
    internal class HitObjectWithCombo : HitObject, IHasCombo
    {
        public Color4 ComboColour { get; set; }
        public bool NewCombo { get; set; }
        public int ComboIndex { get; set; }
    }
}
