using osu.Game.Modes.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Objects
{
    internal class HitObjectWithCombo : HitObject, IHasCombo
    {
        public Color4 ComboColour { get; set; }
        public bool NewCombo { get; set; }
        public int ComboIndex { get; set; }
    }
}
