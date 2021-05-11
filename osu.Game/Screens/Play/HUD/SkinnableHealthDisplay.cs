// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableHealthDisplay : SkinnableDrawable
    {
        public SkinnableHealthDisplay()
            : base(new HUDSkinComponent(HUDSkinComponents.HealthDisplay), _ => new DefaultHealthDisplay())
        {
            CentreComponent = false;
        }
    }
}
