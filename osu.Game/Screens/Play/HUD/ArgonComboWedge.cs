// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonComboWedge : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new ArgonWedgePiece
            {
                WedgeWidth = { Value = 186 },
                WedgeHeight = { Value = 33 },
            };
        }
    }
}
