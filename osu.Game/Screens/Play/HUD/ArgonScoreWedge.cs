// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonScoreWedge : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new ArgonWedgePiece
                {
                    WedgeWidth = { Value = 380 },
                    WedgeHeight = { Value = 72 },
                },
                new ArgonWedgePiece
                {
                    WedgeWidth = { Value = 380 },
                    WedgeHeight = { Value = 72 },
                    Position = new Vector2(4, 5)
                },
            };
        }
    }
}
