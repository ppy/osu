// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditStage : ManiaStage
    {
        private readonly SnapLinePlayfield snapLinePlayfield;

        public ManiaEditStage(int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            : base(firstColumnIndex, definition, ref normalColumnStartAction, ref specialColumnStartAction)
        {
            Add(snapLinePlayfield = new SnapLinePlayfield());
            AddNested(snapLinePlayfield);

            snapLinePlayfield.VisibleTimeRange.BindTo(VisibleTimeRange);
        }

        public void Add(EditSnapLine editSnapLine) => snapLinePlayfield.Add(new DrawableEditSnapLine(editSnapLine));
        public void Remove(DrawableEditSnapLine editSnapLine) => snapLinePlayfield.Remove(editSnapLine);
        public void ClearEditSnapLines() => snapLinePlayfield.Clear();
    }
}
