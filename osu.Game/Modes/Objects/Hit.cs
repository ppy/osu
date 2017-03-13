using osu.Game.Modes.Objects.Types;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Objects
{
    internal class Hit : HitObject, IHasPosition, IHasCombo
    {
        public Vector2 Position { get; set; }
        public Color4 ComboColour { get; set; }

        public bool NewCombo { get; set; }
        public int ComboIndex { get; set; }
    }
}
