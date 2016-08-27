//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Cached;
using osu.Framework.Graphics.Primitives;
using OpenTK;

namespace osu.Framework.Graphics.Containers
{
    public class AutoSizeContainer : Container
    {
        protected bool RequireAutoSize => autoSizeUpdatePending;

        private bool autoSizeUpdatePending;

        public override bool Invalidate(bool affectsSize = true, bool affectsPosition = true, Drawable source = null)
        {
            if (affectsSize)
                autoSizeUpdatePending = true;

            bool alreadyInvalidated = base.Invalidate(affectsSize, affectsPosition, source);

            return !alreadyInvalidated;
        }

        protected override Quad DrawQuadForBounds
        {
            get
            {
                Vector2 size = Vector2.Zero;

                Vector2 maxInheritingSize = Vector2.One;

                // Find the maximum width/height of children
                foreach (Drawable c in Children)
                {
                    if (!c.IsVisible)
                        continue;

                    Vector2 boundingSize = c.GetBoundingSize(this);
                    Vector2 inheritingSize = c.Size * c.VectorScale * c.Scale;

                    if ((c.SizeMode & InheritMode.X) == 0)
                        size.X = Math.Max(size.X, boundingSize.X);
                    else
                        maxInheritingSize.X = Math.Max(maxInheritingSize.X, inheritingSize.X);

                    if ((c.SizeMode & InheritMode.Y) == 0)
                        size.Y = Math.Max(size.Y, boundingSize.Y);
                    else
                        maxInheritingSize.Y = Math.Max(maxInheritingSize.Y, inheritingSize.Y);
                }

                return new Quad(0, 0, size.X * maxInheritingSize.X, size.Y * maxInheritingSize.Y);
            }
        }

        internal override void UpdateSubTree()
        {
            base.UpdateSubTree();

            if (RequireAutoSize)
            {
                Vector2 b = GetBoundingSize(this);
                if (!HasDefinedSize || b != Size)
                {
                    Size = b;

                    Invalidate();
                    UpdateDrawInfoSubtree();
                }

                autoSizeUpdatePending = false;
            }
        }

        internal override float InheritableWidth => HasDefinedSize ? ActualSize.X : Parent?.InheritableWidth ?? 0;
        internal override float InheritableHeight => HasDefinedSize ? ActualSize.Y : Parent?.InheritableHeight ?? 0;

        protected override bool HasDefinedSize => !autoSizeUpdatePending;

        protected override bool ChildrenShouldInvalidate => true;
    }
}
