using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osuTK;

namespace Mvis.Plugin.FakeEditor.Editor
{
    public class DummyHitObjectComposer : HitObjectComposer
    {
        public override Playfield Playfield => new DummyPlayField();
        public override IEnumerable<DrawableHitObject> HitObjects => new List<DrawableHitObject>();
        public override bool CursorInPlacementArea => false;

        public override SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition)
            => new SnapResult(Vector2.Zero, null);

        public override float GetBeatSnapDistanceAt(HitObject referenceObject) => 0;

        public override float DurationToDistance(HitObject referenceObject, double duration) => 0;

        public override double DistanceToDuration(HitObject referenceObject, float distance) => 0;

        public override double GetSnappedDurationFromDistance(HitObject referenceObject, float distance) => 0;

        public override float GetSnappedDistanceFromDistance(HitObject referenceObject, float distance) => 0;
    }
}
