// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.UI.Cursor
{
    public partial class CatchCursorContainer : GameplayCursorContainer
    {
        protected override Drawable CreateCursor() => new CatchCursor();
    }
}
