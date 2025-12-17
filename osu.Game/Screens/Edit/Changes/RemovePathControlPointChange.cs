// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    public class RemovePathControlPointChange : IRevertibleChange
    {
        public readonly IList<PathControlPoint> Target;

        public readonly int Index;

        public readonly PathControlPoint Item;

        public RemovePathControlPointChange(IList<PathControlPoint> target, int index)
        {
            Target = target;
            Index = index;
            Item = target[index];
        }

        public RemovePathControlPointChange(IList<PathControlPoint> target, PathControlPoint item)
        {
            Target = target;
            Index = target.IndexOf(item);
            Item = item;
        }

        public void Apply() => Target.RemoveAt(Index);

        public void Revert() => Target.Insert(Index, Item);
    }
}
