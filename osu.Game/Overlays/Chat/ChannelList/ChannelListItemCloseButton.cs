// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat.ChannelList
{
    public partial class ChannelListItemCloseButton : OsuClickableContainer
    {
        private SpriteIcon icon = null!;

        private Color4 normalColour;
        private Color4 hoveredColour;

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            normalColour = osuColour.Red2;
            hoveredColour = Color4.White;

            Alpha = 0f;
            Size = new Vector2(20);
            Add(icon = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.75f),
                Icon = FontAwesome.Solid.TimesCircle,
                RelativeSizeAxes = Axes.Both,
                Colour = normalColour,
            });
        }

        // Transforms matching OsuAnimatedButton
        protected override bool OnHover(HoverEvent e)
        {
            icon.FadeColour(hoveredColour, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            icon.FadeColour(normalColour, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            icon.ScaleTo(0.75f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            icon.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }
    }
}
