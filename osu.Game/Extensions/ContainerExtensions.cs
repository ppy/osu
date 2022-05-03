// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Extensions
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Prevents this container's children from transforming when the container transforms.
        /// </summary>
        /// <param name="target">The target to prevent its children from transforming.</param>
        /// <param name="parent">The parent of the target.</param>
        public static void KeepChildrenUpright(this Container target, Container parent)
        {
            float parentRotation = parent.Rotation;
            Vector2 parentScale = parent.Scale;

            foreach (Drawable child in target)
            {
                child.RotateTo(-parentRotation);

                // Checking rotations idea taken from https://github.com/ppy/osu/pull/15347/files
                int parentRotationInQuarterTurns = (int)Math.Floor(parentRotation / 90);
                if (parentRotationInQuarterTurns % 2 != 0)
                    child.ScaleTo(new Vector2(parentScale.Y, parentScale.X));
                else
                    child.ScaleTo(parentScale);

                int parentRotationInQuarterTurnsModFour = parentRotationInQuarterTurns % 4;
                if (parentRotationInQuarterTurnsModFour < 0) parentRotationInQuarterTurnsModFour += 4;

                switch (parentRotationInQuarterTurnsModFour)
                {
                    // Rotated by 90
                    case 1:
                        child.Origin = rotateAnchor90(child.Origin, true);
                        break;

                    // Rotated by 180
                    case 2:
                        child.Origin = flipBothAnchors(child.Origin);
                        break;

                    // Rotated by 270
                    case 3:
                        child.Origin = rotateAnchor90(child.Origin, false);
                        break;

                    default:
                        break;
                }
            }
        }

        private static Anchor rotateAnchor90(Anchor anchor, bool clockwise)
        {
            Anchor rotatedAnchor;

            switch (anchor)
            {
                case Anchor.TopLeft:
                    rotatedAnchor = Anchor.TopRight;
                    break;

                case Anchor.TopCentre:
                    rotatedAnchor = Anchor.CentreRight;
                    break;

                case Anchor.TopRight:
                    rotatedAnchor = Anchor.BottomRight;
                    break;

                case Anchor.CentreLeft:
                    rotatedAnchor = Anchor.TopCentre;
                    break;

                case Anchor.Centre:
                    rotatedAnchor = Anchor.Centre;
                    break;

                case Anchor.CentreRight:
                    rotatedAnchor = Anchor.BottomCentre;
                    break;

                case Anchor.BottomLeft:
                    rotatedAnchor = Anchor.TopLeft;
                    break;

                case Anchor.BottomCentre:
                    rotatedAnchor = Anchor.CentreLeft;
                    break;

                case Anchor.BottomRight:
                    rotatedAnchor = Anchor.BottomLeft;
                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            if (!clockwise)
            {
                // Correct if rotating by 270 instead of 90
                rotatedAnchor = flipBothAnchors(rotatedAnchor);
            }

            return rotatedAnchor;
        }

        private static Anchor flipBothAnchors(Anchor anchor)
        {
            Anchor anchorX = getAnchorX(anchor);
            Anchor anchorY = getAnchorY(anchor);

            return flipAnchorX(anchorX) | flipAnchorY(anchorY);
        }

        private static Anchor getAnchorX(Anchor anchor)
        {
            if (anchor.HasFlagFast(Anchor.x0)) return Anchor.x0;

            if (anchor.HasFlagFast(Anchor.x1)) return Anchor.x1;

            if (anchor.HasFlagFast(Anchor.x2)) return Anchor.x2;

            throw new ArgumentOutOfRangeException();
        }

        private static Anchor getAnchorY(Anchor anchor)
        {
            if (anchor.HasFlagFast(Anchor.y0)) return Anchor.y0;

            if (anchor.HasFlagFast(Anchor.y1)) return Anchor.y1;

            if (anchor.HasFlagFast(Anchor.y2)) return Anchor.y2;

            throw new ArgumentOutOfRangeException();
        }

        private static Anchor flipAnchorX(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.x0: return Anchor.x2;

                case Anchor.x1: return Anchor.x1;

                case Anchor.x2: return Anchor.x0;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static Anchor flipAnchorY(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.y0: return Anchor.y2;

                case Anchor.y1: return Anchor.y1;

                case Anchor.y2: return Anchor.y0;

                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
