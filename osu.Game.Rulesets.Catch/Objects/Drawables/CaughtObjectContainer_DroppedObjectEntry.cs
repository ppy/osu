// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public partial class CaughtObjectContainer
    {
        private class DroppedObjectEntry : CaughtObjectEntry
        {
            public DroppedObjectAnimation Animation;

            /// <summary>
            /// The initial position of the dropped object.
            /// </summary>
            public Vector2 DropPosition;

            /// <summary>
            /// 1 or -1 representing visual mirroring of the object.
            /// </summary>
            public int MirrorDirection = 1;

            private readonly Vector2 positionInStack;

            public DroppedObjectEntry(Vector2 positionInStack, IHasCatchObjectState source)
                : base(source)
            {
                this.positionInStack = positionInStack;
            }

            public override void ApplyTransforms(Drawable d)
            {
                d.Position = DropPosition;
                d.Scale *= new Vector2(MirrorDirection, 1);

                using (d.BeginAbsoluteSequence(LifetimeStart))
                {
                    switch (Animation)
                    {
                        case DroppedObjectAnimation.Explode:
                            var xMovement = positionInStack.X * MirrorDirection * 6;
                            d.MoveToY(d.Y - 50, 250, Easing.OutSine).Then().MoveToY(d.Y + 50, 500, Easing.InSine);
                            d.MoveToX(d.X + xMovement, 1000);
                            d.FadeOut(750);
                            break;

                        case DroppedObjectAnimation.Drop:
                            d.MoveToY(d.Y + 75, 750, Easing.InSine);
                            d.FadeOut(750);
                            break;
                    }
                }
            }
        }
    }
}
