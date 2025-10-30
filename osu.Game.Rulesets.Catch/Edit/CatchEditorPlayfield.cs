// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.UI;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchEditorPlayfield : CatchPlayfield
    {
        public CatchEditorPlayfield(IBeatmapDifficultyInfo difficulty)
            : base(difficulty)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // TODO: honor "hit animation" setting?
            Catcher.CatchFruitOnPlate = false;

            // TODO: disable hit lighting as well
        }
    }
}
