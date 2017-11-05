// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : OsuClickableContainer, IHandleOnMouseDown, IHandleOnMouseUp, IHandleOnHoverLost
    {
        private readonly SpriteIcon icon;

        public DownloadButton()
        {
            Children = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(30),
                    Icon = FontAwesome.fa_osu_chevron_down_o,
                },
            };
        }

        public virtual bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            icon.ScaleTo(0.9f, 1000, Easing.Out);
            return false;
        }

        public virtual bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            icon.ScaleTo(1f, 500, Easing.OutElastic);
            return false;
        }

        public override bool OnHover(InputState state)
        {
            icon.ScaleTo(1.1f, 500, Easing.OutElastic);
            return base.OnHover(state);
        }

        public virtual void OnHoverLost(InputState state)
        {
            icon.ScaleTo(1f, 500, Easing.OutElastic);
        }
    }
}
