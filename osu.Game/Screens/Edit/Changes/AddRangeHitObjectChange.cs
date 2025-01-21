// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Removes a collection of <see cref="HitObject"/>s from the provided <see cref="EditorBeatmap"/>.
    /// </summary>
    public class AddRangeHitObjectChange : CompositeChange
    {
        private readonly EditorBeatmap beatmap;
        private readonly HitObject[] hitObjects;

        public AddRangeHitObjectChange(EditorBeatmap beatmap, IEnumerable<HitObject> hitObjects)
        {
            this.beatmap = beatmap;
            this.hitObjects = hitObjects.ToArray();
        }

        protected override void SubmitChanges()
        {
            beatmap.BeginChange();
            foreach (var h in hitObjects)
                Submit(new AddHitObjectChange(beatmap, h));
            beatmap.EndChange();
        }
    }
}
