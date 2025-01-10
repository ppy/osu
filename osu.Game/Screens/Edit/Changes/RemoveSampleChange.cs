// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Audio;

namespace osu.Game.Screens.Edit.Changes
{
    public class RemoveSampleChange : IRevertibleChange
    {
        public readonly IList<HitSampleInfo> Target;

        public readonly int Index;

        public readonly HitSampleInfo Item;

        public RemoveSampleChange(IList<HitSampleInfo> target, int index)
        {
            Target = target;
            Index = index;
            Item = target[index];
        }

        public RemoveSampleChange(IList<HitSampleInfo> target, HitSampleInfo item)
        {
            Target = target;
            Index = target.IndexOf(item);
            Item = item;
        }

        public void Apply() => Target.RemoveAt(Index);

        public void Revert() => Target.Insert(Index, Item);
    }
}
