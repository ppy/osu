// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableAccuracyCounter : SkinnableDrawable
    {
        public SkinnableAccuracyCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.AccuracyCounter), _ => new DefaultAccuracyCounter())
        {
            CentreComponent = false;
        }
    }
}
