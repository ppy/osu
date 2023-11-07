// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonHealthRightLine : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(50f, ArgonHealthDisplay.MAIN_PATH_RADIUS * 2);
            InternalChild = new Circle
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Width = 45f,
                Height = 3f,
            };
        }
    }
}
