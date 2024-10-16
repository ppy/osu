// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Changes
{
    public class InsertPathControlPointChange : IRevertableChange
    {
        public readonly IList<PathControlPoint> Target;

        public readonly int InsertionIndex;

        public readonly PathControlPoint Item;

        public InsertPathControlPointChange(IList<PathControlPoint> target, int insertionIndex, PathControlPoint item)
        {
            Target = target;
            InsertionIndex = insertionIndex;
            Item = item;
        }

        public void Apply() => Target.Insert(InsertionIndex, Item);

        public void Revert() => Target.RemoveAt(InsertionIndex);
    }
}
