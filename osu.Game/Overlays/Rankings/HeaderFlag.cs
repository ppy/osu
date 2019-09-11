// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Input.Events;
using osu.Framework.Extensions.Color4Extensions;
using System;

namespace osu.Game.Overlays.Rankings
{
    public class HeaderFlag : UpdateableFlag
    {
        private const int duration = 200;

        public Action Action;

        private readonly Container hoverContainer;

        public HeaderFlag()
        {
            AddInternal(hoverContainer = new Container
            {
                Alpha = 0,
                Depth = -1,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(15),
                        Icon = FontAwesome.Solid.Times,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(100),
                    }
                }
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverContainer.FadeIn(duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            hoverContainer.FadeOut(duration, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return base.OnClick(e);
        }
    }
}
