// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public partial class EditorStage : Stage
    {
        public EditorStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction columnStartAction)
            : base(firstColumnIndex, definition, ref columnStartAction)
        {
        }

        protected override Column CreateColumn(int index, bool isSpecial) => new EditorColumn(index, isSpecial);
    }
}
