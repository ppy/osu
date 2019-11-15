// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Highlight on hover, bounce on click.
    /// </summary>
    public class OsuAnimatedButton : OsuClickableContainer
    {
        /// <summary>
        /// The colour that should be flashed when the <see cref="OsuAnimatedButton"/> is clicked.
        /// </summary>
        protected Color4 FlashColour = Color4.White.Opacity(0.3f);

        private Color4 hoverColour = Color4.White.Opacity(0.1f);

        /// <summary>
        /// The background colour of the <see cref="OsuAnimatedButton"/> while it is hovered.
        /// </summary>
        protected Color4 HoverColour
        {
            get => hoverColour;
            set
            {
                hoverColour = value;
                hover.Colour = value;
            }
        }

        protected override Container<Drawable> Content => content;

        private readonly Container content;
        private readonly Box hover;

        public OsuAnimatedButton()
        {
            base.Content.Add(content = new Container
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = Color4.Black.Opacity(0.04f),
                    Type = EdgeEffectType.Shadow,
                    Radius = 5,
                },
                Children = new Drawable[]
                {
                    hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = HoverColour,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (AutoSizeAxes != Axes.None)
            {
                content.RelativeSizeAxes = (Axes.Both & ~AutoSizeAxes);
                content.AutoSizeAxes = AutoSizeAxes;
            }

            Enabled.BindValueChanged(enabled => this.FadeColour(enabled.NewValue ? Color4.White : colours.Gray9, 200, Easing.OutQuint), true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeIn(500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hover.FadeOut(500, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            hover.FlashColour(FlashColour, 800, Easing.OutQuint);
            return base.OnClick(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.75f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(e);
        }
    }
}
