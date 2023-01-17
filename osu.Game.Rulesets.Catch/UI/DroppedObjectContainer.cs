// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class DroppedObjectContainer : Container<CaughtObject>
    {
        public DroppedObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
