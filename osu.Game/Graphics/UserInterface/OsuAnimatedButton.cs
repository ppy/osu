// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics.Containers;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// Highlight on hover, bounce on click.
    /// </summary>
    public class OsuAnimatedButton : OsuClickableContainer
    {
        /// <summary>
        /// The colour that should be flashed when the <see cref="IconButton"/> is clicked.
        /// </summary>
        protected Color4 FlashColour = Color4.White.Opacity(0.3f);

        private Color4 hoverColour = Color4.White.Opacity(0.1f);

        /// <summary>
        /// The background colour of the <see cref="IconButton"/> while it is hovered.
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
                        Blending = BlendingMode.Additive,
                        Alpha = 0,
                    },
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Enabled.BindValueChanged(enabled => this.FadeColour(enabled ? Color4.White : colours.Gray9, 200, Easing.OutQuint), true);
        }

        protected override bool OnHover(InputState state)
        {
            hover.FadeIn(500, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hover.FadeOut(500, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            hover.FlashColour(FlashColour, 800, Easing.OutQuint);
            return base.OnClick(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Content.ScaleTo(0.75f, 2000, Easing.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }
    }
}
