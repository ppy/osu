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
