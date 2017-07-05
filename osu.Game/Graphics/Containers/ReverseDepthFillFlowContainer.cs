// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Containers
{
    public class ReverseDepthFillFlowContainer<T> : FillFlowContainer<T> where T : Drawable
    {
        protected override IComparer<Drawable> DepthComparer => new ReverseCreationOrderDepthComparer();
        protected override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();
    }
}
