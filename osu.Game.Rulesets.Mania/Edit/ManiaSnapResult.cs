// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.UI;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaSnapResult : SnapResult
    {
        public readonly Column Column;

        public ManiaSnapResult(Vector2 screenSpacePosition, double time, Column column)
            : base(screenSpacePosition, time)
        {
            Column = column;
        }
    }
}
