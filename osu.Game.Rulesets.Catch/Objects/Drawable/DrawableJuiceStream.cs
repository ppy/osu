// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableJuiceStream : DrawableCatchHitObject<JuiceStream>
    {
        private readonly Func<CatchHitObject, DrawableHitObject<CatchHitObject>> createDrawableRepresentation;
        private readonly Container dropletContainer;

        public DrawableJuiceStream(JuiceStream s, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> createDrawableRepresentation = null)
            : base(s)
        {
            this.createDrawableRepresentation = createDrawableRepresentation;
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.BottomLeft;
            X = 0;

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
            dropletContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case CatchHitObject catchObject:
                    return createDrawableRepresentation?.Invoke(catchObject)?.With(o => ((DrawableCatchHitObject)o).CheckPosition = p => CheckPosition?.Invoke(p) ?? false);
            }

            return base.CreateNestedHitObject(hitObject);
        }
    }
}
