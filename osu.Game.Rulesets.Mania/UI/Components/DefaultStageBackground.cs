// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.UI.Components
{
    public partial class DefaultStageBackground : CompositeDrawable
    {
        public DefaultStageBackground()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new Box
            {
                Name = "Background",
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black
            };
        }
    }
}
