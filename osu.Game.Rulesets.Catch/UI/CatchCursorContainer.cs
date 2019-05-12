// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
