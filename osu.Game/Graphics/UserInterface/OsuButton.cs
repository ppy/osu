// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with added default sound effects.
    /// </summary>
    public class OsuButton : Button
    {
        private Box hover;

        public OsuButton()
        {
            Height = 40;

            Content.Masking = true;
            Content.CornerRadius = 5;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.BlueDark;

            AddRange(new Drawable[]
            {
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Colour = Color4.White.Opacity(0.1f),
                    Alpha = 0,
                    Depth = -1
                },
                new HoverClickSounds(HoverSampleSet.Loud),
            });

            Enabled.ValueChanged += enabledChanged;
            Enabled.TriggerChange();
        }

        private void enabledChanged(ValueChangedEvent<bool> e)
        {
            this.FadeColour(e.NewValue ? Color4.White : Color4.Gray, 200, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeIn(200);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hover.FadeOut(200);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.9f, 4000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(e);
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = OsuFont.GetFont(weight: FontWeight.Bold)
        };
    }
}
