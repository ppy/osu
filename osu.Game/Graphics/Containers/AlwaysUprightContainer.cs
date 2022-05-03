// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    /// <summary>
    /// A container whose children stay upright, even as the container's parent transforms.
    /// </summary>
    public class AlwaysUprightContainer : Container
    {
        private float previousParentRotation;
        private Vector2 previousParentScale = new Vector2(1);

        protected override void Update()
        {
            base.Update();

            float parentRotation = Parent.Rotation;
            Vector2 parentScale = Parent.Scale;

            foreach (Drawable child in Children)
            {
                child.RotateTo(-parentRotation);

                int parentRotationInQuarterTurns = (int)Math.Floor(parentRotation / 90);
                if (parentRotationInQuarterTurns % 2 != 0)
                    child.ScaleTo(new Vector2(parentScale.Y, parentScale.X));
                else
                    child.ScaleTo(parentScale);

                int previousParentRotationInQuarterTurns = (int)Math.Floor(previousParentRotation / 90);
                int rotationsNeeded = parentRotationInQuarterTurns - previousParentRotationInQuarterTurns;
                if (rotationsNeeded < 0)
                    rotationsNeeded += 4;

                switch (rotationsNeeded)
                {
                    // Rotated by 90
                    case 1:
                        child.Origin = rotateAnchor90(child.Origin, true);
                        break;

                    // Rotated by 180
                    case 2:
                        child.Origin = flipAnchorXY(child.Origin);
                        break;

                    // Rotated by 270
                    case 3:
                        child.Origin = rotateAnchor90(child.Origin, false);
                        break;

                    default:
                        break;
                }

                if (Math.Sign(parentScale.X) != Math.Sign(previousParentScale.X))
                {
                    if (parentRotationInQuarterTurns % 2 != 0)
                        child.Origin = flipAnchorY(child.Origin);
                    else
                        child.Origin = flipAnchorX(child.Origin);
                }

                if (Math.Sign(parentScale.Y) != Math.Sign(previousParentScale.Y))
                {
                    if (parentRotationInQuarterTurns % 2 != 0)
                        child.Origin = flipAnchorX(child.Origin);
                    else
                        child.Origin = flipAnchorY(child.Origin);
                }
            }

            previousParentRotation = parentRotation;
            previousParentScale = parentScale;
        }

        private Anchor rotateAnchor90(Anchor anchor, bool clockwise)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    anchor = Anchor.TopRight;
                    break;

                case Anchor.TopCentre:
                    anchor = Anchor.CentreRight;
                    break;

                case Anchor.TopRight:
                    anchor = Anchor.BottomRight;
                    break;

                case Anchor.CentreLeft:
                    anchor = Anchor.TopCentre;
                    break;

                case Anchor.Centre:
                    anchor = Anchor.Centre;
                    break;

                case Anchor.CentreRight:
                    anchor = Anchor.BottomCentre;
                    break;

                case Anchor.BottomLeft:
                    anchor = Anchor.TopLeft;
                    break;

                case Anchor.BottomCentre:
                    anchor = Anchor.CentreLeft;
                    break;

                case Anchor.BottomRight:
                    anchor = Anchor.BottomLeft;
                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            if (!clockwise)
            {
                anchor = flipAnchorXY(anchor);
            }

            return anchor;
        }

        private Anchor flipAnchorXY(Anchor anchor)
        {
            anchor = flipAnchorX(anchor);
            anchor = flipAnchorY(anchor);

            return anchor;
        }

        private Anchor flipAnchorX(Anchor anchor)
        {
            Anchor anchorX = getAnchorX(anchor);
            Anchor anchorY = getAnchorY(anchor);

            Anchor flippedAnchorX;

            switch (anchorX)
            {
                case Anchor.x0:
                    flippedAnchorX = Anchor.x2;
                    break;

                case Anchor.x1:
                    flippedAnchorX = Anchor.x1;
                    break;

                case Anchor.x2:
                    flippedAnchorX = Anchor.x0;
                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            return flippedAnchorX | anchorY;
        }

        private Anchor flipAnchorY(Anchor anchor)
        {
            Anchor anchorX = getAnchorX(anchor);
            Anchor anchorY = getAnchorY(anchor);

            Anchor flippedAnchorY;

            switch (anchorY)
            {
                case Anchor.y0:
                    flippedAnchorY = Anchor.y2;
                    break;

                case Anchor.y1:
                    flippedAnchorY = Anchor.y1;
                    break;

                case Anchor.y2:
                    flippedAnchorY = Anchor.y0;
                    break;

                default: throw new ArgumentOutOfRangeException();
            }

            return anchorX | flippedAnchorY;
        }

        private Anchor getAnchorX(Anchor anchor)
        {
            if (anchor.HasFlagFast(Anchor.x0)) return Anchor.x0;

            if (anchor.HasFlagFast(Anchor.x1)) return Anchor.x1;

            if (anchor.HasFlagFast(Anchor.x2)) return Anchor.x2;

            throw new ArgumentOutOfRangeException();
        }

        private Anchor getAnchorY(Anchor anchor)
        {
            if (anchor.HasFlagFast(Anchor.y0)) return Anchor.y0;

            if (anchor.HasFlagFast(Anchor.y1)) return Anchor.y1;

            if (anchor.HasFlagFast(Anchor.y2)) return Anchor.y2;

            throw new ArgumentOutOfRangeException();
        }
    }
}
