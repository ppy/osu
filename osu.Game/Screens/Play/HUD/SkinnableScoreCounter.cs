// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class SkinnableScoreCounter : SkinnableDrawable
    {
        public SkinnableScoreCounter()
            : base(new HUDSkinComponent(HUDSkinComponents.ScoreCounter), _ => new DefaultScoreCounter())
        {
            CentreComponent = false;
        }
    }
}
