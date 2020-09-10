using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Tau.Edit.Blueprints
{
    public class HardBeatSelectionBlueprint : TauSelectionBlueprint<HardBeat>
    {
        protected new DrawableHardBeat DrawableObject => (DrawableHardBeat)base.DrawableObject;

        protected readonly HitPiece SelectionPiece;

        public HardBeatSelectionBlueprint(DrawableHardBeat hitObject)
            : base(hitObject)
        {
            InternalChild = SelectionPiece = new HitPiece();
        }

        protected override void Update()
        {
            base.Update();

            SelectionPiece.Size = DrawableObject.DrawSize;
        }

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.Circle.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => DrawableObject.Circle.ReceivePositionalInputAt(screenSpacePos);

        public override Quad SelectionQuad => DrawableObject.Circle.ScreenSpaceDrawQuad.AABB;
    }
}
