// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning.Select
{
    public partial class LegacyBackButton : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;

            bool old = skin.GetAnimation("menu-back", true, false) != null;

            if (old)
            {
                InternalChild = new LegacyOldBackButtonPiece
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                };
            }
            else
            {
                InternalChild = new LegacyNewBackButtonPiece
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                };
            }
        }
    }
}
