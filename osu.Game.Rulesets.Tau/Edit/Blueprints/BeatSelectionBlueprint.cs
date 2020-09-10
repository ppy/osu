using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.Edit.Blueprints
{
    public class BeatSelectionBlueprint : TauSelectionBlueprint<Beat>
    {
        protected new DrawableBeat DrawableObject => (DrawableBeat)base.DrawableObject;

        protected readonly HitPiece SelectionPiece;
        protected readonly Box Distance;

        public BeatSelectionBlueprint(DrawableBeat hitObject)
            : base(hitObject)
        {
            InternalChildren = new Drawable[]
            {
                SelectionPiece = new HitPiece(),
                Distance = new Box
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

        protected override void Update()
        {
            base.Update();

            SelectionPiece.Rotation = DrawableObject.Rotation;
            SelectionPiece.Position = Extensions.GetCircularPosition(-DrawableObject.Box.Y, DrawableObject.Rotation);

            Distance.Rotation = DrawableObject.Rotation;
            Distance.Height = -DrawableObject.Box.Y;
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.Box.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.Box.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => DrawableObject.Box.ScreenSpaceDrawQuad.AABB;
    }
}
