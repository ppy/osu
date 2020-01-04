// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Input.Events;
using System;

namespace osu.Game.Overlays.Rankings
{
    public class DismissableFlag : UpdateableFlag
    {
        private const int duration = 200;

        public Action Action;

        private readonly SpriteIcon hoverIcon;

        public DismissableFlag()
        {
            AddInternal(hoverIcon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Depth = -1,
                Alpha = 0,
                Size = new Vector2(10),
                Icon = FontAwesome.Solid.Times,
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverIcon.FadeIn(duration, Easing.OutQuint);
            this.FadeColour(Color4.Gray, duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            hoverIcon.FadeOut(duration, Easing.OutQuint);
            this.FadeColour(Color4.White, duration, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }
    }
}
