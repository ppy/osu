// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        public DrawableJuiceStream(JuiceStream s, Func<CatchHitObject, DrawableHitObject<CatchHitObject>> getVisualRepresentation = null)
            : base(s)
        {
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.BottomLeft;
            X = 0;

            InternalChild = dropletContainer = new Container { RelativeSizeAxes = Axes.Both, };

            foreach (var o in s.NestedHitObjects.Cast<CatchHitObject>())
                AddNested(getVisualRepresentation?.Invoke(o));
        }

        protected override bool ProvidesJudgement => false;

        protected override void AddNested(DrawableHitObject h)
        {
            var catchObject = (DrawableCatchHitObject)h;

            catchObject.CheckPosition = o => CheckPosition?.Invoke(o) ?? false;

            dropletContainer.Add(h);
            base.AddNested(h);
        }
    }
}
