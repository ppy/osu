// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public partial class CaughtObjectContainer
    {
        private class StackedObjectEntry : CaughtObjectEntry
        {
            /// <summary>
            /// The position of this object in relative to the catcher.
            /// </summary>
            public readonly Vector2 PositionInStack;

            public readonly DroppedObjectEntry DelayedDropEntry;

            public StackedObjectEntry(Vector2 positionInStack, DroppedObjectEntry delayedDropEntry, IHasCatchObjectState source)
                : base(source)
            {
                PositionInStack = positionInStack;
                DelayedDropEntry = delayedDropEntry;
            }

            public override void ApplyTransforms(Drawable d)
            {
                d.Position = PositionInStack;
            }
        }
    }
}
