// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableJuiceStream : DrawableCatchHitObject
    {
        private readonly Container dropletContainer;

        public DrawableJuiceStream()
            : this(null)
        {
        }

        public DrawableJuiceStream([CanBeNull] JuiceStream s)
            : base(s)
        {
            RelativeSizeAxes = Axes.X;
            Origin = Anchor.BottomLeft;

            AddInternal(dropletContainer = new Container { RelativeSizeAxes = Axes.Both, });
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);
            dropletContainer.Add(hitObject);
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            dropletContainer.Clear(false);
        }
    }
}
