// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Types;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Objects
{
    internal class Slider : HitObject, IHasCurve, IHasDistance, IHasPosition, IHasCombo, IHasRepeats
    {
        public List<Vector2> ControlPoints { get; set; }
        public CurveType CurveType { get; set; }

        public double Distance { get; set; }

        public Vector2 Position { get; set; }

        public Color4 ComboColour { get; set; }
        public bool NewCombo { get; set; }
        public int ComboIndex { get; set; }

        public int RepeatCount { get; set; }
    }
}
