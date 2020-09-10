using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Tau.Objects;

namespace osu.Game.Rulesets.Tau.Edit.Blueprints
{
    public class TauSelectionBlueprint<T> : OverlaySelectionBlueprint
        where T : TauHitObject
    {
        protected new T HitObject => (T)DrawableObject.HitObject;

        protected override bool AlwaysShowWhenSelected => true;

        protected override bool ShouldBeAlive =>
            (DrawableObject.IsAlive && DrawableObject.IsPresent) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

        protected TauSelectionBlueprint(DrawableHitObject drawableObject)
            : base(drawableObject)
        {
        }
    }
}
