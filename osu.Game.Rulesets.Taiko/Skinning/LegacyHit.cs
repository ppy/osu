// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class LegacyHit : LegacyCirclePiece
    {
        private readonly TaikoSkinComponents component;

        public LegacyHit(TaikoSkinComponents component)
        {
            this.component = component;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AccentColour = component == TaikoSkinComponents.CentreHit
                ? new Color4(235, 69, 44, 255)
                : new Color4(67, 142, 172, 255);
        }
    }
}
