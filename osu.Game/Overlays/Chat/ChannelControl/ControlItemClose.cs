// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItemClose : OsuClickableContainer
    {
        private readonly SpriteIcon icon;

        [Resolved]
        private OsuColour osuColour { get; set; } = null!;

        public ControlItemClose()
        {
            Alpha = 0f;
            Size = new Vector2(20);
            Child = icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(0.75f),
                Icon = FontAwesome.Solid.TimesCircle,
                RelativeSizeAxes = Axes.Both,
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            icon.ScaleTo(0.5f, 1000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            icon.ScaleTo(0.75f, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.FadeColour(osuColour.Red1, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.FadeColour(Colour4.White, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
