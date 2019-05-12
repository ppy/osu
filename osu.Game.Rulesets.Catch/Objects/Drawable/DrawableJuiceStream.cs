// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public class DrawableJuiceStream : DrawableCatchHitObject<JuiceStream>
    {
        private readonly Container dropletContainer;

        public DrawableJuiceStream(JuiceStream s, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> createDrawableRepresentation = null)
            : base(s)
        {
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.BottomLeft;
            X = 0;

            AddInternal(dropletContainer = new Container { RelativeSizeAxes = Axes.Both, });

            foreach (var o in s.NestedHitObjects.Cast<CatchHitObject>())
                AddNested(createDrawableRepresentation?.Invoke(o));
        }

        protected override void AddNested(DrawableHitObject h)
        {
            var catchObject = (DrawableCatchHitObject)h;

            catchObject.CheckPosition = o => CheckPosition?.Invoke(o) ?? false;

            dropletContainer.Add(h);
            base.AddNested(h);
        }
    }
}
