// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with added default sound effects.
    /// </summary>
    public class OsuButton : Button
    {
        public LocalisableString Text
        {
            get => SpriteText?.Text ?? default;
            set
            {
                if (SpriteText != null)
                    SpriteText.Text = value;
            }
        }

        private Color4? backgroundColour;

        public Color4 BackgroundColour
        {
            get => backgroundColour ?? Color4.White;
            set
            {
                backgroundColour = value;
                Background.FadeColour(value);
            }
        }

        protected override Container<Drawable> Content { get; }

        protected Box Hover;
        protected Box Background;
        protected SpriteText SpriteText;

        public OsuButton(HoverSampleSet? hoverSounds = HoverSampleSet.Button)
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
                    },
                    Hover = new Box
                    {
                        Alpha = 0,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(.1f),
                        Blending = BlendingParameters.Additive,
                        Depth = float.MinValue
                    },
                    SpriteText = CreateText(),
                }
            });

            if (hoverSounds.HasValue)
                AddInternal(new HoverClickSounds(hoverSounds.Value));
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (backgroundColour == null)
                BackgroundColour = colours.BlueDark;
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
            {
                Debug.Assert(backgroundColour != null);
                Background.FlashColour(backgroundColour.Value, 200);
            }

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (Enabled.Value)
                Hover.FadeIn(200, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            Hover.FadeOut(300);
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
