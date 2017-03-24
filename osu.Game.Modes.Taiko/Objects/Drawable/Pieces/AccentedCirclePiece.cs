using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable.Pieces
{
    public class AccentedCirclePiece : CirclePiece
    {
        public AccentedCirclePiece(string symbolName)
            : base(symbolName)
        {
        }

        public override Vector2 Size => new Vector2(base.Size.X, base.Size.Y * 1.5f);
    }
}
