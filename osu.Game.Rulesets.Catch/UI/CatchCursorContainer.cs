using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.UI {
    class CatchCursorContainer : GameplayCursorContainer
    {
        protected override Drawable CreateCursor() => new InvisibleCursor();

        private class InvisibleCursor : Drawable
        {

        }
    }
}
