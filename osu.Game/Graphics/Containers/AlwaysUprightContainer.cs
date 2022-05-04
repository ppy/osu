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
                }

                if (Math.Sign(parentScale.X) != Math.Sign(previousParentScale.X))
                {
                    child.Origin = (parentRotationInQuarterTurns % 2 != 0) ? flipAnchorY(child.Origin) : flipAnchorX(child.Origin);
                }

                if (Math.Sign(parentScale.Y) != Math.Sign(previousParentScale.Y))
                {
                    child.Origin = (parentRotationInQuarterTurns % 2 != 0) ? flipAnchorX(child.Origin) : flipAnchorY(child.Origin);
                }
            }

            previousParentRotation = parentRotation;
            previousParentScale = parentScale;
        }

        private Anchor rotateAnchor90(Anchor anchor, bool clockwise)
        {
            Anchor result = 0;

            if (anchor.HasFlagFast(Anchor.x0)) result |= Anchor.y0;
            if (anchor.HasFlagFast(Anchor.x1)) result |= Anchor.y1;
            if (anchor.HasFlagFast(Anchor.x2)) result |= Anchor.y2;

            if (anchor.HasFlagFast(Anchor.y0)) result |= Anchor.x2;
            if (anchor.HasFlagFast(Anchor.y1)) result |= Anchor.x1;
            if (anchor.HasFlagFast(Anchor.y2)) result |= Anchor.x0;

            return clockwise ? result : flipAnchorXY(result);
        }

        private Anchor flipAnchorXY(Anchor anchor)
        {
            anchor = flipAnchorX(anchor);
            anchor = flipAnchorY(anchor);

            return anchor;
        }

        private Anchor flipAnchorX(Anchor anchor) => anchor ^ (Anchor.x0 | Anchor.x2);

        private Anchor flipAnchorY(Anchor anchor) => anchor ^ (Anchor.y0 | Anchor.y2);
    }
}
