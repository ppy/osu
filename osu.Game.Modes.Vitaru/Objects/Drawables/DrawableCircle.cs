using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Vitaru.Objects.Drawables.Pieces;

namespace osu.Game.Modes.Vitaru.Objects.Drawables
{
    public class DrawableCircle : Container
    {
        private CirclePiece circlePiece;

        public float CircleWidth
        {
            get
            {
                return circlePiece.CircleWidth;
            }

            set
            {
                circlePiece.CircleWidth = value;
            }
        }
        public Color4 CircleColor
        {
            get
            {
                return circlePiece.CircleColor;
            }

            set
            {
                circlePiece.CircleColor = value;
            }
        }

        public DrawableCircle()
        {
            Children = new Drawable[]
            {
                circlePiece = new CirclePiece()
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };
        }

        public void setVisible(bool visible)
        {
            circlePiece.Alpha = visible ? 1 : 0;
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}
