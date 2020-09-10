using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Tau.Objects;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Tau.Edit.Blueprints
{
    public class BeatPlacementBlueprint : PlacementBlueprint
    {
        public new Beat HitObject => (Beat)base.HitObject;

        private readonly HitPiece piece;
        private readonly Box distance;

        public BeatPlacementBlueprint()
            : base(new Beat())
        {
            InternalChildren = new Drawable[]
            {
                piece = new HitPiece
                {
                    Size = new Vector2(16),
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                distance = new Box
                {
                    Colour = Color4.Yellow.Opacity(.5f),
                    RelativeSizeAxes = Axes.Y,
                    Height = .5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    RelativePositionAxes = Axes.Both,
                    Width = 5,
                    EdgeSmoothness = Vector2.One
                }
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);

                return true;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdatePosition(SnapResult result)
        {
            base.UpdatePosition(result);

            var angle = ScreenSpaceDrawQuad.Centre.GetDegreesFromPosition(result.ScreenSpacePosition);
            HitObject.Angle = angle;
            piece.Position = Extensions.GetCircularPosition(0.485f, angle);
            piece.Rotation = angle;
            distance.Rotation = angle;
        }
    }
}
