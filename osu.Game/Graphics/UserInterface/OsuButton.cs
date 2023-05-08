// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with added default sound effects.
    /// </summary>
    public abstract partial class OsuButton : Button
    {
        public LocalisableString Text
        {
            get => SpriteText.Text;
            set => SpriteText.Text = value;
        }

        private Color4? backgroundColour;

        /// <summary>
        /// Sets a custom background colour to this button, replacing the provided default.
        /// </summary>
        public virtual Color4 BackgroundColour
        {
            get => backgroundColour ?? defaultBackgroundColour;
            set
            {
                backgroundColour = value;
                Background.FadeColour(value);
            }
        }

        private Color4 defaultBackgroundColour;

        /// <summary>
        /// Sets a default background colour to this button.
        /// </summary>
        protected Color4 DefaultBackgroundColour
        {
            get => defaultBackgroundColour;
            set
            {
                defaultBackgroundColour = value;

                if (backgroundColour == null)
                    Background.FadeColour(value);
            }
        }

        protected override Container<Drawable> Content { get; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            // base call is checked for cases when `OsuClickableContainer` has masking applied to it directly (ie. externally in object initialisation).
            base.ReceivePositionalInputAt(screenSpacePos)
            // Implementations often apply masking / edge rounding at a content level, so it's imperative to check that as well.
            && Content.ReceivePositionalInputAt(screenSpacePos);

        protected Box Hover;
        protected Box Background;
        protected SpriteText SpriteText;

        private readonly Box flashLayer;

        protected OsuButton(HoverSampleSet? hoverSounds = HoverSampleSet.Button)
        {
            Height = 40;

            AddInternal(Content = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
                CornerRadius = 5,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Background = new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                    },
                    Hover = new Box
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Blending = BlendingParameters.Additive,
                        Depth = float.MinValue
                    },
                    SpriteText = CreateText(),
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive,
                        Depth = float.MinValue,
                        Colour = Color4.White.Opacity(0.5f),
                        Alpha = 0,
                    },
                }
            });

            if (hoverSounds.HasValue)
                AddInternal(new HoverClickSounds(hoverSounds.Value) { Enabled = { BindTarget = Enabled } });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DefaultBackgroundColour = colours.BlueDark;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Colour = dimColour;
            Enabled.BindValueChanged(_ => this.FadeColour(dimColour, 200, Easing.OutQuint));
        }

        private Color4 dimColour => Enabled.Value ? Color4.White : Color4.Gray;

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value)
                flashLayer.FadeOutFromOne(800, Easing.OutQuint);

            return base.OnClick(e);
        }

        protected virtual float HoverLayerFinalAlpha => 0.1f;

        protected override bool OnHover(HoverEvent e)
        {
            if (Enabled.Value)
            {
                Hover.FadeTo(0.2f, 40, Easing.OutQuint)
                     .Then()
                     .FadeTo(HoverLayerFinalAlpha, 800, Easing.OutQuint);
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            Hover.FadeOut(800, Easing.OutQuint);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.9f, 4000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        protected virtual SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = OsuFont.GetFont(weight: FontWeight.Bold)
        };
    }
}
