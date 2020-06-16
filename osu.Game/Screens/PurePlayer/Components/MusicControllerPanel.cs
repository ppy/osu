using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class MusicControllerPanel : Container
    {
        public Drawable d;
        public MusicControllerPanel()
        {
            CornerRadius = 12.5f;
            Masking = true;
        }

        protected override void LoadComplete()
        {
            if ( d != null )
                this.Add(d);
        }

        protected override bool OnHover(HoverEvent e)
        {
            d.FadeTo(1, 500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            d.FadeOut(500, Easing.OutQuint);
        }
    }
}