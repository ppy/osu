// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuEditPlayfield : OsuPlayfield
    {
        protected override CursorContainer CreateCursor() => null;

        protected override bool ProxyApproachCircles => false;
    }
}
