// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Screens.Edit.Changes
{
    public class InsertSampleChange : IRevertibleChange
    {
        public readonly IList<HitSampleInfo> Target;

        public readonly int InsertionIndex;

        public readonly HitSampleInfo Item;

        public InsertSampleChange(IList<HitSampleInfo> target, int insertionIndex, HitSampleInfo item)
        {
            Target = target;
            InsertionIndex = insertionIndex;
            Item = item;
        }

        public void Apply() => Target.Insert(InsertionIndex, Item);

        public void Revert() => Target.RemoveAt(InsertionIndex);
    }
}
