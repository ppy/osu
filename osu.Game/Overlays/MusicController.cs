//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays
{
    public class MusicController : OverlayContainer
    {
        public override void Load(BaseGame game)
        {
            base.Load(game);
            Width = 400;
            Height = 130;
            CornerRadius = 5;
            Masking = true;
        }

        //placeholder for toggling
        protected override void PopIn() => FadeIn(500);

        protected override void PopOut() => FadeOut(500);
    }
}
